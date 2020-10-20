using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MiniPlayerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MusicLib musicLib;
        private MediaPlayer mediaPlayer;        

        public MainWindow()
        {
            InitializeComponent();

            mediaPlayer = new MediaPlayer();

            try
            {
                // Tell the lib where the exe resides
                musicLib = new MusicLib(); 
            }
            catch (Exception e)
            {
                DisplayError("Error loading file: " + e.Message);
                return;
            }

            //musicLib.PrintAllTables();
            
            // Put the ids to a ObservableCollection which has a Remove method for use later.
            // The UI will update itself automatically if any changes are made to this collection.
            ObservableCollection<string> items = new ObservableCollection<string>(musicLib.SongIds);     

            // Bind the song IDs to the combo box
            songIdComboBox.ItemsSource = items;
            
            // Select the first item
            if (songIdComboBox.Items.Count > 0)
            {
                songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                deleteButton.IsEnabled = true;
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.FileName = ""; 
            openFileDialog.DefaultExt = "*.wma;*.wav;*mp3";
            openFileDialog.Filter = "Media files|*.mp3;*.m4a;*.wma;*.wav|MP3 (*.mp3)|*.mp3|M4A (*.m4a)|*.m4a|Windows Media Audio (*.wma)|*.wma|Wave files (*.wav)|*.wav|All files|*.*";

            // Show open file dialog box
            bool? result = openFileDialog.ShowDialog();

            // Load the selected song
            if (result == true)
            {
                songIdComboBox.IsEnabled = false;
                Song s = GetSongDetails(openFileDialog.FileName);
                if (s != null)
                {
                    titleTextBox.Text = s.Title;
                    artistTextBox.Text = s.Artist;
                    albumTextBox.Text = s.Album;
                    genreTextBox.Text = s.Genre;
                    lengthTextBox.Text = s.Length;
                    filenameTextBox.Text = s.Filename;
                    mediaPlayer.Open(new Uri(s.Filename));
                    addButton.IsEnabled = true;
                }
            }
        }

        private Song GetSongDetails(string filename)
        {
            Song s = null;

            try
            {
                // PM> Install-Package taglib
                // http://stackoverflow.com/questions/1750464/how-to-read-and-write-id3-tags-to-an-mp3-in-c
                TagLib.File file = TagLib.File.Create(filename);

                s = new Song
                {
                    Title = file.Tag.Title,
                    Artist = file.Tag.AlbumArtists.Length > 0 ? file.Tag.AlbumArtists[0] : "",
                    Album = file.Tag.Album,
                    Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "",
                    Length = file.Properties.Duration.Minutes + ":" + file.Properties.Duration.Seconds,
                    Filename = filename
                };

                return s;
            }
            catch (TagLib.UnsupportedFormatException)
            {
                DisplayError("You did not select a valid song file.");
            }
            catch (Exception ex)
            {
                DisplayError(ex.Message);                
            }
                
            return s;            
        }

        private void DisplayError(string errorMessage)
        {
            MessageBox.Show(errorMessage, "MiniPlayer", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Adding song");

            // Add the selected file to the music library
            Song s = new Song
            {
                Title = titleTextBox.Text,
                Artist = artistTextBox.Text,
                Album = albumTextBox.Text,
                Genre = genreTextBox.Text,
                Length = lengthTextBox.Text,
                Filename = filenameTextBox.Text
            };

            string id = musicLib.AddSong(s).ToString();
            Console.WriteLine("id = " + id);

            // Add the song ID to the combo box
            songIdComboBox.IsEnabled = true;            
            (songIdComboBox.ItemsSource as ObservableCollection<string>).Add(id);
            songIdComboBox.SelectedIndex = songIdComboBox.Items.Count - 1;

            // There is at least one song that can be deleted
            deleteButton.IsEnabled = true;
        }

        private void updateButton_Click(object sender, RoutedEventArgs e)
        {
            string songId = songIdComboBox.SelectedItem.ToString();
            Console.WriteLine("Updating song " + songId);
               
            Song s = new Song        
            {
                Title = titleTextBox.Text,
                Artist = artistTextBox.Text,
                Album = albumTextBox.Text,
                Genre = genreTextBox.Text,
                Length = lengthTextBox.Text,
                Filename = filenameTextBox.Text
            };

            if (musicLib.UpdateSong(Convert.ToInt32(songId), s))
            {
                updateButton.IsEnabled = false;
            }
            else
            {
                DisplayError("There was a problem updating this song.");
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this song?", "MiniPlayer", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string songId = songIdComboBox.SelectedItem.ToString();
                Console.WriteLine("Deleting song " + songId);

                if (musicLib.DeleteSong(Convert.ToInt32(songId)))
                {
                    // Remove the song from the list box and select the next item
                    (songIdComboBox.ItemsSource as ObservableCollection<string>).Remove(
                        songIdComboBox.SelectedItem.ToString());
                    if (songIdComboBox.Items.Count > 0)
                        songIdComboBox.SelectedItem = songIdComboBox.Items[0];
                    else
                    {
                        // No more songs to display
                        deleteButton.IsEnabled = false;
                        titleTextBox.Text = "";
                        artistTextBox.Text = "";
                        albumTextBox.Text = "";
                        genreTextBox.Text = "";
                        lengthTextBox.Text = "";
                        filenameTextBox.Text = "";
                    }
                }
                else
                    DisplayError("There was a problem deleting this song.");                
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
        }

        private void showDataButton_Click(object sender, RoutedEventArgs e)
        {
            musicLib.PrintAllTables();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            musicLib.Save();
        }

        private void songIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Display the selected song
            if (songIdComboBox.SelectedItem != null)
            {
                Console.WriteLine("Load song " + songIdComboBox.SelectedItem);
                int songId = Convert.ToInt32(songIdComboBox.SelectedItem);
                Song s = musicLib.GetSong(songId);
                titleTextBox.Text = s.Title;
                artistTextBox.Text = s.Artist;
                albumTextBox.Text = s.Album;
                genreTextBox.Text = s.Genre;
                lengthTextBox.Text = s.Length;
                filenameTextBox.Text = s.Filename;
                mediaPlayer.Open(new Uri(filenameTextBox.Text));
                updateButton.IsEnabled = false;
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateButton.IsEnabled = true;
        }
    }
}

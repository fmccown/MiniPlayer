using MiniPlayerWpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Appium.Windows;
using System;
using OpenQA.Selenium.Appium;
using System.IO;

namespace MusicLibTests
{
    [TestClass]
    public class MusicLibUnitTest
    {
        // Note: Id is next Id that is auto-incremented 
        private Song defaultSong = new Song
        {
            Id = 9,
            Artist = "Bob",
            Album = "Fire",
            Filename = "test.mp3",
            Genre = "cool",
            Length = "123",
            Title = "Best Song"
        };

        [TestMethod]
        public void TestSongIds()
        {
            MusicLib musicLib = new MusicLib();
            var songIds = new List<string>(musicLib.SongIds);

            // Make sure there are 7 IDs
            Assert.AreEqual(7, songIds.Count);

            // Make sure all IDs but 4 are present
            int idNum = 1;
            foreach (string id in songIds)
            {
                // There is no ID 4
                if (idNum == 4)
                    idNum++;

                Assert.AreEqual(idNum.ToString(), id);
                idNum++;
            }
        }

        [TestMethod]
        public void AddSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // ID auto-increments
            int expectedId = 9;
            int actualId = musicLib.AddSong(defaultSong);
            Assert.AreEqual(expectedId, actualId, "ID of added song was unexpectedly " + actualId);

            // See if we can get back the song that was just added
            Song song = musicLib.GetSong(expectedId);
            Assert.AreEqual(defaultSong, song, "Got back unexpected song: " + song);
        }

        [TestMethod]
        public void DeleteSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Delete a song that already exists
            int songId = 8;
            bool songDeleted = musicLib.DeleteSong(songId);
            Assert.IsTrue(songDeleted, "Song should have been deleted");

            // Verify the song is not in the library anymore
            Song s = musicLib.GetSong(songId);
            Assert.IsNull(s, "Returned song should be null because it doesn't exist");
        }

        [TestMethod]
        public void DeleteMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Attempt to delete a song that does not exist
            int songId = 111;
            bool songDeleted = musicLib.DeleteSong(songId);
            Assert.IsFalse(songDeleted, "Non-existing song should not have been deleted");
        }

        [TestMethod]
        public void GetSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Add the default song and then retrieve it
            int songId = musicLib.AddSong(defaultSong);
            Song song = musicLib.GetSong(songId);
            Assert.AreEqual(defaultSong, song);            
        }

        [TestMethod]
        public void GetMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Get a song that doesn't exist
            int songId = 111;
            Song song = musicLib.GetSong(songId);
            Assert.IsNull(song);
        }

        [TestMethod]
        public void UpdateSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Update everything except the ID
            int songId = musicLib.AddSong(defaultSong);
            Song updatedSong = new Song
            {
                Id = songId,
                Title = "Some title",
                Album = "Some album",
                Artist = "Some artist",
                Filename = "Some filename",
                Genre = "Some genre",
                Length = "Some length"
            };

            bool isUpdated = musicLib.UpdateSong(songId, updatedSong);
            Assert.IsTrue(isUpdated, "Update should have worked; got back " + isUpdated);

            // Make sure the song's title was really changed
            Song s = musicLib.GetSong(songId);
            Assert.AreEqual(updatedSong, s, "Song not updated properly");
        }

        [TestMethod]
        public void UpdateMissingSongTest()
        {
            MusicLib musicLib = new MusicLib();

            // Try to update a song with a bad ID
            int songId = 111;
            bool isUpdated = musicLib.UpdateSong(songId, defaultSong);
            Assert.IsFalse(isUpdated, "Update should NOT have worked; got back " + isUpdated);
        }

        [TestMethod]
        public void TestAlarm()
        {
            // Launch the Alarms & Clock app
            //DesiredCapabilities appCapabilities = new DesiredCapabilities();
            //appCapabilities.SetCapability("app", "Microsoft.WindowsAlarms_8wekyb3d8bbwe!App");
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", "Microsoft.WindowsAlarms_8wekyb3d8bbwe!App");
            var alarmClockSession = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            // Use the session to control the app
            alarmClockSession.FindElementByAccessibilityId("AddAlarmButton").Click();
            alarmClockSession.FindElementByAccessibilityId("AlarmNameTextBox").Click();
            alarmClockSession.FindElementByAccessibilityId("AlarmNameTextBox").Clear();
            alarmClockSession.FindElementByAccessibilityId("AlarmNameTextBox").SendKeys("Test alarm");
        }

        [TestMethod]
        public void TestUi()
        {
            // Launch the Alarms & Clock app
            //DesiredCapabilities appCapabilities = new DesiredCapabilities();
            //appCapabilities.SetCapability("app", "Microsoft.WindowsAlarms_8wekyb3d8bbwe!App");
            string currentDir = @"C:\Users\fmccown\Documents\Visual Studio 2019\Projects\MiniPlayerImprovedWpf\MiniPlayerWpf\bin\Debug\"; 
            string appPath = currentDir + @"\MiniPlayerWpf.exe";

            //Directory.SetCurrentDirectory(currentDir);

            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", appPath);
            options.AddAdditionalCapability("appWorkingDir", currentDir);
            var session = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            // Not sure how to select specific song
            //session.FindElementByAccessibilityId("songIdComboBox").Click();
            session.FindElementByAccessibilityId("titleTextBox").Clear();
            session.FindElementByAccessibilityId("titleTextBox").SendKeys("My Title");
            session.FindElementByAccessibilityId("addButton").Click();

            var songId = session.FindElementByAccessibilityId("songIdComboBox").Text;
            Assert.AreEqual(songId, "10");
        }

    }
}

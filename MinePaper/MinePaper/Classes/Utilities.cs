using System;
using System.Net;
using Windows.Storage;
using Windows.System.UserProfile;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace MinePaper.Classes
{
    public class Utilities
    {
        public static async void SetDesktopBackground(string filename)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
            UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
            await settings.TrySetWallpaperImageAsync(file);
        }

        public static async void SetLockScreenBackground(string filename)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
            UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
            await settings.TrySetLockScreenImageAsync(file);
        }

        public static void WriteSettingsToDisk(Settings settings)
        { 
            string json = JsonConvert.SerializeObject(settings);
            string filename = ApplicationData.Current.LocalFolder.Path + "/" + Constants.CONFIG_FILE_NAME;
            File.WriteAllText(filename, json);
        }

        public static Settings ReadSettingsFromDisk()
        { 
            Settings output = new Settings
            {
                IsAutoRotating = false,
                CurrentImage = null,
                AvailableImages = new List<string> { },
                AutoRotateMinutes = 30,
                LastRotatedTime = DateTime.MinValue,
                LastImageSyncedTime = DateTime.MinValue
            };
            string filename = ApplicationData.Current.LocalFolder.Path + "/" + Constants.CONFIG_FILE_NAME;

            if (File.Exists(filename))
            {
                using (StreamReader file = File.OpenText(filename))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    output = (Settings)serializer.Deserialize(file, typeof(Settings));
                }
            }
            return output;
        }

        public static void DownloadImageFromServer(string filename)
        {
            string uri = Constants.REMOTE_IMAGES_FOLDER + "/" + filename;
            string directory = ApplicationData.Current.LocalFolder.Path + "/images/" + filename;
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(new Uri(uri), directory);
            }
        }
    }
}

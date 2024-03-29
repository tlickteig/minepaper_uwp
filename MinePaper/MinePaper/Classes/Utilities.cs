﻿using System;
using System.Net;
using Windows.Storage;
using Windows.System.UserProfile;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Background;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace MinePaper.Classes
{
    public class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest lWebRequest = base.GetWebRequest(uri);
            lWebRequest.Timeout = Timeout;
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
            return lWebRequest;
        }
    }

    public class Utilities
    {
        public static async void SetDesktopBackground(string filename)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("images\\" + filename);
                UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
                await settings.TrySetWallpaperImageAsync(file);

                Settings appSettings = ReadSettingsFromDisk();
                appSettings.CurrentDesktopImage = filename;
                WriteSettingsToDisk(appSettings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async void SetLockScreenBackground(string filename)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("images\\" + filename);
                UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
                await settings.TrySetLockScreenImageAsync(file);

                Settings appSettings = ReadSettingsFromDisk();
                appSettings.CurrentLockScreenImage = filename;
                WriteSettingsToDisk(appSettings);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void WriteSettingsToDisk(Settings settings)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings);
                string filename = ApplicationData.Current.LocalFolder.Path + "/" + Constants.CONFIG_FILE_NAME;
                File.WriteAllText(filename, json);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Settings ReadSettingsFromDisk()
        { 
            Settings output = new Settings
            {
                IsDesktopRotating = false,
                IsLockScreenRotating = false,
                CurrentDesktopImage = null,
                CurrentLockScreenImage = null,
                AvailableImages = new List<string> { },
                DesktopAutoRotateMinutes = 30,
                LockScreenAutoRotateMinutes = 30,
                DesktopLastRotatedTime = DateTime.Now,
                LockScreenLastRotatedTime = DateTime.Now,
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

            if (output.AvailableImages is null)
            { 
                output.AvailableImages = new List<string>();
            }
            return output;
        }

        public static async Task SyncImagesWithServer(Action OnCompleted = null, RadialProgressBar desktopProgressRing = null, RadialProgressBar lockScreenProgressRing = null)
        {
            try
            {
                if (!Directory.Exists(ApplicationData.Current.LocalFolder.Path + "/images/"))
                {
                    Directory.CreateDirectory(ApplicationData.Current.LocalFolder.Path + "/images/");
                }

                Random random = new Random();
                Settings settings = ReadSettingsFromDisk();
                settings.AvailableImages = ScanImagesDirectory();
                List<string> serverFileList = await GetImageListFromServer();

                if (serverFileList?.Count > 0)
                {
                    List<string> tempLocalFileList = settings.AvailableImages.ToList();
                    foreach (string filename in settings.AvailableImages)
                    {
                        if (!serverFileList.Contains(filename))
                        {
                            DeleteImageFromDisk(filename);
                        }
                    }
                    settings.AvailableImages = ScanImagesDirectory();

                    while (settings.AvailableImages.Count > Constants.MAX_IMAGES)
                    {
                        string imageToDelete = settings.AvailableImages[random.Next(settings.AvailableImages.Count)];
                        DeleteImageFromDisk(imageToDelete);
                        settings.AvailableImages.Remove(imageToDelete);
                    }
                    settings.AvailableImages = ScanImagesDirectory();

                    int numImagesToDownload = random.Next(Constants.MIN_IMAGES_TO_DOWNLOAD, Constants.MAX_IMAGES_TO_DOWNLOAD);
                    int imagesDownloaded = 0;
                    int tries = 0;

                    if (desktopProgressRing != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                            () =>
                            {
                                desktopProgressRing.Minimum = 0;
                                desktopProgressRing.Maximum = numImagesToDownload;
                            });
                    }

                    if (lockScreenProgressRing != null)
                    {
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                            () =>
                            {
                                lockScreenProgressRing.Minimum = 0;
                                lockScreenProgressRing.Maximum = numImagesToDownload;
                            });
                    }

                    for (int maxLoops = 0; maxLoops < 1_000_000 && imagesDownloaded < numImagesToDownload; maxLoops++)
                    {
                        string imageToDownload = serverFileList[random.Next(serverFileList.Count)];
                        if (!settings.AvailableImages.Contains(imageToDownload))
                        {
                            if (settings.AvailableImages.Count >= Constants.MAX_IMAGES)
                            {
                                string imageToRemove = settings.AvailableImages[random.Next(settings.AvailableImages.Count)];
                                DeleteImageFromDisk(imageToRemove);
                                settings.AvailableImages.Remove(imageToRemove);
                            }

                            try
                            {
                                DownloadImageFromServer(imageToDownload);
                                settings.AvailableImages.Add(imageToDownload);
                                imagesDownloaded++;

                                if (desktopProgressRing != null)
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                    () =>
                                    {
                                        desktopProgressRing.Value = imagesDownloaded;
                                    });
                                }

                                if (lockScreenProgressRing != null)
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                                    () =>
                                    {
                                        lockScreenProgressRing.Value = imagesDownloaded;
                                    });
                                }
                            }
                            catch (WebException ex)
                            {
                                LogError(ex);
                                if (ex.Message.Contains("404"))
                                {
                                    if (settings.AvailableImages.Contains(imageToDownload))
                                    {
                                        settings.AvailableImages.Remove(imageToDownload);
                                    }

                                    if (serverFileList.Contains(imageToDownload))
                                    {
                                        serverFileList.Remove(imageToDownload);
                                    }
                                }
                                else if (tries < Constants.MAX_TRIES)
                                {
                                    tries++;
                                }
                                else
                                {
                                    throw ex;
                                }
                            }
                        }
                    }

                    settings.AvailableImages = ScanImagesDirectory();
                    settings.LastImageSyncedTime = DateTime.Now;
                    WriteSettingsToDisk(settings);
                }

                if (OnCompleted != null)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        OnCompleted();
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DeleteImageFromDisk(string filename)
        {
            try
            {
                string directory = ApplicationData.Current.LocalFolder.Path + "/images/" + filename;
                File.Delete(directory);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DownloadImageFromServer(string filename)
        {
            try
            {
                string uri = Constants.REMOTE_IMAGES_FOLDER + "/" + filename;
                string directory = ApplicationData.Current.LocalFolder.Path + "/images/" + filename;
                using (WebClient client = new WebClient())
                {
                    client.Timeout = 10000;
                    client.DownloadFile(new Uri(uri), directory);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<string> ScanImagesDirectory()
        {
            try
            {
                string directory = ApplicationData.Current.LocalFolder.Path + "\\images";
                List<string> images = Directory.GetFiles(directory).ToList();
                List<string> output = new List<string>();

                foreach (string image in images)
                {
                    if (image.EndsWith(".jpg") || image.EndsWith(".jpeg") || image.EndsWith(".png"))
                    {
                        output.Add(image.Replace(directory, string.Empty).Replace("\\", string.Empty));
                    }
                }
                return output;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<List<string>> GetImageListFromServer()
        {
            try
            {
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 5);

                string response = await client.GetStringAsync(Constants.REMOTE_IMAGE_LIST_ENDPOINT);
                Dictionary<string, List<string>> jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(response, serializerSettings);
                List<string> result = new List<string>();

                if (jsonResponse.ContainsKey("files"))
                {
                    result = jsonResponse["files"];
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // Copied from https://stackoverflow.com/questions/21307789/how-to-save-exception-in-txt-file
        public static void LogError(Exception ex)
        {
            string filePath = ApplicationData.Current.LocalFolder.Path + "\\errors.log";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine();

                while (ex != null)
                {
                    writer.WriteLine(ex.GetType().FullName);
                    writer.WriteLine("Message : " + ex.Message);
                    writer.WriteLine("StackTrace : " + ex.StackTrace);

                    ex = ex.InnerException;
                }
            }
        }

        public static void LogData(string message)
        {
            string filePath = ApplicationData.Current.LocalFolder.Path + "\\info.log";
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine("-----------------------------------------------------------------------------");
                writer.WriteLine("Date : " + DateTime.Now.ToString());
                writer.WriteLine("Message : " + message);
            }
        }

        public static async void ShowSimpleErrorDialogAsync(string message = "An error has occurred. Please try again later.") 
        {
            ContentDialog dialog = new ContentDialog()
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "Ok"
            };
            await dialog.ShowAsync();
        }

        public static async void RegisterBackgroundTaskIfNotRegisteredAlready()
        {
            var request = await BackgroundExecutionManager.RequestAccessAsync();
            string taskName = "MinePaperBackgroundTask";
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                    break;
                }
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.SetTrigger(new TimeTrigger(15, false));
            BackgroundTaskRegistration register = builder.Register();
        }
    }
}

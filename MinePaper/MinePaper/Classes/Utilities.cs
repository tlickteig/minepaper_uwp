using System;
using Windows.Storage;
using Windows.System.UserProfile;

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
    }
}

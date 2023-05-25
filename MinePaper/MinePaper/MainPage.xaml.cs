using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.UserProfile;
using Windows.Storage;

namespace MinePaper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }        

        private async void Event_TryMeClicked(object sender, RoutedEventArgs e)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                string filename = "HD_Wallpapers_1080p_Widescreen.jpeg";
                var localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                UserProfilePersonalizationSettings settings = UserProfilePersonalizationSettings.Current;
                await settings.TrySetWallpaperImageAsync(file);
            }
        }
    }
}

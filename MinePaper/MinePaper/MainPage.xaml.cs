using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.UserProfile;
using Windows.Storage;
using MinePaper.Classes;

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
            Utilities.SetLockScreenBackground("HD_Wallpapers_1080p_Widescreen.jpeg");
        }
    }
}

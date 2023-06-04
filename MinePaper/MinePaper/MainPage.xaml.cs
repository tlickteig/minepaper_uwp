using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.UserProfile;
using Windows.Storage;
using MinePaper.Classes;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using System.Threading.Tasks;

namespace MinePaper
{
    public class RotateOption
    { 
        public string Option { get; set; }

        public int Value { get; set; }

        public override string ToString()
        {
            return Option;
        }
    }

    public class WallpaperOption
    { 
        public string ImageName { get; set; }

        public string FullImagePath { get 
            {
                return ApplicationData.Current.LocalFolder.Path + "/images/" + ImageName;
            } 
        }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Settings _settings = new Settings();

        public List<RotateOption> _rotateOptions = new List<RotateOption>()
        {
            new RotateOption() { Option = "30 Minutes", Value = 30 },
            new RotateOption() { Option = "1 Hour", Value = 60 },
            new RotateOption() { Option = "2 Hours", Value = 120 },
            new RotateOption() { Option = "5 Hours", Value = 300 },
            new RotateOption() { Option = "12 Hours", Value = 720 },
            new RotateOption() { Option = "1 Day", Value = 1440 },
        };

        public List<WallpaperOption> _availableImages = new List<WallpaperOption>();

        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                Utilities.RegisterBackgroundTaskIfNotRegisteredAlready();
                vwMainNavigationView.SelectedItem = vwiDesktopItem;
                _settings = Utilities.ReadSettingsFromDisk();

                tglDesktopAutoRotate.IsOn = _settings.IsDesktopRotating;
                if (_settings.IsDesktopRotating)
                {
                    stkDesktopAutoRotateSection.Visibility = Visibility.Visible;
                }
                else
                {
                    stkDesktopAutoRotateSection.Visibility = Visibility.Collapsed;
                }

                tglLockScreenAutoRotate.IsOn = _settings.IsLockScreenRotating;
                if (_settings.IsLockScreenRotating)
                {
                    stkLockScreenAutoRotateSection.Visibility = Visibility.Visible;
                }
                else
                {
                    stkLockScreenAutoRotateSection.Visibility = Visibility.Collapsed;
                }

                cboDesktopRotateFrequency.SelectedItem = _rotateOptions[0];
            }
            catch (Exception ex)
            { 
                Utilities.LogError(ex);
                Utilities.ShowSimpleErrorDialogAsync();
            }

            try
            {
                RotateOption currentDesktopRotateFrequency = _rotateOptions.First(x => x.Value == _settings.DesktopAutoRotateMinutes);
                cboDesktopRotateFrequency.SelectedItem = currentDesktopRotateFrequency;

                RotateOption currentLockScreenRotateFrequency = _rotateOptions.First(x => x.Value == _settings.LockScreenAutoRotateMinutes);
                cboLockScreenRotateFrequency.SelectedItem = currentLockScreenRotateFrequency;
            }
            catch (InvalidOperationException ex)
            { 
                // Eat the exception because it can happen if there is a bad value in the settings file
            }

            try
            {
                List<string> imageNames = Utilities.ScanImagesDirectory();
                if (imageNames?.Count == 0)
                {
                    Utilities.SyncImagesWithServer(RefreshImageList);
                }

                RefreshImageList();
            }
            catch (Exception ex) 
            {
                Utilities.LogError(ex);
                Utilities.ShowSimpleErrorDialogAsync();
            }
        }

        private void vwMainNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (sender.SelectedItem == vwiLockScreenItem)
            {
                stkDesktopSection.Visibility = Visibility.Collapsed;
                stkLockScreenSection.Visibility = Visibility.Visible;
            }
            else
            {
                stkDesktopSection.Visibility = Visibility.Visible;
                stkLockScreenSection.Visibility = Visibility.Collapsed;
            }
        }

        #region Desktop event handlers
        private void tglDesktopAutoRotate_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = (ToggleSwitch)sender;
            _settings.IsDesktopRotating = toggleSwitch.IsOn;
            Utilities.WriteSettingsToDisk(_settings);

            if (toggleSwitch.IsOn)
            {
                stkDesktopAutoRotateSection.Visibility = Visibility.Visible;
            }
            else
            {
                stkDesktopAutoRotateSection.Visibility = Visibility.Collapsed;
            }
        }

        private void cboDesktopRotateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RotateOption selectedItem = (RotateOption)cboDesktopRotateFrequency.SelectedItem;
            _settings.DesktopAutoRotateMinutes = selectedItem.Value;
            Utilities.WriteSettingsToDisk(_settings);
        }

        private void lstDesktopWallpaperList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            WallpaperOption option = (WallpaperOption)listView.SelectedItem;
            SelectDesktopImage(option.ImageName);
        }

        private void btnSetDesktopWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var option = lstDesktopWallpaperList.SelectedItem as WallpaperOption;
                try
                {
                    Utilities.SetDesktopBackground("images\\" + option.ImageName);
                }
                catch (Exception ex)
                {
                    Utilities.LogError(ex);
                }
            }
            else
            {
                Utilities.ShowSimpleErrorDialogAsync("Profile personalization is not enabled. Please try again later.");
            }
        }
        #endregion

        #region Lock Screen event handlers
        private void tglLockScreenAutoRotate_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = (ToggleSwitch)sender;
            _settings.IsLockScreenRotating = toggleSwitch.IsOn;
            Utilities.WriteSettingsToDisk(_settings);

            if (toggleSwitch.IsOn)
            {
                stkLockScreenAutoRotateSection.Visibility = Visibility.Visible;
            }
            else
            {
                stkLockScreenAutoRotateSection.Visibility = Visibility.Collapsed;
            }
        }

        private void cboLockScreenRotateFrequency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RotateOption selectedItem = (RotateOption)cboLockScreenRotateFrequency.SelectedItem;
            _settings.LockScreenAutoRotateMinutes = selectedItem.Value;
            Utilities.WriteSettingsToDisk(_settings);
        }

        private void lstLockScreenWallpaperList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            WallpaperOption option = (WallpaperOption)listView.SelectedItem;
            SelectLockScreenImage(option.ImageName);
        }

        private void btnSetLockScreenWallpaper_Click(object sender, RoutedEventArgs e)
        {
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                var option = lstLockScreenWallpaperList.SelectedItem as WallpaperOption;
                try
                {
                    Utilities.SetLockScreenBackground("images\\" + option.ImageName);
                }
                catch (Exception ex)
                {
                    Utilities.LogError(ex);
                }
            }
            else
            {
                Utilities.ShowSimpleErrorDialogAsync("Profile personalization is not enabled. Please try again later.");
            }
        }
        #endregion

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double height = ((Frame)Window.Current.Content).ActualHeight - 150;
            double width = ((Frame)Window.Current.Content).ActualWidth - 150;

            lstDesktopWallpaperList.Height = height;
            lstLockScreenWallpaperList.Height = height;

            double imageFrameWidth = width - 100;
            double imageFrameHeight = height - 50;
            if (height > (width * 9.0 / 16.0))
            {
                imageFrameHeight = width * (9.0 / 16.0) - 100;
            }
            else if ((width * 9.0 / 16.0) > height)
            {
                imageFrameWidth = height * (16.0 / 9.0) - 50;
            }

            frmMainImage.Height = imageFrameHeight;
            frmMainImage.Width = imageFrameWidth;

            frmLockScreenImage.Height = imageFrameHeight;
            frmLockScreenImage.Width = imageFrameWidth;
        }

        private void SelectDesktopImage(string filename)
        {
            string fullImagePath = ApplicationData.Current.LocalFolder.Path + "/images/" + filename;
            BitmapImage image = new BitmapImage(new Uri(fullImagePath));
            imgMainImage.Source = image;
            txtSelectDesktopImage.Visibility = Visibility.Collapsed;
            btnSetWallpaper.IsEnabled = true;
        }

        private void SelectLockScreenImage(string filename)
        {
            string fullImagePath = ApplicationData.Current.LocalFolder.Path + "/images/" + filename;
            BitmapImage image = new BitmapImage(new Uri(fullImagePath));
            imgLockScreenImage.Source = image;
            txtSelectLockScreenImage.Visibility = Visibility.Collapsed;
            btnSetLockScreen.IsEnabled = true;
        }

        private void RefreshImageList()
        {
            List<string> imageNames = Utilities.ScanImagesDirectory();
            foreach (string imageName in imageNames)
            {
                _availableImages.Add(new WallpaperOption() { ImageName = imageName });
            }

            foreach (WallpaperOption image in _availableImages) 
            { 
                lstDesktopWallpaperList.Items.Add(image);
                lstLockScreenWallpaperList.Items.Add(image);
            }
        }
    }
}

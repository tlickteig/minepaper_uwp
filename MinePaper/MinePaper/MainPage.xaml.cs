using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.System.UserProfile;
using Windows.Storage;
using MinePaper.Classes;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;

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

            cboDesktopRotateFrequency.SelectedItem = _rotateOptions[0];
            try
            {
                RotateOption currentDesktopRotateFrequency = _rotateOptions.First(x => x.Value == _settings.DesktopAutoRotateMinutes);
                cboDesktopRotateFrequency.SelectedItem = currentDesktopRotateFrequency;
            }
            catch (InvalidOperationException ex)
            { 
                // Eat the exception because it can happen if there is a bad value in the settings file
            }

            try
            {
                List<string> imageNames = Utilities.ScanImagesDirectory();
                foreach (string imageName in imageNames) 
                {
                    _availableImages.Add(new WallpaperOption() { ImageName = imageName });
                }
                lstDesktopWallpaperList.ItemsSource = _availableImages;
            }
            catch (Exception ex) 
            {
                throw ex;
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

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lstDesktopWallpaperList.Height = ((Frame)Window.Current.Content).ActualHeight - 150;
        }
    }
}

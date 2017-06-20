using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Color_Architect.CustomCode;
using Windows.UI;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.DataTransfer;

namespace Color_Architect
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            colorFetch.IsChecked = true;
            setGradientColorMix();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) //page loading
        {
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) //bye bye time
        {
            var saveStuffs = ApplicationData.Current.RoamingSettings;
            // saveStuffs.Values["PaleteColor1"] = (SolidColorBrush)paletteColor1.Fill;
        }

        private void colorFetch_Checked(object sender, RoutedEventArgs e)
        {
            defaults.option = 1;
            setWindow();
            titleContent.Text = "Color Fetch";
            colorPick.IsChecked = false;
            colorMix.IsChecked = false;
            colorFetch.IsEnabled = false;
        }

        private void colorPick_Checked(object sender, RoutedEventArgs e)
        {
            defaults.option = 2;
            setWindow();
            titleContent.Text = "Color Pick";
            colorFetch.IsChecked = false;
            colorMix.IsChecked = false;
            colorPick.IsEnabled = false;
        }

        private void colorMix_Checked(object sender, RoutedEventArgs e)
        {
            defaults.option = 3;
            setWindow();
            titleContent.Text = "Color Mix";
            colorFetch.IsChecked = false;
            colorPick.IsChecked = false;
            colorMix.IsEnabled = false;
        }

        private void setWindow()
        {
            colorFetch.IsEnabled = true;
            colorPick.IsEnabled = true;
            colorMix.IsEnabled = true;
        }

        private void colorPickCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (colorPickCanvas.Opacity != 1)
                colorPickCanvas.Opacity = 1;
            double x = e.Position.X;
            double y = e.Position.Y;
            double stepX = colorPickCanvas.ActualWidth / 16;
            double stepY = colorPickCanvas.ActualHeight / 16;
            x = x / colorPickCanvas.ActualWidth > 1 ? colorPickCanvas.ActualWidth : (x < 0 ? 0 : x);
            y = y / colorPickCanvas.ActualHeight > 1 ? colorPickCanvas.ActualHeight : (y < 0 ? 0 : y);
            byte modX = Convert.ToByte(x / stepX);
            byte modY = Convert.ToByte(y / stepY);
            byte red = Convert.ToByte(modX * modY == 256 ? 255 : modX * modY);
            modX = Convert.ToByte(modX == 0 ? 1 : modX);
            modY = Convert.ToByte(modY == 0 ? 1 : modY);
            byte blue = Convert.ToByte(Math.Abs((x % modX) * 255) / modX);
            byte green = Convert.ToByte(Math.Abs((y % modY) * 255) / modY);
            colorPickCanvas.Background = new SolidColorBrush(Color.FromArgb(255, red, green, blue));
            if (addPaletteColorPickButton.Visibility == Visibility.Collapsed)
                addPaletteColorPickButton.Visibility = Visibility.Visible;
        }

        private void colorMixLight_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (defaults.colorMix)
            {
                colorMixRight.Fill = new SolidColorBrush(Colors.White);
                defaults.colorMix = false;
            }
            else
            {
                colorMixLeft.Fill = new SolidColorBrush(Colors.White);
                defaults.colorMix = true;
            }
            setGradientColorMix();
        }

        private void colorMixDark_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (defaults.colorMix)
            {
                colorMixRight.Fill = new SolidColorBrush(Colors.Black);
                defaults.colorMix = false;
            }
            else
            {
                colorMixLeft.Fill = new SolidColorBrush(Colors.Black);
                defaults.colorMix = true;
            }
            setGradientColorMix();
        }
        private void setGradientColorMix()
        {
            GradientStop startGradient = new GradientStop();
            GradientStop endGradient = new GradientStop();
            startGradient.Color = ((SolidColorBrush)colorMixRight.Fill).Color;//Second Color
            startGradient.Offset = 1;
            endGradient.Color = ((SolidColorBrush)colorMixLeft.Fill).Color; //First Color
            GradientStopCollection collection = new GradientStopCollection();
            collection.Add(startGradient);
            collection.Add(endGradient);
            colorMixCanvas.Background = new LinearGradientBrush(collection, 0);
        }

        private void colorMixCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Color left = ((SolidColorBrush)colorMixLeft.Fill).Color;
            Color right = ((SolidColorBrush)colorMixRight.Fill).Color;
            double percent = e.Position.X / colorMixCanvas.ActualWidth;
            if (percent > 1)
                percent = 1;
            else if (percent < 0)
                percent = 0;
            byte red = left.R < right.R? Convert.ToByte(Math.Min(left.R, right.R) + Math.Abs(left.R - right.R) * percent) : Convert.ToByte(Math.Min(left.R, right.R) + Math.Abs(left.R - right.R) * (1 - percent));
            byte blue = left.B < right.B? Convert.ToByte(Math.Min(left.B, right.B) + Math.Abs(left.B - right.B) * percent) : Convert.ToByte(Math.Min(left.B, right.B) + Math.Abs(left.B - right.B) * (1 - percent));
            byte green = left.G < right.G? Convert.ToByte(Math.Min(left.G, right.G) + Math.Abs(left.G - right.G) * percent) : Convert.ToByte(Math.Min(left.G, right.G) + Math.Abs(left.G - right.G) * (1 - percent));
            if (colorMixColorPicked.Visibility == Visibility.Collapsed)
                colorMixColorPicked.Visibility = Visibility.Visible;
            colorMixColorPicked.Fill = new SolidColorBrush(Color.FromArgb(255, red, green, blue));
            colorMixColorPicked.Margin = new Thickness(e.Position.X - 20, 0, 0, 0);
            colorMixRGB.Text = "Red = " + red + ", Green = " + green + ", Blue = " + blue;
            if (colorMixPrimaryButton.Visibility == Visibility.Collapsed)
                colorMixPrimaryButton.Visibility = Visibility.Visible;
        }

        private async void springerButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.springer.com/book/9781484226285"));
        }

        private async void apressButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.apress.com/book/9781484226285"));
        }

        private async void amazonUSButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.amazon.com/dp/1484226283"));
        }

        private async void amazonUKButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.amazon.co.uk/dp/1484226283"));
        }

        private async void browsePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            StorageFile pic = await filePicker.PickSingleFileAsync();
            if(pic != null)
            {
                var openImage = await pic.OpenAsync(Windows.Storage.FileAccessMode.Read);
                BitmapImage image = new BitmapImage();
                image.SetSource(openImage);
                imageView.Source = image;
                setFetchPalette(pic);
            }
        }

        private async void cameraButton_Click(object sender, RoutedEventArgs e)
        {
            CameraCaptureUI capture = new CameraCaptureUI();
            capture.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            capture.PhotoSettings.AllowCropping = false;
            StorageFile pic = await capture.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if(pic != null)
            {
                IRandomAccessStream openImage = await pic.OpenAsync(Windows.Storage.FileAccessMode.Read);
                BitmapImage image = new BitmapImage();
                image.SetSource(openImage);
                imageView.Source = image;
                setFetchPalette(pic);
            }
        }
        private async void setFetchPalette(StorageFile file)
        {
            Stream stream = await file.OpenStreamForReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream.AsRandomAccessStream());
            PixelDataProvider data = await decoder.GetPixelDataAsync();
            byte[] pixels = data.DetachPixelData();
            int step = pixels.Length / 13;
            colorFetchColor1.Fill = new SolidColorBrush(Color.FromArgb(255, pixels[step], pixels[step * 5], pixels[step * 9]));
            colorFetchColor2.Fill = new SolidColorBrush(Color.FromArgb(255, pixels[step * 2], pixels[step * 6], pixels[step * 10]));
            ColorFetchColor3.Fill = new SolidColorBrush(Color.FromArgb(255, pixels[step * 3], pixels[step * 7], pixels[step * 11]));
            colorFetchColor4.Fill = new SolidColorBrush(Color.FromArgb(255, pixels[step * 4], pixels[step * 8], pixels[step * 12]));
            if (colorFetchColor1.Visibility == Visibility.Collapsed)
                colorFetchColor1.Visibility = Visibility.Visible;
            if (colorFetchColor2.Visibility == Visibility.Collapsed)
                colorFetchColor2.Visibility = Visibility.Visible;
            if (ColorFetchColor3.Visibility == Visibility.Collapsed)
                ColorFetchColor3.Visibility = Visibility.Visible;
            if (colorFetchColor4.Visibility == Visibility.Collapsed)
                colorFetchColor4.Visibility = Visibility.Visible;
            if (ColorFetchPaletteButton.Visibility == Visibility.Collapsed)
                ColorFetchPaletteButton.Visibility = Visibility.Visible;
        }

        private void ColorFetchPaletteButton_Click(object sender, RoutedEventArgs e)
        {
            paletteColor1.Fill = colorFetchColor1.Fill;
            paletteColor2.Fill = colorFetchColor2.Fill;
            paletteColor3.Fill = ColorFetchColor3.Fill;
            paletteColor4.Fill = colorFetchColor4.Fill;
        }

        private void addPaletteColorPickButton_Click(object sender, RoutedEventArgs e)
        {
            setPrimaryColor((SolidColorBrush)colorPickCanvas.Background);
        }
        private void setPrimaryColor(SolidColorBrush color)
        {
            paletteColor4.Fill = paletteColor3.Fill;
            paletteColor3.Fill = paletteColor2.Fill;
            paletteColor2.Fill = paletteColor1.Fill;
            paletteColor1.Fill = color;
        }

        private void paletteColor2_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setPrimaryColor((SolidColorBrush)paletteColor2.Fill);
        }

        private void paletteColor3_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setPrimaryColor((SolidColorBrush)paletteColor3.Fill);
        }

        private void paletteColor4_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setPrimaryColor((SolidColorBrush)paletteColor4.Fill);
        }

        private void paletteColor1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(defaults.option == 3)
            {
                if (defaults.colorMix)
                {
                    colorMixRight.Fill = new SolidColorBrush(((SolidColorBrush)paletteColor1.Fill).Color);
                    defaults.colorMix = false;
                }
                else
                {
                    colorMixLeft.Fill = new SolidColorBrush(((SolidColorBrush)paletteColor1.Fill).Color);
                    defaults.colorMix = true;
                }
                setGradientColorMix();
            }
        }

        private void colorMixPrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            setPrimaryColor((SolidColorBrush)colorMixColorPicked.Fill);
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            copyToClipboard();
            if(copyButton.IsChecked == true)
            {
                copyButton.IsChecked = false;
            }
        }

        private void copyButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            copyGrid.Visibility = copyGrid.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RGBCopyToggle_Click(object sender, RoutedEventArgs e)
        {
            if(RGBCopyToggle.IsChecked == true)
            {
                defaults.copyChoice = 1;
                HexCopyToggle.IsChecked = false;
            }
            else
            {
                RGBCopyToggle.IsChecked = true;
            }
        }

        private void HexCopyToggle_Click(object sender, RoutedEventArgs e)
        {
            if (HexCopyToggle.IsChecked == true)
            {
                defaults.copyChoice = 2;
                RGBCopyToggle.IsChecked = false;
            }
            else
            {
                HexCopyToggle.IsChecked = true;
            }
        }

        private void primaryCopyButton_Click(object sender, RoutedEventArgs e)
        {
            enableAllCopy();
            primaryCopyButton.IsEnabled = false;
            defaults.copySettings = 1;
            copyToClipboard();
        }

        private void color2CopyButton_Click(object sender, RoutedEventArgs e)
        {
            enableAllCopy();
            color2CopyButton.IsEnabled = false;
            defaults.copySettings = 2;
            copyToClipboard();
        }

        private void color3CopyButton_Click(object sender, RoutedEventArgs e)
        {
            enableAllCopy();
            color3CopyButton.IsEnabled = false;
            defaults.copySettings = 3;
            copyToClipboard();
        }

        private void color4CopyButton_Click(object sender, RoutedEventArgs e)
        {
            enableAllCopy();
            color4CopyButton.IsEnabled = false;
            defaults.copySettings = 4;
            copyToClipboard();
        }

        private void allColorsCopyButton_Click(object sender, RoutedEventArgs e)
        {
            enableAllCopy();
            allColorsCopyButton.IsEnabled = false;
            defaults.copySettings = 5;
            copyToClipboard();
        }

        private void enableAllCopy()
        {
            primaryCopyButton.IsEnabled = true;
            color2CopyButton.IsEnabled = true;
            color3CopyButton.IsEnabled = true;
            color4CopyButton.IsEnabled = true;
            allColorsCopyButton.IsEnabled = true;
        }

        private void copyToClipboard()
        {
            SolidColorBrush copyColor = new SolidColorBrush();
            switch (defaults.copySettings)
            {
                case 1:
                    copyColor = (SolidColorBrush)paletteColor1.Fill;
                    break;
                case 2:
                    copyColor = (SolidColorBrush)paletteColor2.Fill;
                    break;
                case 3:
                    copyColor = (SolidColorBrush)paletteColor3.Fill;
                    break;
                case 4:
                    copyColor = (SolidColorBrush)paletteColor4.Fill;
                    break;
            }
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            if (defaults.copyChoice == 1)
            {
                if(defaults.copySettings == 5)
                {
                    copyColor = (SolidColorBrush)paletteColor1.Fill;
                    string copyText = String.Concat(copyColor.Color.R.ToString(), ", ", copyColor.Color.G.ToString(), ", ", copyColor.Color.B.ToString());
                    copyColor = (SolidColorBrush)paletteColor2.Fill;
                    copyText = copyText + Environment.NewLine + String.Concat(copyColor.Color.R.ToString(), ", ", copyColor.Color.G.ToString(), ", ", copyColor.Color.B.ToString());
                    copyColor = (SolidColorBrush)paletteColor3.Fill;
                    copyText = copyText + Environment.NewLine + String.Concat(copyColor.Color.R.ToString(), ", ", copyColor.Color.G.ToString(), ", ", copyColor.Color.B.ToString());
                    copyColor = (SolidColorBrush)paletteColor4.Fill;
                    copyText = copyText + Environment.NewLine + String.Concat(copyColor.Color.R.ToString(), ", ", copyColor.Color.G.ToString(), ", ", copyColor.Color.B.ToString());
                    dataPackage.SetText(copyText);
                }
                else
                {
                    dataPackage.SetText(String.Concat(copyColor.Color.R.ToString(), ", ", copyColor.Color.G.ToString(), ", ", copyColor.Color.B.ToString()));
                }
            }
            else
            {
                if (defaults.copySettings == 5)
                {
                    //for you to fill up
                }
                else
                {
                    //for you to fill up
                }
            }
            Clipboard.SetContent(dataPackage);
        }

        private void sharePaletteButton_Click(object sender, RoutedEventArgs e)
        {
            //try and share it
        }
        //After you have understood what is in this application, rewrite this application using structures, custom classes and the concepts you have learnt from the book.
    }
}
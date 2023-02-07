using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Color_Filter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Mat? _loadedImage;
        private Mat? _resultImage;

        public MainWindow()
        {
            InitializeComponent();

            hueUpSlider.Value = 100;
            hueLowSlider.Value = 50;
            satUpSlider.Value = 255;
            satLowSlider.Value = 20;
            valueUpSlider.Value = 255;
            valueLowSlider.Value = 20;
        }

        private void OnOpenImageClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (openFileDialog.ShowDialog() == true)
            {
                _loadedImage = Cv2.ImRead(openFileDialog.FileName);
                UpdateModifiedImage();
            }
        }

        private static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream ms = new();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        private void OnSlidersChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateModifiedImage();
            UpdateColors();
        }

        private void UpdateModifiedImage()
        {
            if (_loadedImage is null) return;
            Mat hsv = new();
            Mat mask = new();
            Mat segmented = new();
            Cv2.CvtColor(_loadedImage, hsv, ColorConversionCodes.BGR2HSV);
            Cv2.InRange(hsv, new Scalar(hueLowSlider.Value / 2, satLowSlider.Value / 100 * 255, valueLowSlider.Value / 100 * 255),
                             new Scalar(hueUpSlider.Value / 2, satUpSlider.Value / 100 * 255, valueUpSlider.Value / 100 * 255),
                             mask);
            Cv2.BitwiseAnd(_loadedImage, _loadedImage, segmented, mask);

            _resultImage = segmented;
            image.Source = BitmapToImageSource(_resultImage.ToBitmap());
        }

        private void UpdateColors()
        {
            var upperHsv = new ColorHelper.HSV(Convert.ToInt32(hueUpSlider.Value), Convert.ToByte(satUpSlider.Value), Convert.ToByte(valueUpSlider.Value));
            var upperRgb = ColorHelper.ColorConverter.HsvToRgb(upperHsv);
            upperColor.Fill = new SolidColorBrush()
            {
                Color = System.Windows.Media.Color.FromArgb(255, upperRgb.R, upperRgb.G, upperRgb.B)
            };

            var lowerHsv = new ColorHelper.HSV(Convert.ToInt32(hueLowSlider.Value), Convert.ToByte(satLowSlider.Value), Convert.ToByte(valueLowSlider.Value));
            var lowerRgb = ColorHelper.ColorConverter.HsvToRgb(lowerHsv);
            lowerColor.Fill = new SolidColorBrush()
            {
                Color = System.Windows.Media.Color.FromArgb(255, lowerRgb.R, lowerRgb.G, lowerRgb.B)
            };
        }

        private void OnSaveImageClick(object sender, RoutedEventArgs e)
        {
            if (_resultImage is null) return;
            SaveFileDialog sv = new()
            {
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            "Portable Network Graphic (*.png)|*.png"
            };

            if (sv.ShowDialog() == true)
                Cv2.ImWrite(sv.FileName, _resultImage);
        }
    }
}
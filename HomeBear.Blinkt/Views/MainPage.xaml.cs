using HomeBear.Blinkt.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeBear.Blinkt.Views
{
    /// <summary>
    /// Entry page of the app. 
    /// It provides an navigation point to all other functionality of HomeBear.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Properties 

        readonly MainPageViewModel viewModel;

        #endregion

        #region Constructor

        public MainPage()
        {

            this.InitializeComponent();
            DataContext = viewModel = new MainPageViewModel();
        }

        #endregion

        #region Action handler

        private void BrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnBrightnessChangeRequested(value);
        }

        private void RedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnRedColorChangeRequested(value);
        }

        private void GreenSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnGreenColorChangeRequested(value);
        }

        private void BlueSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnBlueColorChangeRequested(value);
        }

        private void ColorPickerApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Use color pickers tag as index.
            var index = Convert.ToInt32(PixelColorPicker.Tag);
            var color = PixelColorPicker.Color;

            viewModel.OnColorChangeRequested(color, index);
            PixelColorPickerFlyOut.Hide();
        }

        private void ColorPickerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            PixelColorPickerFlyOut.Hide();
        }

        private void ColorPickerTurnOffButton_Click(object sender, RoutedEventArgs e)
        {
            var index = Convert.ToInt32(PixelColorPicker.Tag);
            viewModel.OnTurnOffRequested(index);
            PixelColorPickerFlyOut.Hide();
        }

        #endregion
    }
}
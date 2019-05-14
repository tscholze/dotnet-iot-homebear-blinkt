using HomeBear.Blinkt.ViewModel;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace HomeBear.Blinkt.Views
{
    /// <summary>
    /// Entry page of the app. 
    /// It provides an navigation point to all other functionality of HomeBear.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Properties 

        /// <summary>
        /// Underlying view model of the view / page.
        /// </summary>
        readonly MainPageViewModel viewModel;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of the Main Page.
        /// Will initialize the data context.
        /// </summary>
        public MainPage()
        {

            InitializeComponent();
            DataContext = viewModel = new MainPageViewModel();
        }

        #endregion

        #region Action handler

        /// <summary>
        /// Will be raised if user changes the brightness slider.
        /// </summary>
        /// <param name="sender">Sender's slider</param>
        /// <param name="e">Event args</param>
        private void BrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnBrightnessChangeRequested(value);
        }

        /// <summary>
        /// Will be raised if user changes the red slider.
        /// </summary>
        /// <param name="sender">Sender's slider</param>
        /// <param name="e">Event args</param>
        private void RedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnRedColorChangeRequested(value);
        }

        /// <summary>
        /// Will be raised if user changes the green slider.
        /// </summary>
        /// <param name="sender">Sender's slider</param>
        /// <param name="e">Event args</param>
        private void GreenSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnGreenColorChangeRequested(value);
        }

        /// <summary>
        /// Will be raised if user changes the blue slider.
        /// </summary>
        /// <param name="sender">Sender's slider</param>
        /// <param name="e">Event args</param>
        private void BlueSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var value = Convert.ToInt32(e.NewValue);
            viewModel.OnBlueColorChangeRequested(value);
        }

        /// <summary>
        /// Will be raised if user applies a color with the picker.
        /// It will raise an `OnColorChangeRequested` command.
        /// </summary>
        /// <param name="sender">Sender's button</param>
        /// <param name="e">Event args</param>
        private void ColorPickerApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // Use color pickers tag as index.
            var index = Convert.ToInt32(PixelColorPicker.Tag);
            var color = PixelColorPicker.Color;

            viewModel.OnColorChangeRequested(color, index);
            PixelColorPickerFlyOut.Hide();
        }

        /// <summary>
        /// Will be raised if user cancels the color picker action.
        /// </summary>
        /// <param name="sender">Sender's button</param>
        /// <param name="e">Event args</param>
        private void ColorPickerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            PixelColorPickerFlyOut.Hide();
        }

        /// <summary>
        /// Will be raised if user turns off the pixel via a picker action.
        /// It will raise an `OnTurnOffRequested` command.
        /// </summary>
        /// <param name="sender">Sender's button</param>
        /// <param name="e">Event args</param>
        private void ColorPickerTurnOffButton_Click(object sender, RoutedEventArgs e)
        {
            var index = Convert.ToInt32(PixelColorPicker.Tag);
            viewModel.OnTurnOffRequested(index);
            PixelColorPickerFlyOut.Hide();
        }

        #endregion
    }
}
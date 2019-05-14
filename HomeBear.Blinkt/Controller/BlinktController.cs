using HomeBear.Blinkt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;

namespace HomeBear.Blinkt.Controller
{
    /// <summary>
    /// Pimoroni original code:
    /// https://github.com/pimoroni/blinkt/blob/master/library/blinkt.py
    /// 
    /// PimoroniSharp port:
    /// https://github.com/MarcJenningsUK/PimoroniSharp/blob/master/Pimoroni.Blinkt/Blinkt.cs
    /// </summary>
    class BlinktController
    {
        #region Private constants

        /// <summary>
        /// Data BCM GPIO pin number.
        /// </summary>
        private const int GPIO_NUMBER_DATA = 23;

        /// <summary>
        /// Clock BCM GPIO pin numbers.
        /// </summary>
        private const int GPIO_NUMBER_CLOCK = 24;

        /// <summary>
        /// Number of available pixels on the Blinkt.
        /// </summary>
        private const int NUMBER_OF_PIXELS = 8;

        /// <summary>
        /// Number of pulses that is required to lock the clock.
        /// </summary>
        private const int NUMBER_OF_CLOCK_LOCK_PULSES = 36;

        /// <summary>
        /// Number of pulses that is required to release the clock.
        /// </summary>
        private const int NUMBER_OF_CLOCK_UNLOCK_PULSES = 32;

        #endregion

        #region Private properties 

        private static readonly BlinktController instance = new BlinktController();

        /// <summary>
        /// System's default GPIO controller.
        /// </summary>
        private GpioController gpioController = GpioController.GetDefault();

        /// <summary>
        /// GPIO pin for the data value.
        /// </summary>
        private GpioPin dataPin;

        /// <summary>
        /// GPIO pin for the clock value.
        /// </summary>
        private GpioPin clockPin;

        /// <summary>
        /// List of all led pixels.
        /// </summary>
        private Pixel[] pixels = new Pixel[NUMBER_OF_PIXELS];

        #endregion

        #region Public Properties

        /// <summary>
        /// Default instance of BlinktController.
        /// </summary>
        public static BlinktController Default
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Constructor & Deconstructor

        static BlinktController()
        {

        }

        private BlinktController()
        {
            // Setup list.
            for (int i = 0; i < NUMBER_OF_PIXELS; i++)
            {
                pixels[i] = new Pixel();
            }

            // Ensure required instance are set.
            if (gpioController == null)
            {
                return;
                throw new Exception("Default GPIO controller not found.");
            }

            // Setup pins.
            dataPin = gpioController.OpenPin(GPIO_NUMBER_DATA);
            clockPin = gpioController.OpenPin(GPIO_NUMBER_CLOCK);
            dataPin.SetDriveMode(GpioPinDriveMode.Output);
            clockPin.SetDriveMode(GpioPinDriveMode.Output);

            WritePixelValues();
        }

        /// <summary>
        /// Deconstructor.
        /// Cleans GPIO connections.
        /// </summary>
        ~BlinktController()
        {
            System.Console.WriteLine("DEINIT");
            TurnOff();
            WritePixelValues();
            clockPin.Dispose();
            dataPin.Dispose();
        }

        #endregion

        #region Private helper

        private void SetClockState(bool locked)
        {
            // Get the number of required pulses.
            var numberOfPulses = locked ? NUMBER_OF_CLOCK_LOCK_PULSES : NUMBER_OF_CLOCK_UNLOCK_PULSES;

            // Switch of data transfer.
            dataPin.Write(GpioPinValue.Low);

            // Send pulses to clock pin.
            for (int i = 0; i < numberOfPulses; i++)
            {
                clockPin.Write(GpioPinValue.High);
                clockPin.Write(GpioPinValue.Low);
            }
        }

        private void WritePixel(Pixel pixel)
        {
            var sendBright = (int)((31.0m * pixel.Brightness)) & 31;
            WriteByte(Convert.ToByte(224 | sendBright));
            WriteByte(Convert.ToByte(pixel.Blue));
            WriteByte(Convert.ToByte(pixel.Green));
            WriteByte(Convert.ToByte(pixel.Red));
        }

        private void WriteByte(byte input)
        {
            int value;
            byte modded = Convert.ToByte(input);
            for (int count = 0; count < 8; count++)
            {
                value = modded & 128;
                dataPin.Write(value == 128 ? GpioPinValue.High : GpioPinValue.Low);
                clockPin.Write(GpioPinValue.High);
                modded = Convert.ToByte((modded << 1) % 256);
                clockPin.Write(GpioPinValue.Low);
            }
        }

        private void WritePixelValues()
        {
            SetClockState(true);

            foreach (var pixel in pixels)
            {
                WritePixel(pixel);
            }

            SetClockState(false);
        }

        private void PerformAction(BlinktControllerAction action, int? value, bool writeByte = false, int? index = null)
        {
            // Get specified pixels.
            List<Pixel> specifiedPixels;
            if (index is int pixelIndex)
            {
                specifiedPixels = new List<Pixel> { pixels[pixelIndex] };
            }
            else
            {
                specifiedPixels = pixels.ToList();
            }

            // Perform action.
            switch (action)
            {
                case BlinktControllerAction.TurnOn:
                    specifiedPixels.ForEach(p => p.TurnOn());
                    break;

                case BlinktControllerAction.TurnOff:
                    specifiedPixels.ForEach(p => p.TurnOff());
                    break;

                case BlinktControllerAction.ModifyBrightness:
                    var brightnessValues = Convert.ToDecimal(value) / 100;
                    specifiedPixels.ForEach(p => p.SetBrightness(brightnessValues));
                    break;

                case BlinktControllerAction.ModifyRed:
                    var redValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetRed(redValue));
                    break;

                case BlinktControllerAction.ModifyGreen:
                    var greenValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetGreen(greenValue));
                    break;

                case BlinktControllerAction.ModifyBlue:
                    var blueValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetBlue(blueValue));
                    break;

                default:
                    throw new NotImplementedException($"{action} is not implemented");
            }

            // Write bytes to GPIO if required.
            if (writeByte)
            {
                WritePixelValues();
            }
        }

        #endregion

        #region Public method helper

        /// <summary>
        /// Will turn on all pixels to maximum whiteness.
        /// </summary>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void TurnOn(int? index = null)
        {
            PerformAction(BlinktControllerAction.TurnOn, null, true, index);
        }

        /// <summary>
        /// Will turn on all pixels to maximum darkness.
        /// </summary>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void TurnOff(int? index = null)
        {
            PerformAction(BlinktControllerAction.TurnOff, null, true, index);
        }

        /// <summary>
        /// Sets brightness value for all or specified pixels.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void SetBrightness(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(BlinktControllerAction.ModifyBrightness, value, writeByte, index);
        }

        /// <summary>
        /// Sets red value for all or specified pixels.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void SetRed(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(BlinktControllerAction.ModifyRed, value, writeByte, index);
        }

        /// <summary>
        /// Sets green value for all or specified pixels.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void SetGreen(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(BlinktControllerAction.ModifyGreen, value, writeByte, index);
        }

        /// <summary>
        /// Sets value for all or specified pixels.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        public void SetBlue(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(BlinktControllerAction.ModifyBlue, value, writeByte, index);

        }

        /// <summary>
        /// Updates all pixels with underlying values.
        /// </summary>
        public void UpdateAll()
        {
            WritePixelValues();
        }

        #endregion

    }

    /// <summary>
    /// Describes all available Blinkt controller actions per pixel.
    /// </summary>
    enum BlinktControllerAction
    {
        TurnOn,
        TurnOff,
        ModifyBrightness,
        ModifyRed,
        ModifyGreen,
        ModifyBlue
    }
}

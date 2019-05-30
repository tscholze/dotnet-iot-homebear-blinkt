using HomeBear.Blinkt.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;

namespace HomeBear.Blinkt.Controller
{
    /// <summary>
    /// APA102 controller as used for the Pimoroni Blinked.
    /// 
    /// Links:
    ///     - Pimoroni original code:
    ///         https://github.com/pimoroni/blinkt/blob/master/library/blinkt.py
    /// 
    ///     - PimoroniSharp port of the mostly similar Blinkt!:
    ///         https://github.com/MarcJenningsUK/PimoroniSharp/blob/master/Pimoroni.Blinkt/Blinkt.cs
    /// </summary>
    class APA102: IDisposable
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
        /// Number of available leds on the APA102 (Blinkt).
        /// </summary>
        private const int NUMBER_OF_LEDS = 8;

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

        private static readonly APA102 instance = new APA102();

        /// <summary>
        /// System's default GPIO controller.
        /// </summary>
        private GpioController gpioController = GpioController.GetDefault();

        /// <summary>
        /// GPIO pin for the data value.
        /// </summary>
        private readonly GpioPin dataPin;

        /// <summary>
        /// GPIO pin for the clock value.
        /// </summary>
        private readonly GpioPin clockPin;

        /// <summary>
        /// List of all led.
        /// </summary>
        private readonly APA102LED[] leds = new APA102LED[NUMBER_OF_LEDS];

        #endregion

        #region Public Properties

        /// <summary>
        /// Default instance of BlinktController.
        /// </summary>
        public static APA102 Default
        {
            get
            {
                return instance;
            }
        }

        #endregion

        #region Constructor & Deconstructor

        static APA102()
        {

        }

        /// <summary>
        /// Constructors.
        /// 
        /// Configurates all required values.
        /// </summary>
        private APA102()
        {
            // Setup list.
            for (int i = 0; i < NUMBER_OF_LEDS; i++)
            {
                leds[i] = new APA102LED();
            }

            // Ensure required instance are set.
            if (gpioController == null)
            {
                throw new Exception("Default GPIO controller not found.");
            }

            // Setup pins.
            dataPin = gpioController.OpenPin(GPIO_NUMBER_DATA);
            clockPin = gpioController.OpenPin(GPIO_NUMBER_CLOCK);
            dataPin.SetDriveMode(GpioPinDriveMode.Output);
            clockPin.SetDriveMode(GpioPinDriveMode.Output);

            // Write LED values to device.
            WriteLEDValues();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Disposes all related values.
        /// </summary>
        public void Dispose()
        {
            TurnOff();
            WriteLEDValues();
            clockPin.Dispose();
            dataPin.Dispose();
        }

        #endregion

        #region Private helper

        /// <summary>
        /// Sets the clock state according to given value.
        /// </summary>
        /// <param name="locked">True if state should be locked.</param>
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

        /// <summary>
        /// Writes values of a given LED to the device.
        /// </summary>
        /// <param name="led">LED to write to the device.</param>
        private void WriteLED(APA102LED led)
        {
            var sendBright = (int)((31.0m * led.Brightness)) & 31;
            WriteByte(Convert.ToByte(224 | sendBright));
            WriteByte(Convert.ToByte(led.Blue));
            WriteByte(Convert.ToByte(led.Green));
            WriteByte(Convert.ToByte(led.Red));
        }

        /// <summary>
        /// Writes given input to the device.
        /// </summary>
        /// <param name="input">New input to write.</param>
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

        /// <summary>
        /// Writes all LED values to the device.
        /// </summary>
        private void WriteLEDValues()
        {
            SetClockState(true);

            foreach (var led in leds)
            {
                WriteLED(led);
            }

            SetClockState(false);
        }

        /// <summary>
        /// Performs given action with optional parameters.
        /// </summary>
        /// <param name="action">Action to perform.</param>
        /// <param name="value">Optional value of the action.</param>
        /// <param name="writeByte">If true, action will be written to the device. Default: false.</param>
        /// <param name="index">Optional index for the action.</param>
        private void PerformAction(APA102Action action, int? value, bool writeByte = false, int? index = null)
        {
            // Get specified leds.
            List<APA102LED> specifiedPixels;
            if (index is int ledIndex)
            {
                specifiedPixels = new List<APA102LED> { leds[ledIndex] };
            }
            else
            {
                specifiedPixels = leds.ToList();
            }

            // Perform action.
            switch (action)
            {
                case APA102Action.TurnOn:
                    specifiedPixels.ForEach(p => p.TurnOn());
                    break;

                case APA102Action.TurnOff:
                    specifiedPixels.ForEach(p => p.TurnOff());
                    break;

                case APA102Action.ModifyBrightness:
                    var brightnessValues = Convert.ToDecimal(value) / 100;
                    specifiedPixels.ForEach(p => p.SetBrightness(brightnessValues));
                    break;

                case APA102Action.ModifyRed:
                    var redValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetRed(redValue));
                    break;

                case APA102Action.ModifyGreen:
                    var greenValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetGreen(greenValue));
                    break;

                case APA102Action.ModifyBlue:
                    var blueValue = Convert.ToInt32(value);
                    specifiedPixels.ForEach(p => p.SetBlue(blueValue));
                    break;

                default:
                    throw new NotImplementedException($"{action} is not implemented");
            }

            // Write bytes to GPIO if required.
            if (writeByte)
            {
                WriteLEDValues();
            }
        }

        #endregion

        #region Public method helper

        /// <summary>
        /// Will turn on all leds to maximum whiteness.
        /// </summary>
        /// <param name="index">If set, only specified led will be modified.</param>
        public void TurnOn(int? index = null)
        {
            PerformAction(APA102Action.TurnOn, null, true, index);
        }

        /// <summary>
        /// Will turn on all leds to maximum darkness.
        /// </summary>
        /// <param name="index">If set, only specified led will be modified.</param>
        public void TurnOff(int? index = null)
        {
            PerformAction(APA102Action.TurnOff, null, true, index);
        }

        /// <summary>
        /// Sets brightness value for all or specified leds.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified led will be modified.</param>
        public void SetBrightness(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(APA102Action.ModifyBrightness, value, writeByte, index);
        }

        /// <summary>
        /// Sets red value for all or specified leds.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified led will be modified.</param>
        public void SetRed(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(APA102Action.ModifyRed, value, writeByte, index);
        }

        /// <summary>
        /// Sets green value for all or specified leds.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        /// <param name="index">If set, only specified led will be modified.</param>
        public void SetGreen(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(APA102Action.ModifyGreen, value, writeByte, index);
        }

        /// <summary>
        /// Sets value for all or specified leds.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <param name="writeByte">If `true`, the changes will 
        /// immediately written to the GPIO pins.</param>
        public void SetBlue(int value, bool writeByte = false, int? index = null)
        {
            PerformAction(APA102Action.ModifyBlue, value, writeByte, index);

        }

        /// <summary>
        /// Updates all leds with underlying values.
        /// </summary>
        public void UpdateAll()
        {
            WriteLEDValues();
        }

        #endregion
    }
}

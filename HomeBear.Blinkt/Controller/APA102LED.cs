using HomeBear.Blinkt.Utils.Extension;
using System;

namespace HomeBear.Blinkt.Model
{
    /// <summary>
    /// Describes a rgb-colorful LED.
    /// </summary>
    public class APA102LED
    {
        #region Public properties

        /// <summary>
        /// Brightness of the pixel.
        /// </summary>
        public decimal Brightness { get; private set; }

        /// <summary>
        /// Green part of the led lightning.
        /// Use setRed(...) to set the value.
        /// </summary>
        public int Red { get; private set; }

        /// <summary>
        /// Green part of the led lightning.
        /// Use setGreen(...) to set the value.
        /// </summary>
        public int Green { get; private set; }

        /// <summary>
        /// Blue part of the led lightning.
        /// Use setBlue(...) to set the value.
        /// </summary>
        public int Blue { get; private set; }

        #endregion

        #region Public Helper

        /// <summary>
        /// Sets int-based brightness value.
        /// Value has to be between 0 and 100.
        /// </summary>
        /// <param name="value">Brightness value.</param>
        public void SetBrightness(decimal value)
        {
            if (value < 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException("Brightness value must be between 0 and 1.");
            }

            Brightness = value;
        }

        /// <summary>
        /// Sets int-based red value.
        /// Value has to be between 0 and 255.
        /// </summary>
        /// <param name="value">Red value.</param>
        public void SetRed(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException("Red value must be between 0 and 255.");
            }

            Red = value;
        }

        /// <summary>
        /// Sets int-based green value.
        /// Value has to be between 0 and 255.
        /// </summary>
        /// <param name="value">Green value.</param>
        public void SetGreen(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException("Green value must be between 0 and 255.");
            }

            Green = value;
        }

        /// <summary>
        /// Sets int-based blue value.
        /// Value has to be between 0 and 255.
        /// </summary>
        /// <param name="value">Blue value.</param>
        public void SetBlue(int value)
        {
            if (value < 0 || value > 255)
            {
                throw new ArgumentOutOfRangeException("Blue value must be between 0 and 255.");
            }

            Blue = value;
        }

        /// <summary>
        /// Sets r,g,b values.
        /// RGB values habe to be between 0 and 255.
        /// Brightness has to be between 0 and 100.
        /// </summary>
        /// <param name="red">Red value.</param>
        /// <param name="green">Green value.</param>
        /// <param name="blue">Blue value.</param>
        /// <param name="brightness">Brightness value. Default value is 1.</param>
        public void SetRgb(int red, int green, int blue, int brightness = 1)
        {
            SetBrightness(brightness);
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
        }

        /// <summary>
        /// Sets hex string-based blue value.
        /// Value has to be six value characters.
        /// E.g. #112233 or 0xrrggbb.
        /// </summary>
        /// <param name="hex">Hexadecimal string.</param>
        /// <param name="brightness">Brightness value. Default value is 1.</param>
        public void SetRgbHex(string hex, int brightness = 1)
        {
            // Unify string look.
            hex = hex.Replace("#", string.Empty);
            hex = hex.Replace("0x", string.Empty);
            hex = hex.ToUpper();

            // Check for the valid length of the string.
            if (hex.Length == 6 == false)
            {
                throw new ArgumentOutOfRangeException("Hex must have 6 value characters like 0xrrggbb.");
            }

            // Convert string to hex parts and set related values.
            SetBrightness(brightness);
            SetRed(hex.ToHexInt(0));
            SetGreen(hex.ToHexInt(2));
            SetBlue(hex.ToHexInt(4));
        }

        /// <summary>
        /// Sets all colors to the maximum (white).
        /// </summary>
        public void TurnOn()
        {
            SetBrightness(1);
            SetRed(255);
            SetGreen(255);
            SetBlue(255);
        }

        /// <summary>
        /// Sets all colors to the minimum (black).
        /// </summary>
        public void TurnOff()
        {
            SetBrightness(0);
            SetRed(0);
            SetGreen(0);
            SetBlue(0);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Pixel instance by given rgb values.
        /// All values have to between 0 and 255.
        /// 
        /// Default value is r, g, b = 0.
        /// </summary>
        /// <param name="red">Red value.</param>
        /// <param name="green">Green value.</param>
        /// <param name="blue">Blue value.</param>
        /// <param name="brightness">Brightness value. Default value is 1.</param>
        public APA102LED(int red = 0, int green = 0, int blue = 0, int brightness = 1)
        {
            SetRed(red);
            SetGreen(green);
            SetBlue(blue);
        }

        /// <summary>
        /// Creates a new Pixel instance by given hex string-value.
        /// Value has to be six value characters.
        /// E.g. #112233 or 0x112233.
        /// </summary>
        /// <param name="hex">Hexadecimal string.</param>
        /// <param name="brightness">Brightness value. Default value is 1.</param>
        public APA102LED(string hex, int brightness = 1)
        {
            SetBrightness(brightness);
            SetRgbHex(hex);
        }

        #endregion
    }
}

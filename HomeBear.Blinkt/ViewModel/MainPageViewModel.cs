using GalaSoft.MvvmLight.Command;
using HomeBear.Blinkt.Controller;
using System;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.System.Threading;
using Windows.UI;

namespace HomeBear.Blinkt.ViewModel
{
    class MainPageViewModel : BaseViewModel
    {
        #region Public properties 

        private string currentTime;
        /// <summary>
        /// Gets the current time.
        /// </summary>
        public string CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the personal, formatted greeting.
        /// </summary>
        public string Greeting
        {
            get
            {
                return "Hey ho maker friends!";
            }
        }

        /// <summary>
        /// Gets the app name.
        /// </summary>
        public string AppName
        {
            get
            {
                return Package.Current.DisplayName;
            }
        }

        /// <summary>
        /// Gets the app author's url.
        /// </summary>
        public string AppAuthorUrl
        {
            get
            {
                return "tscholze.github.io";
            }
        }

        /// <summary>
        /// Gets the current formatted app version.
        /// </summary>
        public string AppVersion
        {
            get
            {
                return string.Format("Version: {0}.{1}",
                    Package.Current.Id.Version.Major,
                    Package.Current.Id.Version.Minor);
            }
        }

        /// <summary>
        /// Will trigger the switchting on and off of all
        /// pixels.
        /// </summary>
        public ICommand LightSwitchCommand { get; private set; }

        /// <summary>
        /// Will trigger a new random generated color to be set.
        /// </summary>
        public ICommand RandomizeLightCommand { get; private set; }

        /// <summary>
        /// Will trigger a random animation of colors.
        /// </summary>
        public ICommand AnimateLightCommand { get; private set; }

        #endregion

        #region Private properties

        /// <summary>
        /// Underlying blink controller.
        /// </summary>
        readonly APA102 blinktController = APA102.Default;

        /// <summary>
        /// Helper property to store which action should off / on button
        /// raise.
        /// </summary>
        bool lightIsOn = false;

        /// <summary>
        /// Determins the currently animated pixel index. Required for the
        /// `AnimateLightCommand`.
        /// </summary>
        int animatedPixelIndex = 0;

        /// <summary>
        /// Animated pixel timer instance.
        /// </summary>
        ThreadPoolTimer animatedThreadTimer;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor of the MainPageViewModel.
        /// Will setup timers and commands.
        /// </summary>
        public MainPageViewModel()
        {
            // Setup timer.
            ThreadPoolTimer.CreatePeriodicTimer
                (ClockTimer_Tick,
                TimeSpan.FromSeconds(1)
           );

            // Setup commands
            LightSwitchCommand = new RelayCommand(() =>
            {
                if (lightIsOn)
                {
                    blinktController.TurnOff();
                }
                else
                {
                    blinktController.TurnOn();
                }

                lightIsOn = !lightIsOn;
            });

            RandomizeLightCommand = new RelayCommand(() =>
            {
                Random random = new Random();

                // Set randomized values.
                blinktController.SetBrightness(100);
                blinktController.SetRed(random.Next(256));
                blinktController.SetGreen(random.Next(256));
                blinktController.SetBlue(random.Next(256));

                // Trigger batch value update.
                blinktController.UpdateAll();
            });

            AnimateLightCommand = new RelayCommand(() =>
            {
                // If timer is set (its active)
                // interpret the command call as "stop".
                if (animatedThreadTimer != null)
                {
                    blinktController.TurnOff();
                    StopAnimateTimer();
                    return;
                }

                // Reset the index and start new timer.
                animatedPixelIndex = 0;
                animatedThreadTimer = ThreadPoolTimer.CreatePeriodicTimer(AnimatedThreadTimer_Tick, TimeSpan.FromMilliseconds(500));
            });
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Will stop the animate timer.
        /// </summary>
        private void StopAnimateTimer()
        {
            animatedThreadTimer.Cancel();
            animatedThreadTimer = null;
        }

        /// <summary>
        /// Will be update the `CurrentTime` member with each tick.
        /// </summary>
        /// <param name="timer"></param>
        private void ClockTimer_Tick(ThreadPoolTimer timer)
        {
            CurrentTime = DateTime.Now.ToShortTimeString();
        }

        /// <summary>
        /// Will update and modify the animate pixel with each tick.
        /// </summary>
        /// <param name="timer"></param>
        private void AnimatedThreadTimer_Tick(ThreadPoolTimer timer)
        {
            // Animate pixel. 
            // Ruleset:
            //  If index is 0 (first pixel)
            //      -> turn the pixel on
            //  If index is 1 to 7
            //      -> turn incremented pixel on and old pixel off
            //  If index is 8 (that means one more than requiered)
            //      -> turn old pixel of and cancel timer
            switch (animatedPixelIndex)
            {
                case 0:
                    blinktController.TurnOn(0);
                    break;

                case 8:
                    StopAnimateTimer();
                    blinktController.TurnOff(animatedPixelIndex - 1);
                    break;

                default:
                    blinktController.TurnOn(animatedPixelIndex);
                    blinktController.TurnOff(animatedPixelIndex - 1);
                    break;
            }

            // Increment animated pixel.
            animatedPixelIndex++;
        }

        #endregion

        #region Public helper methods

        /// <summary>
        /// Raised on user wants to turn on a specified lamp.
        /// </summary>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void OnTurnOnRequested(int index)
        {
            blinktController.TurnOn(index);
        }

        /// <summary>
        /// Raised on user wants to turn off a specified lamp.
        /// </summary>
        /// <param name="index">If set, only specified pixel will be modified.</param>
        public void OnTurnOffRequested(int index)
        {
            blinktController.TurnOff(index);
        }

        /// <summary>
        /// Raised on user wants to change brightness color.
        /// </summary>
        /// <param name="value">New brightness value.</param>
        public void OnBrightnessChangeRequested(int value)
        {
            blinktController.SetBrightness(value, true, null);
        }

        /// <summary>
        /// Raised on user wants to change red color.
        /// </summary>
        /// <param name="value">New red value.</param>
        public void OnRedColorChangeRequested(int value)
        {
            blinktController.SetRed(value, true, null);
        }

        /// <summary>
        /// Raised on user wants to change green color.
        /// </summary>
        /// <param name="value">New green value.</param>
        public void OnGreenColorChangeRequested(int value)
        {
            blinktController.SetGreen(value, true, null);
        }

        /// <summary>
        /// Raised on user wants to change blue color.
        /// </summary>
        /// <param name="value">New blue value.</param>
        public void OnBlueColorChangeRequested(int value)
        {
            blinktController.SetBlue(value, true, null);
        }

        /// <summary>
        /// Raised on user wants to change the color of a 
        /// specified pixel.
        /// </summary>
        /// <param name="color">New color.</param>
        /// <param name="index">Pixel index.</param>
        public void OnColorChangeRequested(Color color, int index)
        {
            // Set values for selected pixel.
            blinktController.SetBrightness(100, false, index);
            blinktController.SetRed(color.R, false, index);
            blinktController.SetGreen(color.G, false, index);
            blinktController.SetBlue(color.B, false, index);

            // Batch update
            blinktController.UpdateAll();
        }

        #endregion
    }
}

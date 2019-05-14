using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

namespace HomeBear.Blinkt.ViewModel
{
    /// <summary>
    /// This helper class simplifies the process of `INotifyPropertyChanged` handling.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when property changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Occures on the property changed.
        /// </summary>
        /// <param name="propertyName">Property name.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            // Ensure required data is set.
            var changed = PropertyChanged;
            if (changed == null)
                return;

            // Dispatch action onto the ui thread.
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        #endregion

        #region Private properties 

        /// <summary>
        /// Current dispatcher.
        /// </summary>
        private readonly CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;

        #endregion
    }
}

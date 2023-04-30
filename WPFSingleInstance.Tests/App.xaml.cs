using System.Collections.Generic;
using System.Windows;

namespace WPFSingleInstance.Tests
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstance
    {
        #region ISingleInstanceApp Members

        /// <summary>
        /// Execute this method when you retry to open the application and its already open
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (MainWindow is null)
            {
                MainWindow = new MainWindow();
            }
            
            if (MainWindow.WindowState == WindowState.Minimized)
            {
                MainWindow.WindowState = WindowState.Normal;
            }
            if (MainWindow.Visibility != Visibility.Visible)
            {
                MainWindow.UpdateLayout();
                MainWindow.Visibility = Visibility.Visible;
            }

            MainWindow.Activate();
            MainWindow.Focus();
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;

            return true;
        }

        #endregion
    }
}

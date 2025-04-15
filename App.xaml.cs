using System.Configuration;
using System.Data;
using System.Windows;

namespace SysMax
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Show the splash screen (using fully qualified name)
            SysMax2._1.SplashScreen splashScreen = new SysMax2._1.SplashScreen();
            // Use ShowDialog to make it modal and wait for it to close
            bool? splashResult = splashScreen.ShowDialog();

            // Optionally check splashResult if you need to know if loading succeeded
            if (splashResult == true) 
            {
                // Create and show the main window only after the splash screen is closed (using fully qualified name)
                SysMax2._1.MainWindow mainWindow = new SysMax2._1.MainWindow();
                // Set the application's MainWindow property
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                // Handle the case where splash screen loading failed or was cancelled
                // For now, just shut down
                this.Shutdown();
            }
        }
    }

}

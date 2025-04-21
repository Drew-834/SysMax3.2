using System;
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
                try
                {
                    // Create and show the main window only after the splash screen is closed (using fully qualified name)
                    SysMax2._1.MainWindow mainWindow = new SysMax2._1.MainWindow();
                    // Set the application's MainWindow property
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                }
                catch (Exception ex)
                {
                    // Log or display the exception to understand the failure
                    MessageBox.Show($"Failed to load the main application window:\n\n{ex.ToString()}", 
                                    "Application Startup Error", 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Error);
                    // Optionally log to a file here as well
                    this.Shutdown(-1); // Shutdown with an error code
                }
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

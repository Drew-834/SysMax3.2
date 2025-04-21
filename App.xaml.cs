using System;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading; // Needed for DispatcherUnhandledException

namespace SysMax
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Add global exception handlers
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

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

        // Handler for UI thread exceptions
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Prevent default WPF crash behavior
            e.Handled = true; 
            ShowUnhandledExceptionError(e.Exception, "UI Thread Exception");
        }

        // Handler for non-UI thread exceptions
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowUnhandledExceptionError(e.ExceptionObject as Exception, "Non-UI Thread Exception");
            // It's generally recommended to shut down after a non-UI thread exception
            // unless you know specifically how to recover.
            if (e.IsTerminating)
            {
                // If the CLR is terminating, we can't really prevent it.
                // Log and attempt to show message, but it might not work.
            }
        }

        // Helper method to show the error
        private void ShowUnhandledExceptionError(Exception ex, string errorType)
        {
            string errorMessage = $"An unexpected error occurred ({errorType}):\n\n{(ex?.ToString() ?? "Unknown Error")}";
            
            // Use MessageBox.Show (might not work reliably if called from non-UI thread crash)
            // Logging to a file here would be more robust.
            try
            {
                MessageBox.Show(errorMessage, "Unhandled Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                // If MessageBox fails, attempt to log or just swallow
            }
            
            // Ensure shutdown after reporting error, especially for non-UI exceptions
            // Use a non-zero exit code to indicate failure
            Shutdown(-1);
        }
    }
}

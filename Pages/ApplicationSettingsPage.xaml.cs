using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using SysMax2._1.Models;
using SysMax2._1.Services;
using System.IO;
using System.Text.Json;

namespace SysMax2._1.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationSettingsPage.xaml
    /// </summary>
    public partial class ApplicationSettingsPage : Page
    {
        private MainWindow? mainWindow;
        private readonly SettingsService _settingsService;
        private AppSettings _currentSettings; // Initialized in constructor

        public ApplicationSettingsPage()
        {
            InitializeComponent();

            // Use the singleton instance and load settings immediately
            _settingsService = SettingsService.Instance;
            _currentSettings = _settingsService.LoadSettings(); // Initialize _currentSettings here

            // Get reference to main window for assistant interactions
            mainWindow = Window.GetWindow(this) as MainWindow;

            // Apply loaded settings to the UI
            ApplySettingsToUI();
        }

        private void ThresholdSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                // Use slider's Name property to identify which slider changed
                if (slider.Name == "CpuThresholdSlider" && CpuThresholdText != null)
                {
                    CpuThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider.Name == "MemoryThresholdSlider" && MemoryThresholdText != null)
                {
                    MemoryThresholdText.Text = $"{(int)slider.Value}%";
                }
                else if (slider.Name == "DiskThresholdSlider" && DiskThresholdText != null)
                {
                    DiskThresholdText.Text = $"{(int)slider.Value}%";
                }
            }
        }

        private void ApplySettingsToUI()
        {
            try
            {
                // Notifications
                EnableNotificationsCheckbox.IsChecked = _currentSettings.EnableNotifications;
                CriticalIssuesOnlyCheckbox.IsChecked = _currentSettings.CriticalIssuesOnly;
                NotificationSoundCheckbox.IsChecked = _currentSettings.NotificationSound;

                // Monitoring
                UpdateFrequencyComboBox.SelectedIndex = _currentSettings.GetUpdateFrequencyIndex();
                CpuThresholdSlider.Value = _currentSettings.CpuAlertThreshold;
                MemoryThresholdSlider.Value = _currentSettings.MemoryAlertThreshold;
                DiskThresholdSlider.Value = _currentSettings.DiskAlertThreshold;
                CpuThresholdText.Text = $"{_currentSettings.CpuAlertThreshold}%";
                MemoryThresholdText.Text = $"{_currentSettings.MemoryAlertThreshold}%";
                DiskThresholdText.Text = $"{_currentSettings.DiskAlertThreshold}%";

                // Advanced
                EnableLoggingCheckbox.IsChecked = _currentSettings.EnableLogging;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying settings to UI: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (mainWindow != null) mainWindow.UpdateStatus("Error loading settings UI");
            }
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create AppSettings object from UI
            var settingsToSave = new AppSettings
            {
                // Notifications
                EnableNotifications = EnableNotificationsCheckbox.IsChecked ?? false,
                CriticalIssuesOnly = CriticalIssuesOnlyCheckbox.IsChecked ?? false,
                NotificationSound = NotificationSoundCheckbox.IsChecked ?? false,

                // Monitoring
                CpuAlertThreshold = (int)CpuThresholdSlider.Value,
                MemoryAlertThreshold = (int)MemoryThresholdSlider.Value,
                DiskAlertThreshold = (int)DiskThresholdSlider.Value,

                // Advanced
                EnableLogging = EnableLoggingCheckbox.IsChecked ?? false,
                
                // Keep existing values for removed settings to avoid losing them if user downgrades?
                // Or just don't save them anymore.
                Language = _currentSettings.Language, // Keep existing
                Theme = _currentSettings.Theme, // Keep existing
                DefaultUserMode = _currentSettings.DefaultUserMode, // Keep existing
                StartWithWindows = _currentSettings.StartWithWindows, // Keep existing
                RunInBackground = _currentSettings.RunInBackground, // Keep existing
                AutoUpdateCheck = _currentSettings.AutoUpdateCheck // Keep existing
            };
            
            // Use helper methods for remaining ComboBoxes
            settingsToSave.SetUpdateFrequencyFromIndex(UpdateFrequencyComboBox.SelectedIndex);

            // Save using the service
            try
            {
                _settingsService.SaveSettings(settingsToSave);
                _currentSettings = settingsToSave; // Update the in-memory settings

                MessageBox.Show("Settings saved successfully.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);

                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Settings saved");
                    mainWindow.ShowAssistantMessage("Your application settings have been saved.");
                }

                // Apply settings that require immediate action
                ApplyImmediateSettings(settingsToSave);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (mainWindow != null)
                {
                    mainWindow.UpdateStatus("Error saving settings");
                    mainWindow.ShowAssistantMessage($"I encountered an error trying to save your settings: {ex.Message}");
                }
            }
        }
        
        // Helper method to apply settings that need immediate effect
        private void ApplyImmediateSettings(AppSettings settings)
        {
            // Update Logging Service state based on EnableLogging setting
            LoggingService.Instance.IsLoggingEnabled = settings.EnableLogging;
            LoggingService.Instance.Log(LogLevel.Info, $"Logging enabled status set to: {settings.EnableLogging}");

            // Update Hardware Monitor thresholds (already handled by SettingsSaved event in service)
            // Update Hardware Monitor interval (already handled by SettingsSaved event in service)
            
            // Update Notification Service state (if one exists)
            // NotificationService.Instance.IsEnabled = settings.EnableNotifications;
            // NotificationService.Instance.NotifyOnlyCritical = settings.CriticalIssuesOnly;
            // NotificationService.Instance.PlaySound = settings.NotificationSound;
        }
    }
}

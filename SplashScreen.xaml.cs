using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SysMax2._1
{
    public partial class SplashScreen : Window
    {
        private DispatcherTimer _animationTimer;
        private List<string> _loadingPhrases = new List<string>
        {
            "Warming up the circuits...",
            "Checking system flux capacitors...",
            "Reticulating splines...",
            "Polishing the interface...",
            "Optimizing hyperdrive parameters...",
            "Aligning the Dilithium crystals...",
            "Engaging warp core..."
        };
        private Random _random = new Random();

        public SplashScreen()
        {
            InitializeComponent();
            _animationTimer = null!; // Initialize to null to satisfy compiler, will be set in Loaded event
            this.Loaded += SplashScreen_Loaded;
        }

        private void SplashScreen_Loaded(object? sender, RoutedEventArgs e)
        {
            // Start phrase animation
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromSeconds(1.5);
            _animationTimer.Tick += AnimationTimer_Tick;
            UpdateLoadingText(); // Initial text
            _animationTimer.Start();

            // Simulate loading work and close splash screen
            SimulateLoading();
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            UpdateLoadingText();
        }

        private void UpdateLoadingText()
        {
            int index = _random.Next(_loadingPhrases.Count);
            LoadingText.Text = _loadingPhrases[index];
        }

        private async void SimulateLoading()
        {
            // Simulate work (e.g., loading resources, initializing services)
            await Task.Delay(5000); // Simulate 5 seconds of loading

            // Stop the timer
            _animationTimer?.Stop();

            // Signal that loading is complete (App.xaml.cs will handle this)
            this.DialogResult = true; // Indicate success
            this.Close();
        }
    }
} 
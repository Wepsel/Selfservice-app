using System;
using System.Windows;
using System.Windows.Threading;

namespace MooieWelkomApp
{
    public partial class LoadingScreen : Window
    {
        private DispatcherTimer timer;
        private int progressValue;

        public LoadingScreen()
        {
            InitializeComponent();
            SetLoadingText();
            StartProgressAnimation();
        }

        private void SetLoadingText()
        {
            string greeting = GetGreeting();
            string userName = Environment.UserName; // Haal de naam van de gebruiker op
            txtLoading.Text = $"{greeting}, {userName}..."; // Stel de tekst in op basis van het tijdstip en de naam van de gebruiker
        }

        private string GetGreeting()
        {
            int hour = DateTime.Now.Hour;

            if (hour >= 5 && hour < 12)
            {
                return "Goedemorgen";
            }
            else if (hour >= 12 && hour < 18)
            {
                return "Goedemiddag";
            }
            else
            {
                return "Goedeavond";
            }
        }

        private void StartProgressAnimation()
        {
            progressValue = 0;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };

            timer.Tick += (s, e) =>
            {
                progressValue += 2; // Verhoog de voortgang
                progressBar.Value = progressValue;

                if (progressValue >= 100)
                {
                    timer.Stop();
                    Close(); // Sluit het laadscherm
                }
            };

            timer.Start();
        }
    }
}

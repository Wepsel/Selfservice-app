using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace MooieWelkomApp
{
    public partial class StartupAppsWindow : Window
    {
        public StartupAppsWindow()
        {
            InitializeComponent();
        }

        private void OpenElo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://elo.kempenhorst.nl",
                UseShellExecute = true
            });
        }

        private void OpenMagister_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://vobo.magister.net",
                UseShellExecute = true
            });
        }

        private void OpenWebmail_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://webmail.voboscholen.nl",
                UseShellExecute = true
            });
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            // Zorg ervoor dat het hoofdmenu (MainWindow) weer wordt geopend
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

            if (mainWindow == null)
            {
                // Als MainWindow niet gevonden wordt, maak een nieuw exemplaar aan
                mainWindow = new MainWindow();
            }

            mainWindow.Show(); // Toon het hoofdmenu
            this.Close();      // Sluit dit venster
        }
    }
}

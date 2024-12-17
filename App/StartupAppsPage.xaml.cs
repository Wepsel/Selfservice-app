using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;

namespace MooieWelkomApp
{
    public partial class StartupAppsWindow : Window
    {
        public class StartupApp
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public bool IsEnabled { get; set; }
            public string Source { get; set; }
        }

        private List<StartupApp> startupApps = new List<StartupApp>();

        public StartupAppsWindow()
        {
            InitializeComponent();
            LoadStartupApps();
        }

        private void LoadStartupApps()
        {
            startupApps.Clear();

            // Lees zowel HKEY_CURRENT_USER als HKEY_LOCAL_MACHINE
            LoadAppsFromRegistry("HKEY_CURRENT_USER", Registry.CurrentUser, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run");
            LoadAppsFromRegistry("HKEY_LOCAL_MACHINE", Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run");

            StartupAppsList.ItemsSource = startupApps;
        }

        private void LoadAppsFromRegistry(string source, RegistryKey baseKey, string runPath, string startupApprovedPath)
        {
            Dictionary<string, bool> approvedApps = new Dictionary<string, bool>();

            // Lees StartupApproved sleutel (status)
            using (RegistryKey approvedKey = baseKey.OpenSubKey(startupApprovedPath))
            {
                if (approvedKey != null)
                {
                    foreach (var appName in approvedKey.GetValueNames())
                    {
                        byte[] status = approvedKey.GetValue(appName) as byte[];
                        if (status != null && status.Length > 0)
                        {
                            // Status van app bepalen
                            bool isEnabled = status[0] == 0;
                            approvedApps[appName] = isEnabled;
                        }
                    }
                }
            }

            // Lees Run sleutel (werkelijke opstart-apps)
            using (RegistryKey runKey = baseKey.OpenSubKey(runPath))
            {
                if (runKey != null)
                {
                    foreach (var appName in runKey.GetValueNames())
                    {
                        string path = runKey.GetValue(appName)?.ToString();
                        bool isEnabled = approvedApps.ContainsKey(appName) ? approvedApps[appName] : true; // Default ingeschakeld als onbekend

                        startupApps.Add(new StartupApp
                        {
                            Name = appName,
                            Path = path,
                            IsEnabled = isEnabled,
                            Source = source
                        });
                    }
                }
            }

            // Voeg uitgeschakelde apps toe die niet in de Run-sleutel staan
            foreach (var appName in approvedApps.Keys)
            {
                if (!startupApps.Any(a => a.Name == appName))
                {
                    startupApps.Add(new StartupApp
                    {
                        Name = appName,
                        Path = "Uitgeschakeld",
                        IsEnabled = false,
                        Source = source
                    });
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(sender, true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateRegistry(sender, false);
        }

        private void UpdateRegistry(object sender, bool enable)
        {
            var checkBox = sender as System.Windows.Controls.CheckBox;
            var app = checkBox?.Tag as StartupApp;

            if (app == null) return;

            RegistryKey baseKey = app.Source == "HKEY_LOCAL_MACHINE" ? Registry.LocalMachine : Registry.CurrentUser;
            string runPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            string startupApprovedPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

            try
            {
                using (RegistryKey approvedKey = baseKey.OpenSubKey(startupApprovedPath, true))
                using (RegistryKey runKey = baseKey.OpenSubKey(runPath, true))
                {
                    if (enable)
                    {
                        // Zet app aan
                        runKey.SetValue(app.Name, app.Path);
                        approvedKey.SetValue(app.Name, new byte[] { 0, 0, 0, 0 }); // Ingeschakeld
                    }
                    else
                    {
                        // Zet app uit
                        runKey.DeleteValue(app.Name, false);
                        approvedKey.SetValue(app.Name, new byte[] { 2, 0, 0, 0 }); // Uitgeschakeld
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fout bij bijwerken: {ex.Message}");
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            mainWindow?.Show();
            this.Close();
        }
    }
}

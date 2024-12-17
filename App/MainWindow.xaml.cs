using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MooieWelkomApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;
            ShowLoadingScreen();
        }
        private void NavigateToHome_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigeren naar Homepagina.");
        }

        private void NavigateToOpstartApps_Click(object sender, RoutedEventArgs e)
        {
            // Maak het nieuwe venster en open het
            StartupAppsWindow startupWindow = new StartupAppsWindow();
            startupWindow.Show();

            // Verberg het huidige hoofdvenster
            this.Hide();
        }

        private void NavigateToAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Navigeren naar Over-pagina.");
        }
        private void ShowLoadingScreen()
        {
            // Simuleer een laadscherm
            LoadingScreen loadingScreen = new LoadingScreen();
            loadingScreen.ShowDialog();

            // Timer voor laadscherm
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                loadingScreen.Close();
                this.Visibility = Visibility.Visible;
                txtWelcome.Text = $"Welkom, {System.Environment.UserName}";
                txtWelcome.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1)));
                UpdateLastUpdateText();
                UpdateUptimeText();
            };
            timer.Start();
        }

        private void UpdateLastUpdateText()
        {
            DateTime? lastUpdate = GetLastWindowsUpdate();
            if (lastUpdate.HasValue)
            {
                txtLastUpdate.Text = $"Laatste Windows Update: {lastUpdate.Value:G}";

                // Controleer of het langer dan 30 dagen geleden is
                var timeSinceLastUpdate = DateTime.Now - lastUpdate.Value;
                if (timeSinceLastUpdate.Days >= 30)
                {
                    ShowNotification("Het is al meer dan 30 dagen geleden sinds de laatste update. Het is tijd om je systeem bij te werken!");
                }
            }
            else
            {
                txtLastUpdate.Text = "Laatste Windows Update: Geen updates uitgevoerd!";
                ShowNotification("Geen updates uitgevoerd! Het wordt aanbevolen om updates uit te voeren.");
            }

            // Voeg de Dell Command Update-informatie toe
            DateTime? dellCommandUpdate = GetDellCommandUpdate();
            if (dellCommandUpdate.HasValue)
            {
                txtDellCommandLastUpdate.Text = $"Laatste Dell update controle: {dellCommandUpdate.Value:G}";

                // Controleer of het langer dan 30 dagen geleden is
                var timeSinceLastUpdate = DateTime.Now - dellCommandUpdate.Value;
                if (timeSinceLastUpdate.Days >= 30)
                {
                    ShowNotification("Het is al meer dan 30 dagen geleden sinds de laatste Dell update. Het is tijd om je systeem bij te werken!");
                }
            }
            else
            {
                txtDellCommandLastUpdate.Text = "Laatste Dell update controle: Geen data.";
                ShowNotification("Geen updates uitgevoerd! Het wordt aanbevolen om updates uit te voeren.");
            }
        }

        private void UpdateUptimeText()
        {
            var uptime = GetSystemUptime();
            int days = (int)uptime.TotalDays;
            int hours = uptime.Hours;
            int minutes = uptime.Minutes;
            txtUptime.Text = $"Computer is al {days} dagen {hours} uren {minutes} minuten niet opnieuw opgestart";

            // Controleer of de uptime 3 dagen of meer is
            if (days >= 3)
            {
                ShowNotification("De computer is al 3 dagen of langer aan. Overweeg om het systeem opnieuw op te starten om optimale prestaties te behouden.");
            }
        }

        private TimeSpan GetSystemUptime()
        {
            try
            {
                using (PerformanceCounter uptimeCounter = new PerformanceCounter("System", "System Up Time"))
                {
                    uptimeCounter.NextValue(); // Reset counter
                    System.Threading.Thread.Sleep(1000); // Wait for next value
                    float uptimeSeconds = uptimeCounter.NextValue();
                    return TimeSpan.FromSeconds(uptimeSeconds);
                }
            }
            catch (Exception)
            {
                return TimeSpan.Zero;
            }
        }

        private DateTime? GetLastWindowsUpdate()
        {
            try
            {
                EventLog eventLog = new EventLog("System");
                foreach (EventLogEntry entry in eventLog.Entries)
                {
                    if (entry.Source == "Microsoft-Windows-WindowsUpdateClient")
                    {
                        return entry.TimeGenerated;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private DateTime? GetDellCommandUpdate()
        {
            try
            {
                return GetLastDellUpdateFromRegistry();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private DateTime? GetLastDellUpdateFromRegistry()
        {
            try
            {
                var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Dell\UpdateService\Service");
                if (key != null)
                {
                    object timestampObj = key.GetValue("LastCheckTimestamp");
                    if (timestampObj != null)
                    {
                        string timestampString = timestampObj.ToString();
                        if (DateTime.TryParse(timestampString, out DateTime lastCheckTime))
                        {
                            return lastCheckTime;
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void ShowNotification(string message)
        {
            // Zet de tekst van de melding
            txtNotification.Text = message;

            // Zorg ervoor dat de popup bovenop het venster verschijnt
            NotificationPopup.IsOpen = true;
            NotificationPopup.StaysOpen = true;

            // Zet de popup zichtbaar met animatie, zodat hij niet wordt verborgen
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(1));
            NotificationPopup.BeginAnimation(OpacityProperty, fadeIn);
        }

        // Sluit de notificatie-popup wanneer de OK-knop is ingedrukt
        private void CloseNotification(object sender, RoutedEventArgs e)
        {
            NotificationPopup.IsOpen = false;
        }

        private void ResetWiFi_Click(object sender, RoutedEventArgs e)
        {
            ResetWifiAdapter();
        }

        private void ResetWifiAdapter()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/C netsh interface set interface \"Wi-Fi\" disable")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);

                psi.Arguments = "/C netsh wlan delete profile name=*";
                Process.Start(psi);

                psi.Arguments = "/C netsh interface set interface \"Wi-Fi\" enable";
                Process.Start(psi);

                MessageBox.Show("Wi-Fi adapter is gereset en de profielen zijn verwijderd.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden bij het resetten van de Wi-Fi adapter: {ex.Message}");
            }
        }
        private void ResetKeyboardLayout_Click(object sender, RoutedEventArgs e)
        {
            ResetKeyboardLayout();
        }

        private void ResetKeyboardLayout()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", "-Command \"& { $langList = Get-WinUserLanguageList; $langList[0].InputMethodTips.Add('0409:00020409'); Set-WinUserLanguageList $langList -Force; }\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);

                MessageBox.Show("De toetsenbordinstelling is gewijzigd naar Verenigde Staten (Internationaal).");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden bij het resetten van de toetsenbordinstelling: {ex.Message}");
            }
        }
        // Knop voor het uitvoeren van Dell Command Update
        private void InvokeDellCommandUpdate_Click(object sender, RoutedEventArgs e)
        {
            InvokeDellCommandUpdate();
        }

        private void InvokeDellCommandUpdate()
        {
            try
            {
                // Pad naar Dell Command Update CLI
                string dcuPath = @"C:\Program Files (x86)\Dell\CommandUpdate\dcu-cli.exe";

                if (!File.Exists(dcuPath))
                {
                    MessageBox.Show("Dell Command Update is niet geïnstalleerd op dit systeem.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Fase 1: Zoeken naar updates
                MessageBox.Show("Dell Command Update zoekt naar updates...", "Informatie", MessageBoxButton.OK, MessageBoxImage.Information);
                ProcessStartInfo scanProcess = new ProcessStartInfo
                {
                    FileName = dcuPath,
                    Arguments = "/scan",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (Process process = Process.Start(scanProcess))
                {
                    process.WaitForExit();
                }

                // Fase 2: Updates toepassen
                string logFilePath = @"C:\tmp\dcu-log.txt";
                ProcessStartInfo applyUpdatesProcess = new ProcessStartInfo
                {
                    FileName = dcuPath,
                    Arguments = $"/applyupdates /log \"{logFilePath}\"",
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                using (Process process = Process.Start(applyUpdatesProcess))
                {
                    process.WaitForExit();
                }

                // Controleer logbestand
                if (File.Exists(logFilePath))
                {
                    MessageBox.Show($"Updates succesvol toegepast. Controleer het logbestand: {logFilePath}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Updates succesvol toegepast, maar het logbestand kon niet worden gevonden.", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                // Foutmelding tonen
                MessageBox.Show($"Er is een fout opgetreden tijdens het uitvoeren van Dell Command Update: {ex.Message}", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Knop voor het wissen van de Google Chrome-cache
        private void ClearChromeCache_Click(object sender, RoutedEventArgs e)
        {
            ClearChromeCache();
        }
        private void ClearChromeCache()
        {
            try
            {
                string chromeCachePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Google\\Chrome\\User Data\\Default\\Cache";
                if (System.IO.Directory.Exists(chromeCachePath))
                {
                    System.IO.Directory.Delete(chromeCachePath, true);
                    MessageBox.Show("De Google Chrome-cache is succesvol gewist.");
                }
                else
                {
                    MessageBox.Show("Google Chrome-cachemap niet gevonden. Chrome is mogelijk niet geïnstalleerd.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden bij het wissen van de Google Chrome-cache: {ex.Message}");
            }
        }

        // Knop voor het uitvoeren van Windows Updates
        private void InvokeWindowsUpdate_Click(object sender, RoutedEventArgs e)
        {
            InvokeWindowsUpdate();
        }

        private void InvokeWindowsUpdate()
        {
            try
            {
                // Start Windows Update proces zonder zichtbare interface
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", "-Command \"UsoClient StartInstall\"")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };
                Process.Start(psi);

                MessageBox.Show("Windows Updates zijn gestart. Controleer later of herstarten nodig is.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Er is een fout opgetreden bij het starten van Windows Updates: {ex.Message}");
            }
        }
    }
}

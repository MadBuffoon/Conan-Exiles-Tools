using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace CE_Launcher
{
    public partial class MainWindow : Window
    {
        private string modFolderPath;
        private string executablePath;
        private string settingsFilePath;
        private string lastSelectedFilePath;
        private Process runningProcess;

        public MainWindow()
        {
            InitializeComponent();
            InitializePaths();
            LoadSettings(); // Load settings first to get the last selected file
            LoadTxtFiles(); // Load files after settings so we can set the selected item
            CheckIfProcessIsRunning(); // Check if the process is already running
        }

        private void InitializePaths()
        {
            // Get the current directory of the application
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            modFolderPath = Path.Combine(currentDirectory, "ConanSandbox\\Mods");
            executablePath = Path.Combine(currentDirectory, "ConanSandbox\\Binaries\\Win64\\ConanSandbox.exe");
            settingsFilePath = Path.Combine(currentDirectory, "CELauncher_Settings.json");
        }

        private void LoadSettings()
        {
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                var settings = JsonConvert.DeserializeObject<Settings>(json);

                if (settings != null)
                {
                    if (!string.IsNullOrEmpty(settings.LastSelectedFile))
                    {
                        lastSelectedFilePath = settings.LastSelectedFile;
                    }

                    // Set the window's position
                    this.Top = settings.WindowTop;
                    this.Left = settings.WindowLeft;

                    // Set the checkbox state
                    CloseOnLaunchCheckBox.IsChecked = settings.CloseOnLaunch;
                }
            }
        }

        private void LoadTxtFiles()
        {
            try
            {
                var txtFiles = Directory.GetFiles(modFolderPath, "*.txt")
                    .Where(file => !file.EndsWith("modlist.txt", StringComparison.OrdinalIgnoreCase))
                    .Select(file => new FileInfo(file))
                    .ToList();

                // Create a dictionary to store the full file names and their display names without the .txt extension
                var fileDictionary = txtFiles.ToDictionary(
                    f => f.FullName, // Key: Full file path
                    f => Path.GetFileNameWithoutExtension(f.Name) // Value: File name without the .txt extension
                );

                // Bind the display names (values) to the ComboBox
                TxtFileDropdown.ItemsSource = fileDictionary.Values.ToList();

                // Set the last selected file as the selected item (trimmed for display)
                if (!string.IsNullOrEmpty(lastSelectedFilePath))
                {
                    var lastSelectedFileName = Path.GetFileNameWithoutExtension(Path.GetFileName(lastSelectedFilePath));
                    TxtFileDropdown.SelectedItem = fileDictionary.Values.FirstOrDefault(name => name.Equals(lastSelectedFileName, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load text files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CheckIfProcessIsRunning()
        {
            // Get the process name from the executable path
            string processName = Path.GetFileNameWithoutExtension(executablePath);

            // Check if the process is running
            runningProcess = Process.GetProcessesByName(processName).FirstOrDefault();

            if (runningProcess != null)
            {
                // If running, disable the Launch button and change its appearance
                LaunchButton.IsEnabled = false;
                LaunchButton.Content = "Running...";

                // Monitor the process to restore the button when the process exits
                runningProcess.EnableRaisingEvents = true;
                runningProcess.Exited += OnProcessExited;
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            // Restore the Launch button when the process exits
            Dispatcher.Invoke(() =>
            {
                LaunchButton.IsEnabled = true;
                LaunchButton.Content = "Launch";
            });
        }

        private void SaveSettings(string selectedFile)
        {
            var settings = new Settings
            {
                LastSelectedFile = selectedFile,
                WindowTop = this.Top,
                WindowLeft = this.Left,
                CloseOnLaunch = CloseOnLaunchCheckBox.IsChecked == true  // Save checkbox state
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsFilePath, json);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (TxtFileDropdown.SelectedItem is string selectedFile)
            {
                SaveSettings(selectedFile);
            }
            else
            {
                SaveSettings(null);
            }
        }

        private async void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TxtFileDropdown.SelectedItem is string selectedDisplayName && !string.IsNullOrWhiteSpace(selectedDisplayName))
                {
                    // Find the original full path based on the selected display name
                    var selectedFile = Directory.GetFiles(modFolderPath, "*.txt")
                        .Select(f => new FileInfo(f))
                        .FirstOrDefault(f => Path.GetFileNameWithoutExtension(f.Name).Equals(selectedDisplayName, StringComparison.OrdinalIgnoreCase));

                    if (selectedFile != null && selectedFile.Exists)
                    {
                        var modListPath = Path.Combine(modFolderPath, "modlist.txt");
                        File.Copy(selectedFile.FullName, modListPath, overwrite: true);

                        SaveSettings(selectedFile.FullName);

                        // Add a 1-second delay before launching the executable
                        await Task.Delay(1000);

                        try
                        {
                            Process process = System.Diagnostics.Process.Start(executablePath);
                            if (process != null)
                            {
                                LaunchButton.IsEnabled = false;
                                LaunchButton.Content = "Running...";

                                // Monitor the process to restore the button when the process exits
                                process.EnableRaisingEvents = true;
                                process.Exited += OnProcessExited;

                                // Close the window if the checkbox is checked
                                if (CloseOnLaunchCheckBox.IsChecked == true)
                                {
                                    Close();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to launch the application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"The selected file does not exist: {selectedFile?.FullName}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a file from the dropdown.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class Settings
    {
        public string LastSelectedFile { get; set; }
        public double WindowTop { get; set; }
        public double WindowLeft { get; set; }
        public bool CloseOnLaunch { get; set; }  // New property to store checkbox state
    }
}

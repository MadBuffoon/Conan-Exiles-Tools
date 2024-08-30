using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ConanExilesModManager
{
    public partial class MainWindow : Window
    {
        private Border _selectedRow; // Track the currently selected row
        // Variables to track the last click time and double-click detection
        private DateTime _lastClickTime;
        private const int DoubleClickTime = 300; // Time in milliseconds for double-click detection

        // Variables to track the sorting order for Available mods
        private bool _isAvailableIdSortedAscending = true;
        private bool _isAvailableNameSortedAscending = true;
        private bool _isAvailableSizeSortedAscending = true;

        // Variables to track the sorting order for Selected mods
        private bool _isSelectedIdSortedAscending = true;
        private bool _isSelectedNameSortedAscending = true;
        private bool _isSelectedSizeSortedAscending = true;

        public MainWindow()
        {
            InitializeComponent();
            SetWindowTitleWithVersion();
            SettingsManager.LoadWindowPositionAndSize(this);
            LoadSettings();
            this.KeyDown += OnKeyDown;
        }

        private void SetWindowTitleWithVersion()
        {
            Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string versionString = $"v{version.Major}.{version.Minor}.{version.Build}";
            this.Title = $"Conan Exiles Mod Manager {versionString}";
            VersionTextBlock.Text = $"{versionString}";
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        private void Button_SaveSettings_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsManager.SaveSetting("ModFolder", Mod_Folder_Text.Text);
            SettingsManager.SaveSetting("WorkshopModFolder", WorkshopModFolder_Text.Text);
            SettingsManager.SaveSetting("ConanExilesFolder", ConanExilesFolder_Text.Text);

            MessageBox.Show("Settings have been saved successfully.", "Save Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void LoadSettings()
        {
            // Check if the settings file exists before attempting to load it
            if (!SettingsManager.SettingsFileExists())
            {
                MessageBox.Show("Settings file not found. Please go to the settings tab and fix.", "Load Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var settings = SettingsManager.LoadSettings();
            if (settings.TryGetValue("ModFolder", out var modFolder))
                Mod_Folder_Text.Text = modFolder;

            if (settings.TryGetValue("WorkshopModFolder", out var workshopFolder))
                WorkshopModFolder_Text.Text = workshopFolder;

            if (settings.TryGetValue("ConanExilesFolder", out var conanFolder))
                ConanExilesFolder_Text.Text = conanFolder;

            ModManager.LoadAvailableMods(AvailableModsPanel, SelectedModsPanel, workshopFolder, this);
        }

        private void Button_Import_OnClick(object sender, RoutedEventArgs e)
        {
            ModManager.LoadAvailableMods(AvailableModsPanel, SelectedModsPanel, WorkshopModFolder_Text.Text, this);
            ModManager.ImportMods(AvailableModsPanel, SelectedModsPanel, Import_Export.Text, this);
        }

        private void Button_Export_OnClick(object sender, RoutedEventArgs e)
        {
            ModManager.ExportMods(SelectedModsPanel, Import_Export);
        }

        
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_selectedRow != null)
            {
                if (e.Key == Key.Up)
                {
                    MoveUp_Click(sender, e);
                }
                else if (e.Key == Key.Down)
                {
                    MoveDown_Click(sender, e);
                }
            }
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow != null)
            {
                int index = SelectedModsPanel.Children.IndexOf(_selectedRow);

                if (index > 1) // Ensure it doesn't move above the header
                {
                    SelectedModsPanel.Children.RemoveAt(index);
                    SelectedModsPanel.Children.Insert(index - 1, _selectedRow);
                }
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRow != null)
            {
                int index = SelectedModsPanel.Children.IndexOf(_selectedRow);

                if (index < SelectedModsPanel.Children.Count - 1) // Ensure it doesn't move below the last item
                {
                    SelectedModsPanel.Children.RemoveAt(index);
                    SelectedModsPanel.Children.Insert(index + 1, _selectedRow);
                }
            }
        }

        private void Button_ModFolder_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select the folder where the modlist.txt file is located.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            string folderPath = SettingsManager.SelectFolder(); // Updated to use SettingsManager.SelectFolder
            if (!string.IsNullOrEmpty(folderPath))
            {
                Mod_Folder_Text.Text = folderPath;
                SettingsManager.SaveSetting("ModFolder", folderPath);
            }
        }

        private void Button_WorkshopMod_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select the folder where the Steam \\steamapps \\workshop \\content \\440900 folder is located.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            string folderPath = SettingsManager.SelectFolder(); // Updated to use SettingsManager.SelectFolder
            if (!string.IsNullOrEmpty(folderPath))
            {
                WorkshopModFolder_Text.Text = folderPath;
                SettingsManager.SaveSetting("WorkshopModFolder", folderPath);
            }
        }

        private void Button_ConanExilesFolder_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please select the folder where the game is installed.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            string folderPath = SettingsManager.SelectFolder(); // Updated to use SettingsManager.SelectFolder
            if (!string.IsNullOrEmpty(folderPath))
            {
                ConanExilesFolder_Text.Text = folderPath;
                SettingsManager.SaveSetting("ConanExilesFolder", folderPath);
            }
        }


        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ModManager.LoadAvailableMods(AvailableModsPanel, SelectedModsPanel, WorkshopModFolder_Text.Text, this);
        }

        private void Button_OpenModList_OnClick(object sender, RoutedEventArgs e)
        {
            ModManager.LoadAvailableMods(AvailableModsPanel, SelectedModsPanel, WorkshopModFolder_Text.Text, this);
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = Mod_Folder_Text.Text,
                Filter = "Text files (*.txt)|*.txt",
                Title = "Open Mod List"
            };

            if (dialog.ShowDialog() == true)
            {
                string[] modPaths = System.IO.File.ReadAllLines(dialog.FileName);
                ModManager.LoadSelectedModsFromTxt(SelectedModsPanel, AvailableModsPanel, modPaths, WorkshopModFolder_Text.Text, this);
            }
        }

        private void Button_Save_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = Mod_Folder_Text.Text,
                Filter = "Text files (*.txt)|*.txt",
                Title = "Save Mod List",
                FileName = "ModList.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                SaveSelectedModsToFile(saveDialog.FileName);
            }
        }

        private void SaveSelectedModsToFile(string filePath)
        {
            var lines = new System.Collections.Generic.List<string>();

            foreach (var child in SelectedModsPanel.Children.OfType<Border>())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock;
                var nameText = grid.Children[1] as TextBlock;

                string baseFolder;
                string modLine;

                if (idText.Text == "Mods" || nameText.Text == "Mods")
                {
                    baseFolder = Mod_Folder_Text.Text;
                    modLine = System.IO.Path.Combine(baseFolder, nameText.Text + ".pak");
                }
                else
                {
                    baseFolder = WorkshopModFolder_Text.Text;
                    modLine = System.IO.Path.Combine(baseFolder, idText.Text, nameText.Text + ".pak");
                }

                lines.Add(modLine);
            }

            System.IO.File.WriteAllLines(filePath, lines);

            MessageBox.Show("Mod list saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SettingsManager.SaveWindowPositionAndSize(this);
        }

        public void OpenModSteamPage(string modId)
        {
            string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={modId}";
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the Steam page. {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportAllSelectedMods()
        {
            var ids = new System.Collections.Generic.List<string>();

            foreach (var child in SelectedModsPanel.Children.OfType<Border>())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock;
                ids.Add(idText.Text);
            }

            Import_Export.Text = string.Join(", ", ids);
            MainTabControl.SelectedIndex = 1; // Assuming "Import/Export" is at index 1
        }

        // Sorting and Row selection event handlers would go here...
        private void SelectRow(Border row)
        {
            if (_selectedRow != null)
            {
                // Reset the background color of the previously selected row
                _selectedRow.Background = new SolidColorBrush(Colors.Transparent);
            }

            // Highlight the newly selected row
            _selectedRow = row;
            _selectedRow.Background = new SolidColorBrush(Colors.Yellow);
        }

        public void ModInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid modRow)
            {
                DateTime now = DateTime.Now;

                if ((now - _lastClickTime).TotalMilliseconds <= DoubleClickTime)
                {
                    if (modRow.Parent is Border border)
                    {
                        if (SelectedModsPanel.Children.Contains(border))
                        {
                            SelectedModsPanel.Children.Remove(border);
                            AvailableModsPanel.Children.Add(border);
                        }
                        else if (AvailableModsPanel.Children.Contains(border))
                        {
                            AvailableModsPanel.Children.Remove(border);
                            SelectedModsPanel.Children.Add(border);

                            SelectRow(border);
                        }
                    }
                }
                else if (modRow.Parent is Border border && SelectedModsPanel.Children.Contains(border))
                {
                    SelectRow(border);
                }

                _lastClickTime = now;
            }
        }
        private void SortAvailableByID(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("ID", AvailableModsPanel, ref _isAvailableIdSortedAscending);
            }
        }

        private void SortAvailableByName(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("Name", AvailableModsPanel, ref _isAvailableNameSortedAscending);
            }
        }

        private void SortAvailableBySize(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("Size", AvailableModsPanel, ref _isAvailableSizeSortedAscending);
            }
        }

        private void SortSelectedByID(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("ID", SelectedModsPanel, ref _isSelectedIdSortedAscending);
            }
        }

        private void SortSelectedByName(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("Name", SelectedModsPanel, ref _isSelectedNameSortedAscending);
            }
        }

        private void SortSelectedBySize(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                UIManager.SortMods("Size", SelectedModsPanel, ref _isSelectedSizeSortedAscending);
            }
        }

    }
}

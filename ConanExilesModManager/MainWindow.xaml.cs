using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Newtonsoft.Json;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;

namespace ConanExilesModManager
{
    public partial class MainWindow : Window
    {
        private Border _selectedRow; // Track the currently selected row
        private DateTime _lastClickTime;
        private const int DoubleClickTime = 300; // Milliseconds
        private bool _isIdSortedAscending = true;
        private bool _isNameSortedAscending = true;
        private bool _isSizeSortedAscending = true;


        public MainWindow()
{
    InitializeComponent();
    LoadSettings(); // Load settings when the window is initialized
    this.KeyDown += OnKeyDown;
}
        
        private void AddContextMenu(Grid modRow, string modId)
        {
            var contextMenu = new ContextMenu();

            // Menu item for opening the mod's Steam page
            var openSteamPageMenuItem = new MenuItem { Header = "Open Mod Steam Page" };
            openSteamPageMenuItem.Click += (s, e) => OpenModSteamPage(modId);
            contextMenu.Items.Add(openSteamPageMenuItem);

            // Menu item for exporting all selected mods
            var exportAllMenuItem = new MenuItem { Header = "Export" };
            exportAllMenuItem.Click += (s, e) => ExportAllSelectedMods();
            contextMenu.Items.Add(exportAllMenuItem);

            modRow.ContextMenu = contextMenu;
        }

        private void ExportAllSelectedMods()
        {
            var ids = new List<string>();

            foreach (var child in SelectedModsPanel.Children.OfType<Border>())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock; // Folder Name (ID)

                ids.Add(idText.Text);
            }

            // Join all IDs with commas and set the result to the Import_Export TextBox
            Import_Export.Text = string.Join(", ", ids);

            // Find the TabControl
            var tabControl = this.FindName("MainTabControl") as TabControl;
            if (tabControl != null)
            {
                // Find the specific TabItem by Header
                foreach (TabItem tabItem in tabControl.Items)
                {
                    if (tabItem.Header.ToString() == "Import/Export")
                    {
                        tabControl.SelectedItem = tabItem;
                        break;
                    }
                }
            }
        }





        private void OpenModSteamPage(string modId)
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



private bool _isAvailableIdSortedAscending = true;
private bool _isAvailableNameSortedAscending = true;
private bool _isAvailableSizeSortedAscending = true;

private bool _isSelectedIdSortedAscending = true;
private bool _isSelectedNameSortedAscending = true;
private bool _isSelectedSizeSortedAscending = true;

private void SortAvailableByID(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("ID", AvailableModsPanel, ref _isAvailableIdSortedAscending);
    }
}

private void SortAvailableByName(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("Name", AvailableModsPanel, ref _isAvailableNameSortedAscending);
    }
}

private void SortAvailableBySize(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("Size", AvailableModsPanel, ref _isAvailableSizeSortedAscending);
    }
}

private void SortSelectedByID(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("ID", SelectedModsPanel, ref _isSelectedIdSortedAscending);
    }
}

private void SortSelectedByName(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("Name", SelectedModsPanel, ref _isSelectedNameSortedAscending);
    }
}

private void SortSelectedBySize(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 2)
    {
        SortMods("Size", SelectedModsPanel, ref _isSelectedSizeSortedAscending);
    }
}

private void SortMods(string column, StackPanel panel, ref bool isAscending)
{
    // Get all the rows except for the header row (assumed to be the first child)
    var rows = panel.Children.OfType<Border>().ToList();

    // Sort the rows based on the selected column
    IOrderedEnumerable<Border> sortedRows = null;

    switch (column)
    {
        case "ID":
            sortedRows = isAscending
                ? rows.OrderBy(row => ((row.Child as Grid).Children[0] as TextBlock).Text)
                : rows.OrderByDescending(row => ((row.Child as Grid).Children[0] as TextBlock).Text);
            break;

        case "Name":
            sortedRows = isAscending
                ? rows.OrderBy(row => ((row.Child as Grid).Children[1] as TextBlock).Text)
                : rows.OrderByDescending(row => ((row.Child as Grid).Children[1] as TextBlock).Text);
            break;

        case "Size":
            sortedRows = isAscending
                ? rows.OrderBy(row => Convert.ToDouble(((row.Child as Grid).Children[2] as TextBlock).Text.Replace(" MB", "")))
                : rows.OrderByDescending(row => Convert.ToDouble(((row.Child as Grid).Children[2] as TextBlock).Text.Replace(" MB", "")));
            break;
    }

    isAscending = !isAscending; // Toggle the sorting order for next time

    // Clear the panel and re-add the sorted rows
    panel.Children.Clear();

    // Add the sorted rows back to the panel
    foreach (var row in sortedRows)
    {
        panel.Children.Add(row);
    }
}







        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAvailableMods(); // Reload the available mods when the Refresh button is clicked
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

        private void ModInfo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid modRow)
            {
                DateTime now = DateTime.Now;

                if ((now - _lastClickTime).TotalMilliseconds <= DoubleClickTime)
                {
                    // Double-click detected
                    if (modRow.Parent is Border border)
                    {
                        if (SelectedModsPanel.Children.Contains(border))
                        {
                            // Move from Selected to Available
                            SelectedModsPanel.Children.Remove(border);
                            AvailableModsPanel.Children.Add(border);
                        }
                        else if (AvailableModsPanel.Children.Contains(border))
                        {
                            // Move from Available to Selected
                            AvailableModsPanel.Children.Remove(border);
                            SelectedModsPanel.Children.Add(border);

                            // Select and highlight the row in the Selected Mods panel
                            SelectRow(border);
                        }
                    }
                }
                else if (modRow.Parent is Border border && SelectedModsPanel.Children.Contains(border))
                {
                    // Single-click, select the row but only in the Selected Mods panel
                    SelectRow(border);
                }

                _lastClickTime = now;
            }
        }

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

        private void LoadAvailableMods()
{
    string workshopModFolderPath = WorkshopModFolder_Text.Text;

    if (Directory.Exists(workshopModFolderPath))
    {
        // Clear any previous content
        AvailableModsPanel.Children.Clear();
        SelectedModsPanel.Children.Clear();

        // Get all directories within the WorkshopModFolder
        var directories = Directory.GetDirectories(workshopModFolderPath);

        foreach (var dir in directories)
        {
            var folderName = Path.GetFileName(dir);
            var pakFiles = Directory.GetFiles(dir, "*.pak");

            foreach (var pakFile in pakFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pakFile);
                var fileInfo = new FileInfo(pakFile);
                var fileSizeInMB = fileInfo.Length / (1024.0 * 1024.0); // Convert size to MB

                // Create a Grid for the mod info row
                var modRow = new Grid
                {
                    Margin = new Thickness(0, 0, 0, 0) // Remove margin between rows
                };

                // Adjusted column widths
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // ID column
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) }); // Name column
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) }); // Size column

                // Add the Folder Name, Name, and Size to the grid
                var idText = new TextBlock { Text = folderName, TextAlignment = TextAlignment.Right }; // Folder Name as ID
                var nameText = new TextBlock { Text = fileNameWithoutExtension, Margin = new Thickness(10, 0, 0, 0) };
                var sizeText = new TextBlock { Text = $"{fileSizeInMB:F2} MB",TextAlignment = TextAlignment.Right, Margin = new Thickness(5, 0, 5, 0) };

                Grid.SetColumn(nameText, 1);
                Grid.SetColumn(sizeText, 2);

                modRow.Children.Add(idText);
                modRow.Children.Add(nameText);
                modRow.Children.Add(sizeText);

                // Attach hover events for highlighting
                modRow.MouseEnter += (s, e) => modRow.Background = new SolidColorBrush(Colors.LightGray);
                modRow.MouseLeave += (s, e) => modRow.Background = new SolidColorBrush(Colors.Transparent);

                // Attach click event handler
                modRow.MouseLeftButtonDown += ModInfo_MouseLeftButtonDown;

                // Attach context menu for opening Steam page
                AddContextMenu(modRow, folderName);

                // Create a Border to wrap the modRow and add a line underneath
                var borderedRow = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(0, 0, 0, 1), // Line between rows
                    Child = modRow
                };

                // Add the Border to the Available Mods panel
                AvailableModsPanel.Children.Add(borderedRow);
            }
        }
    }
    else
    {
        MessageBox.Show("The WorkshopModFolder path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

        private void AddHeadersToPanel(StackPanel panel, string title)
        {
            // Add title TextBlock
            panel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            });

            // Create a header row for ID, Name, and Size
            var header = new Grid
            {
                Margin = new Thickness(0, 0, 0, 5)
            };

            // Adjusted column widths
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // ID column
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) }); // Name column
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) }); // Size column

            var idHeader = new TextBlock { Text = "ID", FontWeight = FontWeights.Bold };
            var nameHeader = new TextBlock { Text = "Name", FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 0, 0) };
            var sizeHeader = new TextBlock { Text = "Size", FontWeight = FontWeights.Bold, Margin = new Thickness(5, 0, 0, 0) };

            Grid.SetColumn(nameHeader, 1);
            Grid.SetColumn(sizeHeader, 2);

            header.Children.Add(idHeader);
            header.Children.Add(nameHeader);
            header.Children.Add(sizeHeader);

            panel.Children.Add(header);
        }

        private void LoadSettings()
        {
            string jsonFilePath = "CEMM_Settings.json";

            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                if (settings != null)
                {
                    if (settings.ContainsKey("ModFolder"))
                    {
                        Mod_Folder_Text.Text = settings["ModFolder"];
                    }

                    if (settings.ContainsKey("WorkshopModFolder"))
                    {
                        WorkshopModFolder_Text.Text = settings["WorkshopModFolder"];
                    }

                    if (settings.ContainsKey("ConanExilesFolder"))
                    {
                        ConanExilesFolder_Text.Text = settings["ConanExilesFolder"];
                    }

                    // Load window position and size
                    if (settings.ContainsKey("WindowTop") && settings.ContainsKey("WindowLeft"))
                    {
                        this.Top = double.Parse(settings["WindowTop"]);
                        this.Left = double.Parse(settings["WindowLeft"]);
                    }

                    if (settings.ContainsKey("WindowHeight") && settings.ContainsKey("WindowWidth"))
                    {
                        this.Height = double.Parse(settings["WindowHeight"]);
                        this.Width = double.Parse(settings["WindowWidth"]);
                    }

                    // After loading the settings, load the available mods
                    LoadAvailableMods();
                }
            }
            else
            {
                MessageBox.Show("Settings file not found. Please go to settings tab and fix.", "Load Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void SavePathToJson(string key, string path)
        {
            string jsonFilePath = "CEMM_Settings.json"; // Path to save the config file
            Dictionary<string, string> paths;

            // Load existing JSON if it exists
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                paths = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            else
            {
                paths = new Dictionary<string, string>();
            }

            // Update the path
            paths[key] = path;

            // Save the updated dictionary back to the file
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(paths, Formatting.Indented));
        }

        private void ShowMessage(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private string SelectFolder()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true // This ensures the dialog is for folder selection
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) // Use CommonFileDialogResult.Ok
            {
                return dialog.FileName;
            }
            return null;
        }

        private void Button_ModFolder_OnClick(object sender, RoutedEventArgs e)
        {
            // Show message before folder selection
            ShowMessage("Please select the folder where the modlist.txt file is located.");

            string folderPath = SelectFolder();
            if (!string.IsNullOrEmpty(folderPath))
            {
                Mod_Folder_Text.Text = folderPath;
                SavePathToJson("ModFolder", folderPath);
            }
        }

        private void Button_WorkshopMod_OnClick(object sender, RoutedEventArgs e)
        {
            // Show message before folder selection
            ShowMessage("Please select the folder where the \\workshop\\content\\440900 folder is located.");

            string folderPath = SelectFolder();
            if (!string.IsNullOrEmpty(folderPath))
            {
                WorkshopModFolder_Text.Text = folderPath;
                SavePathToJson("WorkshopModFolder", folderPath);
            }
        }

        private void Button_ConanExilesFolder_OnClick(object sender, RoutedEventArgs e)
        {
            // Show message before folder selection
            ShowMessage("Please select the folder where the game is installed.");

            string folderPath = SelectFolder();
            if (!string.IsNullOrEmpty(folderPath))
            {
                ConanExilesFolder_Text.Text = folderPath;
                SavePathToJson("ConanExilesFolder", folderPath);
            }
        }


        private void Button_SaveSettings_OnClick(object sender, RoutedEventArgs e)
        {
            // Get the current values from the text fields
            string modFolderPath = Mod_Folder_Text.Text;
            string workshopModFolderPath = WorkshopModFolder_Text.Text;
            string conanExilesFolderPath = ConanExilesFolder_Text.Text;

            // Save these values to the JSON file
            SavePathToJson("ModFolder", modFolderPath);
            SavePathToJson("WorkshopModFolder", workshopModFolderPath);
            SavePathToJson("ConanExilesFolder", conanExilesFolderPath);

            // Optionally show a message to indicate the settings were saved
            MessageBox.Show("Settings have been saved successfully.", "Save Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void Button_OpenModList_OnClick(object sender, RoutedEventArgs e)
{
    var dialog = new Microsoft.Win32.OpenFileDialog
    {
        InitialDirectory = Mod_Folder_Text.Text,
        Filter = "Text files (*.txt)|*.txt",
        Title = "Open Mod List"
    };

    if (dialog.ShowDialog() == true)
    {
        string[] modPaths = File.ReadAllLines(dialog.FileName);
        LoadSelectedModsFromTxt(modPaths);
    }
}

private void LoadSelectedModsFromTxt(string[] modPaths)
{
    LoadAvailableMods();
    SelectedModsPanel.Children.Clear();

    foreach (var modPath in modPaths)
    {
        string modName = Path.GetFileNameWithoutExtension(modPath);
        string folderName = Path.GetFileName(Path.GetDirectoryName(modPath));
        string availableModPath = Path.Combine(WorkshopModFolder_Text.Text, folderName, modName + ".pak");

        bool isAvailable = File.Exists(availableModPath);

        var modRow = new Grid
        {
            Margin = new Thickness(0, 0, 0, 0) // Remove margin between rows
        };

        // Adjusted column widths
        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // ID column
        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) }); // Name column
        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) }); // Size column

        // Add the Folder Name, Name, and Size to the grid
        var idText = new TextBlock { Text = folderName }; // Folder Name as ID
        var nameText = new TextBlock { Text = modName, Margin = new Thickness(10, 0, 0, 0) };
        var sizeText = new TextBlock { Text = isAvailable ? $"{new FileInfo(availableModPath).Length / (1024.0 * 1024.0):F2} MB" : "N/A", Margin = new Thickness(5, 0, 5, 0) };

        Grid.SetColumn(nameText, 1);
        Grid.SetColumn(sizeText, 2);

        modRow.Children.Add(idText);
        modRow.Children.Add(nameText);
        modRow.Children.Add(sizeText);

        // Persistent red background if mod is not available
        if (!isAvailable)
        {
            modRow.Background = new SolidColorBrush(Colors.LightCoral);
        }
        else
        {
            // Attach hover events for highlighting if available
            modRow.MouseEnter += (s, e) => modRow.Background = new SolidColorBrush(Colors.LightGray);
            modRow.MouseLeave += (s, e) => modRow.Background = new SolidColorBrush(Colors.Transparent);
        }

        // Attach click event handler
        modRow.MouseLeftButtonDown += ModInfo_MouseLeftButtonDown;

        // Attach context menu for opening Steam page
        AddContextMenu(modRow, folderName);

        // Create a Border to wrap the modRow and add a line underneath
        var borderedRow = new Border
        {
            BorderBrush = new SolidColorBrush(Colors.Gray),
            BorderThickness = new Thickness(0, 0, 0, 1), // Line between rows
            Child = modRow
        };

        // Add the Border to the Selected Mods panel
        SelectedModsPanel.Children.Add(borderedRow);

        // Remove from AvailableModsPanel if available
        if (isAvailable)
        {
            RemoveFromAvailableMods(folderName, modName);
        }
    }
}

private void RemoveFromAvailableMods(string folderName, string modName)
{
    foreach (var child in AvailableModsPanel.Children.OfType<Border>().ToList())
    {
        var grid = child.Child as Grid;
        var idText = grid.Children[0] as TextBlock;
        var nameText = grid.Children[1] as TextBlock;

        if (idText.Text == folderName && nameText.Text == modName)
        {
            AvailableModsPanel.Children.Remove(child);
            break;
        }
    }
}
private void Button_Save_OnClick(object sender, RoutedEventArgs e)
{
    var saveDialog = new Microsoft.Win32.SaveFileDialog
    {
        InitialDirectory = Mod_Folder_Text.Text, // Start in the ModFolder
        Filter = "Text files (*.txt)|*.txt",
        Title = "Save Mod List",
        FileName = "ModList.txt" // Default filename
    };

    if (saveDialog.ShowDialog() == true)
    {
        SaveSelectedModsToFile(saveDialog.FileName);
    }
}

private void SaveSelectedModsToFile(string filePath)
{
    var lines = new List<string>();

    foreach (var child in SelectedModsPanel.Children.OfType<Border>())
    {
        var grid = child.Child as Grid;
        var idText = grid.Children[0] as TextBlock; // Folder Name (ID)
        var nameText = grid.Children[1] as TextBlock; // Mod Name

        string baseFolder;
        string modLine;

        if (idText.Text == "Mods" || nameText.Text == "Mods")
        {
            baseFolder = Mod_Folder_Text.Text;
            modLine = Path.Combine(baseFolder, nameText.Text + ".pak");
        }
        else
        {
            baseFolder = WorkshopModFolder_Text.Text;
            modLine = Path.Combine(baseFolder, idText.Text, nameText.Text + ".pak");
        }

        lines.Add(modLine);
    }

    // Write all lines to the file
    File.WriteAllLines(filePath, lines);

    MessageBox.Show("Mod list saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
}

private void Button_Export_OnClick(object sender, RoutedEventArgs e)
{
    var ids = new List<string>();

    foreach (var child in SelectedModsPanel.Children.OfType<Border>())
    {
        var grid = child.Child as Grid;
        var idText = grid.Children[0] as TextBlock; // Folder Name (ID)

        ids.Add(idText.Text);
    }

    // Join all IDs with commas and set the result to the Import_Export TextBox
    Import_Export.Text = string.Join(", ", ids);
}

private void Button_Import_OnClick(object sender, RoutedEventArgs e)
{
    // Reload the available mods to ensure the panel is up-to-date
    LoadAvailableMods();

    // Clear the SelectedModsPanel before importing
    SelectedModsPanel.Children.Clear();

    // Split the IDs from the Import_Export TextBox and trim spaces
    var ids = Import_Export.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(id => id.Trim())
        .ToArray();

    // Iterate through each ID
    foreach (var id in ids)
    {
        // Try to find the mod in the AvailableModsPanel first
        var modFound = false;

        foreach (var child in AvailableModsPanel.Children.OfType<Border>().ToList())
        {
            var grid = child.Child as Grid;
            var idText = grid.Children[0] as TextBlock;

            if (idText.Text == id)
            {
                // Mod found in AvailableModsPanel, move it to SelectedModsPanel
                AvailableModsPanel.Children.Remove(child);
                SelectedModsPanel.Children.Add(child);

                // Reattach event handlers and context menu
                ReattachHandlers(child);
                
                modFound = true;
                break;
            }
        }

        // If mod is not found in AvailableModsPanel, create a new entry with N/A values
        if (!modFound)
        {
            CreateNAEntry(id);
        }
    }
}



private void CreateNAEntry(string id)
{
    // Initialize variables for Name and Size
    string name = "N/A";
    string size = "N/A";

    // Check if the ID exists in the AvailableModsPanel
    foreach (var child in AvailableModsPanel.Children.OfType<Border>())
    {
        var grid = child.Child as Grid;
        var idText = grid.Children[0] as TextBlock;
        var nameText = grid.Children[1] as TextBlock;
        var sizeText = grid.Children[2] as TextBlock;

        if (idText.Text == id)
        {
            // Found the mod in AvailableModsPanel, get the Name and Size
            name = nameText.Text;
            size = sizeText.Text;
            break;
        }
    }

    // Create a new entry with the found Name and Size, or N/A if not found
    var modRow = new Grid
    {
        Margin = new Thickness(0, 0, 0, 0) // Remove margin between rows
    };

    // Adjusted column widths
    modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // ID column
    modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) }); // Name column
    modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) }); // Size column

    // Add the ID, Name, and Size to the grid
    var idTextBlock = new TextBlock { Text = id ,TextAlignment = TextAlignment.Right};
    var nameTextBlock = new TextBlock { Text = name, Margin = new Thickness(5, 0, 0, 0) };
    var sizeTextBlock = new TextBlock { Text = size,TextAlignment = TextAlignment.Right, Margin = new Thickness(5, 0, 0, 0) };

    Grid.SetColumn(nameTextBlock, 1);
    Grid.SetColumn(sizeTextBlock, 2);

    modRow.Children.Add(idTextBlock);
    modRow.Children.Add(nameTextBlock);
    modRow.Children.Add(sizeTextBlock);

    // Create a Border to wrap the modRow and add a line underneath
    var borderedRow = new Border
    {
        BorderBrush = new SolidColorBrush(Colors.Gray),
        BorderThickness = new Thickness(0, 0, 0, 1), // Line between rows
        Child = modRow
    };

    // Reattach event handlers and context menu to the new row
    ReattachHandlers(borderedRow);

    // Add the new modRow to the Selected Mods panel
    SelectedModsPanel.Children.Add(borderedRow);
}


private void ReattachHandlers(Border modBorder)
{
    var grid = modBorder.Child as Grid;
    var idText = grid.Children[0] as TextBlock;

    // Attach hover events for highlighting
    modBorder.MouseEnter += (s, e) => modBorder.Background = new SolidColorBrush(Colors.LightGray);
    modBorder.MouseLeave += (s, e) => modBorder.Background = new SolidColorBrush(Colors.Transparent);

    // Attach click event handler
    modBorder.MouseLeftButtonDown += ModInfo_MouseLeftButtonDown;

    // Attach context menu for opening Steam page
    AddContextMenu(grid, idText.Text);
}



protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
{
    base.OnClosing(e);

    // Load existing settings if they exist
    string jsonFilePath = "CEMM_Settings.json";
    Dictionary<string, string> settings;

    if (File.Exists(jsonFilePath))
    {
        string json = File.ReadAllText(jsonFilePath);
        settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
    }
    else
    {
        settings = new Dictionary<string, string>();
    }

    // Save the window's current position and size
    settings["WindowTop"] = this.Top.ToString();
    settings["WindowLeft"] = this.Left.ToString();
    settings["WindowHeight"] = this.Height.ToString();
    settings["WindowWidth"] = this.Width.ToString();

    // Save all settings back to the JSON file
    File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
}





    }
}

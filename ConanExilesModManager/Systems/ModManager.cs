using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ConanExilesModManager
{
    public static class ModManager
    {
        public static void LoadAvailableMods(StackPanel availableModsPanel, StackPanel selectedModsPanel, string workshopModFolderPath, MainWindow mainWindow)
        {
            if (Directory.Exists(workshopModFolderPath))
            {
                availableModsPanel.Children.Clear();
                selectedModsPanel.Children.Clear();

                var directories = Directory.GetDirectories(workshopModFolderPath);
                foreach (var dir in directories)
                {
                    var folderName = Path.GetFileName(dir);
                    var pakFiles = Directory.GetFiles(dir, "*.pak");

                    foreach (var pakFile in pakFiles)
                    {
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pakFile);
                        var fileSizeInMB = new FileInfo(pakFile).Length / (1024.0 * 1024.0);

                        var modRow = new Grid { Margin = new Thickness(0) };
                        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
                        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) });
                        modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) });

                        var idText = new TextBlock { Text = folderName, TextAlignment = TextAlignment.Right };
                        var nameText = new TextBlock { Text = fileNameWithoutExtension, Margin = new Thickness(10, 0, 0, 0) };
                        var sizeText = new TextBlock { Text = $"{fileSizeInMB:F2} MB", TextAlignment = TextAlignment.Right, Margin = new Thickness(5, 0, 5, 0) };

                        Grid.SetColumn(nameText, 1);
                        Grid.SetColumn(sizeText, 2);

                        modRow.Children.Add(idText);
                        modRow.Children.Add(nameText);
                        modRow.Children.Add(sizeText);

                        UIManager.AttachHoverEvents(modRow);
                        UIManager.AttachClickEvents(modRow, mainWindow);
                        UIManager.AddContextMenu(modRow, folderName, mainWindow);

                        var borderedRow = new Border
                        {
                            BorderBrush = new SolidColorBrush(Colors.Gray),
                            BorderThickness = new Thickness(0, 0, 0, 1),
                            Child = modRow
                        };

                        availableModsPanel.Children.Add(borderedRow);
                    }
                }
            }
            else
            {
                MessageBox.Show("The WorkshopModFolder path does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void LoadSelectedModsFromTxt(StackPanel selectedModsPanel, StackPanel availableModsPanel, string[] modPaths, string workshopModFolderPath, MainWindow mainWindow)
        {
            
            selectedModsPanel.Children.Clear();
    
            foreach (var modPath in modPaths)
            {
                string modName = Path.GetFileNameWithoutExtension(modPath);
                string folderName = Path.GetFileName(Path.GetDirectoryName(modPath));
                string availableModPath = Path.Combine(workshopModFolderPath, folderName, modName + ".pak");

                bool isAvailable = File.Exists(availableModPath);

                var modRow = new Grid { Margin = new Thickness(0) };
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) });
                modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) });

                var idText = new TextBlock { Text = folderName };
                var nameText = new TextBlock { Text = modName, Margin = new Thickness(10, 0, 0, 0) };
                var sizeText = new TextBlock { Text = isAvailable ? $"{new FileInfo(availableModPath).Length / (1024.0 * 1024.0):F2} MB" : "N/A", Margin = new Thickness(5, 0, 5, 0) };

                Grid.SetColumn(nameText, 1);
                Grid.SetColumn(sizeText, 2);

                modRow.Children.Add(idText);
                modRow.Children.Add(nameText);
                modRow.Children.Add(sizeText);

                if (!isAvailable)
                {
                    modRow.Background = new SolidColorBrush(Colors.LightCoral);
                }
                else
                {
                    UIManager.AttachHoverEvents(modRow);
                }

                UIManager.AttachClickEvents(modRow, mainWindow);
                UIManager.AddContextMenu(modRow, folderName, mainWindow);

                var borderedRow = new Border
                {
                    BorderBrush = new SolidColorBrush(Colors.Gray),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Child = modRow
                };

                selectedModsPanel.Children.Add(borderedRow);

                if (isAvailable)
                {
                    RemoveFromAvailableMods(availableModsPanel, folderName, modName);
                }
            }
        }

        private static void RemoveFromAvailableMods(StackPanel availableModsPanel, string folderName, string modName)
        {
            foreach (var child in availableModsPanel.Children.OfType<Border>().ToList())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock;
                var nameText = grid.Children[1] as TextBlock;

                if (idText.Text == folderName && nameText.Text == modName)
                {
                    availableModsPanel.Children.Remove(child);
                    break;
                }
            }
        }
        public static void ImportMods(StackPanel availableModsPanel, StackPanel selectedModsPanel, string modIdsText, MainWindow mainWindow)
        {
            var ids = modIdsText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(id => id.Trim())
                                .ToArray();

            foreach (var id in ids)
            {
                var modFound = false;

                foreach (var child in availableModsPanel.Children.OfType<Border>().ToList())
                {
                    var grid = child.Child as Grid;
                    var idText = grid.Children[0] as TextBlock;

                    if (idText.Text == id)
                    {
                        availableModsPanel.Children.Remove(child);
                        selectedModsPanel.Children.Add(child);

                        UIManager.ReattachHandlers(child, id, mainWindow);

                        modFound = true;
                        break;
                    }
                }

                if (!modFound)
                {
                    CreateNAEntry(id, availableModsPanel, selectedModsPanel, mainWindow);
                }
            }
        }

        public static void ExportMods(StackPanel selectedModsPanel, TextBox importExportTextBox)
        {
            var ids = new List<string>();

            foreach (var child in selectedModsPanel.Children.OfType<Border>())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock; // Folder Name (ID)
                ids.Add(idText.Text);
            }

            importExportTextBox.Text = string.Join(", ", ids);
        }

        private static void CreateNAEntry(string id, StackPanel availableModsPanel, StackPanel selectedModsPanel, MainWindow mainWindow)
        {
            string name = "N/A";
            string size = "N/A";

            foreach (var child in availableModsPanel.Children.OfType<Border>())
            {
                var grid = child.Child as Grid;
                var idText = grid.Children[0] as TextBlock;
                var nameText = grid.Children[1] as TextBlock;
                var sizeText = grid.Children[2] as TextBlock;

                if (idText.Text == id)
                {
                    name = nameText.Text;
                    size = sizeText.Text;
                    break;
                }
            }

            var modRow = new Grid
            {
                Margin = new Thickness(0)
            };

            modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
            modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.6, GridUnitType.Star) });
            modRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.9, GridUnitType.Star) });

            var idTextBlock = new TextBlock { Text = id, TextAlignment = TextAlignment.Right };
            var nameTextBlock = new TextBlock { Text = name, Margin = new Thickness(5, 0, 0, 0) };
            var sizeTextBlock = new TextBlock { Text = size, TextAlignment = TextAlignment.Right, Margin = new Thickness(5, 0, 0, 0) };

            Grid.SetColumn(nameTextBlock, 1);
            Grid.SetColumn(sizeTextBlock, 2);

            modRow.Children.Add(idTextBlock);
            modRow.Children.Add(nameTextBlock);
            modRow.Children.Add(sizeTextBlock);

            var borderedRow = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = modRow
            };

            UIManager.ReattachHandlers(borderedRow, id, mainWindow);

            selectedModsPanel.Children.Add(borderedRow);
        }
    }
}

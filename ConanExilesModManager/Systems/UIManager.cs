using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ConanExilesModManager
{
    public static class UIManager
    {
        public static void AddContextMenu(Grid modRow, string modId, MainWindow mainWindow)
        {
            var contextMenu = new ContextMenu();

            var openSteamPageMenuItem = new MenuItem { Header = "Open Mod Steam Page" };
            openSteamPageMenuItem.Click += (s, e) => mainWindow.OpenModSteamPage(modId);
            contextMenu.Items.Add(openSteamPageMenuItem);

            var exportAllMenuItem = new MenuItem { Header = "Export" };
            exportAllMenuItem.Click += (s, e) => mainWindow.ExportAllSelectedMods();
            contextMenu.Items.Add(exportAllMenuItem);

            modRow.ContextMenu = contextMenu;
        }

        public static void AttachHoverEvents(Grid modRow)
        {
            modRow.MouseEnter += (s, e) => modRow.Background = new SolidColorBrush(Colors.LightGray);
            modRow.MouseLeave += (s, e) => modRow.Background = new SolidColorBrush(Colors.Transparent);
        }

        public static void AttachClickEvents(Grid modRow, MainWindow mainWindow)
        {
            modRow.MouseLeftButtonDown += mainWindow.ModInfo_MouseLeftButtonDown;
        }

        public static void SortMods(string column, StackPanel panel, ref bool isAscending)
        {
            var rows = panel.Children.OfType<Border>().ToList();
            IOrderedEnumerable<Border> sortedRows = null;

            switch (column)
            {
                case "ID":
                    sortedRows = isAscending ?
                        rows.OrderBy(row => ((row.Child as Grid).Children[0] as TextBlock).Text) :
                        rows.OrderByDescending(row => ((row.Child as Grid).Children[0] as TextBlock).Text);
                    break;

                case "Name":
                    sortedRows = isAscending ?
                        rows.OrderBy(row => ((row.Child as Grid).Children[1] as TextBlock).Text) :
                        rows.OrderByDescending(row => ((row.Child as Grid).Children[1] as TextBlock).Text);
                    break;

                case "Size":
                    sortedRows = isAscending ?
                        rows.OrderBy(row => Convert.ToDouble(((row.Child as Grid).Children[2] as TextBlock).Text.Replace(" MB", ""))) :
                        rows.OrderByDescending(row => Convert.ToDouble(((row.Child as Grid).Children[2] as TextBlock).Text.Replace(" MB", "")));
                    break;
            }

            isAscending = !isAscending;

            panel.Children.Clear();
            foreach (var row in sortedRows)
            {
                panel.Children.Add(row);
            }
        }
        public static void ReattachHandlers(Border modBorder, string modId, MainWindow mainWindow)
        {
            var grid = modBorder.Child as Grid;

            modBorder.MouseEnter += (s, e) => modBorder.Background = new SolidColorBrush(Colors.LightGray);
            modBorder.MouseLeave += (s, e) => modBorder.Background = new SolidColorBrush(Colors.Transparent);

            modBorder.MouseLeftButtonDown += mainWindow.ModInfo_MouseLeftButtonDown;

            AddContextMenu(grid, modId, mainWindow);
        }
    }
}

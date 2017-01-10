using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.ViewModels;

namespace MegaApp.Services
{
    internal static class DialogService
    {
        public static async Task ShowAlertAsync(string title, string content)
        {
            var dialog = new MessageDialog(content, title);
            await UiService.OnUiThread(async() => await dialog.ShowAsync());
        }

        public static async Task<bool> ShowOkCancelAsync(string title, string content)
        {
            var dialog = new MessageDialog(content, title);
            dialog.Commands.Add(new UICommand() { Id = true, Label = ResourceService.UiResources.GetString("UI_Ok") });
            dialog.Commands.Add(new UICommand() { Id = false, Label = ResourceService.UiResources.GetString("UI_Cancel") });
            dialog.CancelCommandIndex = 1;
            dialog.DefaultCommandIndex = 1;
            var result = await dialog.ShowAsync();
            return (bool) result.Id;
        }

        public static async void ShowOverquotaAlert()
        {
            await ShowAlertAsync("Overquota Alert!",
                "Operation not allowed, you will exceed the storage limit of your account.");

            //var customMessageDialog = new CustomMessageDialog(
            //    ResourceService.AppMessages.GetString("AM_OverquotaAlert_Title"),
            //    ResourceService.AppMessages.GetString("AM_OverquotaAlert")
            //    App.AppInformation, MessageDialogButtons.YesNo);

            //customMessageDialog.OkOrYesButtonTapped += (sender, args) =>
            //{
            //    ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(
            //        new Uri("/Pages/MyAccountPage.xaml?Pivot=1", UriKind.RelativeOrAbsolute));
            //};

            //customMessageDialog.ShowDialog();
        }

        public static async Task<string> ShowInputDialogAsync(string title, string message,
            InputDialogSettings settings = null)
        {
            if (settings == null)
                settings = new InputDialogSettings();

            var dialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = ResourceService.UiResources.GetString("UI_Ok"),
                SecondaryButtonText = ResourceService.UiResources.GetString("UI_Cancel"),
                Title = title
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0)
            };
            var messageText = new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 0, 0, 12),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var input = new TextBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Text = settings.InputText,
                SelectionStart = 0,
            };

            if (settings.IgnoreExtensionInSelection)
            {
                var fileName = Path.GetFileNameWithoutExtension(settings.InputText);
                input.SelectionLength = fileName.Length;
            }
            else
            {
                input.SelectionLength = settings.InputText.Length;
            }

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(input);
            dialog.Content = stackPanel;
            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    return null;
                case ContentDialogResult.Primary:
                    return input.Text;
                case ContentDialogResult.Secondary:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates the sort menu with all the sort options.
        /// </summary>
        /// <param name="folder">Folder to sort.</param>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateSortMenu(FolderViewModel folder)
        {
            MenuFlyout menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameAscending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_ALPHABETICAL_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_ALPHABETICAL_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNameDescending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_ALPHABETICAL_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_ALPHABETICAL_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionLargest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_SIZE_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_SIZE_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionSmallest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_SIZE_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_SIZE_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionNewest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_MODIFICATION_DESC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionOldest"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_MODIFICATION_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_MODIFICATION_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionFilesAscending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_DEFAULT_ASC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_DEFAULT_ASC);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionFilesDescending"),
                Foreground = GetSortMenuItemForeground(folder, (int)MSortOrderType.ORDER_DEFAULT_DESC),
                Command = new RelayCommand(() =>
                {
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle,
                        (int)MSortOrderType.ORDER_DEFAULT_DESC);
                    folder.LoadChildNodes();
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort menu item foreground color depending on the current sort order of the folder.
        /// </summary>
        /// <param name="folder">Folder to check the current sort order.</param>
        /// <param name="sortOrder">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetSortMenuItemForeground(FolderViewModel folder, int sortOrder)
        {
            if (UiService.GetSortOrder(folder.FolderRootNode.Base64Handle, folder.FolderRootNode.Name) == sortOrder)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }
    }

    public class InputDialogSettings
    {
        public string InputText { get; set; } = string.Empty;

        public bool IsTextSelected { get; set; }

        public bool IgnoreExtensionInSelection { get; set; }
    }
}

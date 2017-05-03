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
    /// <summary>
    /// Service to display Dialogs to the user
    /// </summary>
    internal static class DialogService
    {
        /// <summary>
        /// Show an Alert Dialog that can be dismissed by the "OK" button.
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="content">Content message of the dialog</param>
        public static async Task ShowAlertAsync(string title, string content)
        {
            var dialog = new MessageDialog(content, title);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Show a dialog that has an "OK" and a "Cancel" button option
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="content">Content message of the dialog</param>
        /// <returns>True if the "OK" button is pressed, else False</returns>
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

        public static async void ShowTransferOverquotaWarning()
        {
            await ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning_Title"),
                ResourceService.AppMessages.GetString("AM_TransferOverquotaWarning"));
        }

        /// <summary>
        /// Show an Input Dialog to the uses
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="content">Content message of the dialog</param>
        /// <param name="settings">Input dialog behavior/option settings</param>
        /// <returns>The value of the input dialog when primary button was pressed, else NULL</returns>
        public static async Task<string> ShowInputDialogAsync(string title, string content,
            InputDialogSettings settings = null)
        {
            // Create default input settings if null
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
                Text = content,
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
        /// Shows a dialog to allow copy a node link to the clipboard or share it using other app
        /// </summary>
        /// <param name="node">Node to share the link</param>
        public static async void ShowShareLink(NodeViewModel node)
        {
            var dialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = ResourceService.UiResources.GetString("UI_Copy"),
                SecondaryButtonText = ResourceService.UiResources.GetString("UI_Share"),
                Title = ResourceService.UiResources.GetString("UI_ExportLink")
            };

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(0, 20, 0, 0)
            };

            var messageText = new TextBlock
            {
                Text = node.OriginalMNode.getPublicLink(true),
                Margin = new Thickness(0, 20, 0, 12),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var linkWithoutKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithoutKey")
            };
            linkWithoutKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(false);

            var decryptionKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_DecryptionKey")
            };
            decryptionKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getBase64Key();

            var linkWithKey = new RadioButton
            {
                Content = ResourceService.UiResources.GetString("UI_LinkWithKey"),
                IsChecked = true
            };
            linkWithKey.Checked += (sender, args) => messageText.Text = node.OriginalMNode.getPublicLink(true);

            stackPanel.Children.Add(linkWithoutKey);
            stackPanel.Children.Add(decryptionKey);
            stackPanel.Children.Add(linkWithKey);

            var stackPanelLinkWithExpirationDate = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var linkWithExpirationDateLabel = new TextBlock
            {
                Text = node.SetLinkExpirationDateText,
                Margin = new Thickness(0, 20, 0, 8),
                TextWrapping = TextWrapping.WrapWholeWords,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            var enableLinkExpirationDateSwitch = new ToggleSwitch
            {
                IsOn = node.LinkWithExpirationTime,
                IsEnabled = AccountService.AccountDetails.IsProAccount
            };

            var expirationDateCalendarDatePicker = new CalendarDatePicker
            {
                IsEnabled = enableLinkExpirationDateSwitch.IsOn,
                DateFormat = "{day.integer(2)}‎/‎{month.integer(2)}‎/‎{year.full}",
                Date = node.LinkExpirationDate
            };
            expirationDateCalendarDatePicker.Opened += (sender, args) =>
            {
                expirationDateCalendarDatePicker.LightDismissOverlayMode = LightDismissOverlayMode.On;
                expirationDateCalendarDatePicker.MinDate = DateTime.Today.AddDays(1);                
            };
            expirationDateCalendarDatePicker.DateChanged += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsCalendarOpen = false;

                if (expirationDateCalendarDatePicker.Date == null)
                {
                    enableLinkExpirationDateSwitch.IsOn = false;
                    if (node.LinkExpirationTime > 0)
                        node.SetLinkExpirationTime(0);
                }
                else if (node.LinkExpirationDate == null ||
                    !node.LinkExpirationDate.Value.ToUniversalTime().Equals(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime()))
                {
                    node.SetLinkExpirationTime(expirationDateCalendarDatePicker.Date.Value.ToUniversalTime().ToUnixTimeSeconds());
                }
            };

            enableLinkExpirationDateSwitch.Toggled += (sender, args) =>
            {
                expirationDateCalendarDatePicker.IsEnabled = enableLinkExpirationDateSwitch.IsOn;
                if (enableLinkExpirationDateSwitch.IsOn)
                    expirationDateCalendarDatePicker.Date = node.LinkExpirationDate;
                else
                    expirationDateCalendarDatePicker.Date = null;
            };

            stackPanelLinkWithExpirationDate.Children.Add(enableLinkExpirationDateSwitch);
            stackPanelLinkWithExpirationDate.Children.Add(expirationDateCalendarDatePicker);

            stackPanel.Children.Add(linkWithExpirationDateLabel);
            stackPanel.Children.Add(stackPanelLinkWithExpirationDate);
            stackPanel.Children.Add(messageText);
            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();
            switch (result)
            {
                case ContentDialogResult.None:
                    break;

                case ContentDialogResult.Primary:
                    ShareService.CopyLinkToClipboard(messageText.Text);
                    break;

                case ContentDialogResult.Secondary:
                    ShareService.ShareLink(messageText.Text);
                    break;

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
        /// <summary>
        /// Default text for the input in the dialog
        /// </summary>
        public string InputText { get; set; } = string.Empty;

        /// <summary>
        /// Is the text in the input selected as default
        /// </summary>
        public bool IsTextSelected { get; set; }

        /// <summary>
        /// Ignore extensions when the text is default selected.
        /// </summary>
        public bool IgnoreExtensionInSelection { get; set; }
    }
}

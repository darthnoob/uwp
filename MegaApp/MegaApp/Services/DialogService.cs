using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using mega;
using MegaApp.Classes;
using MegaApp.Enums;
using MegaApp.Extensions;
using MegaApp.ViewModels;
using MegaApp.ViewModels.Contacts;
using MegaApp.ViewModels.Dialogs;
using MegaApp.ViewModels.MyAccount;
using MegaApp.ViewModels.SharedFolders;
using MegaApp.Views;
using MegaApp.Views.Dialogs;

namespace MegaApp.Services
{
    /// <summary>
    /// Service to display Dialogs to the user
    /// </summary>
    internal static class DialogService
    {
        /// <summary>
        /// Show an Alert Dialog that can be dismissed by a button.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Content message of the dialog.</param>
        public static async Task ShowAlertAsync(string title, string message)
        {
            var dialog = new AlertDialog(title, message);
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show an Alert Dialog that can be dismissed by a button.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Content message of the dialog.</param>
        /// <param name="button">Label of the dialog button.</param>
        public static async Task ShowAlertAsync(string title, string message, string button)
        {
            var dialog = new AlertDialog(title, message, button);
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Show a dialog with a message and "Ok"/"Cancel" buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <returns>True if the "Ok" button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message)
        {
            var dialog = new OkCancelDialog(title, message);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="dialogButtons">A <see cref="OkCancelDialogButtons"/> value that indicates the buttons to display.</param>
        /// <param name="primaryButton">Label for the primary button.</param>
        /// <param name="secondaryButton">Label for the secondary button.</param>
        /// <returns>True if the primary button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message,
            OkCancelDialogButtons dialogButtons, string primaryButton = null, string secondaryButton = null)
        {
            var dialog = new OkCancelDialog(title, message, null, 
                dialogButtons, primaryButton, secondaryButton);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message, a warning and "Ok"/"Cancel" buttons.
        /// </summary>
        /// <param name="title">Title of the dialog.</param>
        /// <param name="message">Message of the dialog.</param>
        /// <param name="warning">Warning of the dialog.</param>
        /// <returns>True if the "Ok" button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message, string warning)
        {
            var dialog = new OkCancelDialog(title, message, warning);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show a dialog with a message, a warning and two buttons.
        /// </summary>
        /// <param name="title">Title of the dialog</param>
        /// <param name="message">Message of the dialog</param>
        /// <param name="warning">Warning of the dialog</param>
        /// <param name="dialogButtons">A <see cref="OkCancelDialogButtons"/> value that indicates the buttons to display.</param>
        /// <param name="primaryButton">Label for the primary button.</param>
        /// <param name="secondaryButton">Label for the secondary button.</param>
        /// <returns>True if the primary button is pressed, else False.</returns>
        public static async Task<bool> ShowOkCancelAsync(string title, string message, string warning,
            OkCancelDialogButtons dialogButtons, string primaryButton = null, string secondaryButton = null)
        {
            var dialog = new OkCancelDialog(title, message, warning,
                dialogButtons, primaryButton, secondaryButton);
            return await dialog.ShowAsyncQueueBool();
        }

        /// <summary>
        /// Show an standard input dialog.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public static async Task<string> ShowInputDialogAsync(string title, string message,
            InputDialogSettings settings = null)
        {
            var dialog = new InputDialog(title, message, null, null, settings);
            var result = await dialog.ShowAsyncQueueBool();
            return result ? dialog.ViewModel.InputText : null;
        }

        /// <summary>
        /// Show an standard input dialog.
        /// </summary>
        /// <param name="title">Title of the input dialog.</param>
        /// <param name="message">Message of the input dialog.</param>
        /// <param name="primaryButton">Label of the primary button of the input dialog.</param>
        /// <param name="secondaryButton">Label of the secondary button of the input dialog.</param>
        /// <param name="settings">Input dialog behavior/option settings.</param>
        public static async Task<string> ShowInputDialogAsync(string title, string message,
            string primaryButton, string secondaryButton, InputDialogSettings settings = null)
        {
            var dialog = new InputDialog(title, message, primaryButton, secondaryButton, settings);
            var result = await dialog.ShowAsyncQueueBool();
            return result ? dialog.ViewModel.InputText : null;
        }

        public static async void ShowOverquotaAlert()
        {
            var result = await ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_OverquotaAlert_Title"),
                ResourceService.AppMessages.GetString("AM_OverquotaAlert"),
                OkCancelDialogButtons.YesNo);

            if (!result) return;

            UiService.OnUiThread(() =>
            {
                NavigateService.Instance.Navigate(typeof(MyAccountPage), false,
                    NavigationObject.Create(typeof(MainViewModel), NavigationActionType.Upgrade));
            });
        }

        public static async void ShowTransferOverquotaWarning()
        {
            var dialog = new TransferOverquotaWarningDialog();
            await dialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Shows an alert dialog to inform that the DEBUG mode is enabled.
        /// <para>Also asks if the user wants to disable it.</para>
        /// </summary>
        public static async void ShowDebugModeAlert()
        {
            var result = await ShowOkCancelAsync(
                ResourceService.AppMessages.GetString("AM_DebugModeEnabled_Title"),
                ResourceService.AppMessages.GetString("AM_DebugModeEnabled_Message"),
                OkCancelDialogButtons.YesNo);

            if (result)
                DebugService.DebugSettings.DisableDebugMode();

            DebugService.DebugSettings.ShowDebugAlert = false;
        }

        /// <summary>
        /// Storage the instance of the <see cref="AwaitEmailConfirmationDialog"/>
        /// </summary>
        private static AwaitEmailConfirmationDialog awaitEmailConfirmationDialog;

        /// <summary>
        /// Show a dialog indicating that is waiting for an email confirmation
        /// </summary>
        /// <param name="email">Email for which is waiting confirmation</param>
        public static async void ShowAwaitEmailConfirmationDialog(string email)
        {
            if (awaitEmailConfirmationDialog == null)
                awaitEmailConfirmationDialog = new AwaitEmailConfirmationDialog(email);
            else
                awaitEmailConfirmationDialog.ViewModel.Email = email;

            await awaitEmailConfirmationDialog.ShowAsyncQueue();
        }

        private static AchievementInformationDialog achievementInformationDialog;

        public static async void ShowAchievementInformationDialog(AwardClassViewModel award)
        {
            if (achievementInformationDialog == null)
                achievementInformationDialog = new AchievementInformationDialog(award);
            else
                achievementInformationDialog.ViewModel.Award = award;

            await achievementInformationDialog.ShowAsyncQueue();
        }

        /// <summary>
        /// Close the await email confirmation dialog if exists
        /// </summary>
        public static void CloseAwaitEmailConfirmationDialog()
        {
            awaitEmailConfirmationDialog?.Hide();
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

            var result = await dialog.ShowAsyncQueue();
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
            var currentSortOrder = UiService.GetSortOrder(folder?.FolderRootNode?.Base64Handle, folder?.FolderRootNode?.Name);

            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_ALPHABETICAL_ASC : MSortOrderType.ORDER_ALPHABETICAL_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionSize"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_SIZE),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_SIZE_ASC : MSortOrderType.ORDER_SIZE_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionDateModified"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_MODIFICATION),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_MODIFICATION_ASC : MSortOrderType.ORDER_MODIFICATION_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionType"),
                Foreground = GetSortMenuItemForeground(currentSortOrder, NodesSortOrderType.ORDER_TYPE),
                Command = new RelayCommand(() =>
                {
                    var newOrder = folder != null && folder.ItemCollection.IsCurrentOrderAscending ?
                        MSortOrderType.ORDER_DEFAULT_ASC : MSortOrderType.ORDER_DEFAULT_DESC;
                    if (folder == null) return;
                    UiService.SetSortOrder(folder.FolderRootNode.Base64Handle, newOrder);
                    folder.LoadChildNodes();
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Gets the sort menu item foreground color depending on the current sort order.
        /// </summary>
        /// <param name="currentSortOrder">Current sort order of the list/collection.</param>
        /// <param name="sortOrderToCheck">Sort order to check.</param>
        /// <returns>The brush object with the color.</returns>
        private static Brush GetSortMenuItemForeground(object currentSortOrder, object sortOrderToCheck)
        {
            if (currentSortOrder is MSortOrderType && sortOrderToCheck is NodesSortOrderType)
            {
                switch ((MSortOrderType)currentSortOrder)
                {
                    case MSortOrderType.ORDER_ALPHABETICAL_ASC:
                    case MSortOrderType.ORDER_ALPHABETICAL_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_NAME)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_CREATION_ASC:
                    case MSortOrderType.ORDER_CREATION_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_CREATION)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_DEFAULT_ASC:
                    case MSortOrderType.ORDER_DEFAULT_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_TYPE)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_MODIFICATION_ASC:
                    case MSortOrderType.ORDER_MODIFICATION_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_MODIFICATION)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;

                    case MSortOrderType.ORDER_SIZE_ASC:
                    case MSortOrderType.ORDER_SIZE_DESC:
                        if ((NodesSortOrderType)sortOrderToCheck == NodesSortOrderType.ORDER_SIZE)
                            return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];
                        break;
                }
            }

            if (currentSortOrder is MSortOrderType && sortOrderToCheck is MSortOrderType &&
                (MSortOrderType)currentSortOrder == (MSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is ContactsSortOrderType && sortOrderToCheck is ContactsSortOrderType &&
                (ContactsSortOrderType)currentSortOrder == (ContactsSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is ContactRerquestsSortOrderType && sortOrderToCheck is ContactRerquestsSortOrderType &&
                (ContactRerquestsSortOrderType)currentSortOrder == (ContactRerquestsSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is IncomingSharesSortOrderType && sortOrderToCheck is IncomingSharesSortOrderType &&
                (IncomingSharesSortOrderType)currentSortOrder == (IncomingSharesSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            if (currentSortOrder is OutgoingSharesSortOrderType && sortOrderToCheck is OutgoingSharesSortOrderType &&
                (OutgoingSharesSortOrderType)currentSortOrder == (OutgoingSharesSortOrderType)sortOrderToCheck)
                return (SolidColorBrush)Application.Current.Resources["MegaRedColorBrush"];

            return (SolidColorBrush)Application.Current.Resources["MegaAppForegroundBrush"];
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactsSortMenu(ContactsListViewModel contacts,
            bool showReferralStatusSort = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_NAME;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionEmail"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_EMAIL),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_EMAIL;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            if (showReferralStatusSort)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionReferralStatus"),
                    Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_STATUS),
                    Command = new RelayCommand(() =>
                    {
                        contacts.CurrentOrder = ContactsSortOrderType.ORDER_STATUS;
                        contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                    })
                });
            }

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for contacts.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateInviteContactsSortMenu(ContactsListViewModel contacts)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_NAME;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionReferralStatus"),
                Foreground = GetSortMenuItemForeground(contacts.CurrentOrder, ContactsSortOrderType.ORDER_STATUS),
                Command = new RelayCommand(() =>
                {
                    contacts.CurrentOrder = ContactsSortOrderType.ORDER_STATUS;
                    contacts.SortBy(contacts.CurrentOrder, contacts.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for contact requests.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateContactRequestsSortMenu(ContactRequestsListViewModel contactRequests)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(contactRequests.CurrentOrder, ContactRerquestsSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    contactRequests.CurrentOrder = ContactRerquestsSortOrderType.ORDER_NAME;
                    contactRequests.SortBy(contactRequests.CurrentOrder, contactRequests.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for incoming shared items.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateIncomingSharedItemsSortMenu(IncomingSharesViewModel sharedItems, 
            bool areContactIncomingShares = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_NAME;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionDateModified"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_MODIFICATION),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_MODIFICATION;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionAccessLevel"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_ACCESS),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_ACCESS;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            if(!areContactIncomingShares)
            {
                menuFlyout.Items?.Add(new MenuFlyoutItem()
                {
                    Text = ResourceService.UiResources.GetString("UI_SortOptionOwner"),
                    Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, IncomingSharesSortOrderType.ORDER_OWNER),
                    Command = new RelayCommand(() =>
                    {
                        sharedItems.CurrentOrder = IncomingSharesSortOrderType.ORDER_OWNER;
                        sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                    })
                });
            }

            return menuFlyout;
        }

        /// <summary>
        /// Creates a sort menu for outgoing shared items.
        /// </summary>
        /// <returns>The flyout menu with the sort options.</returns>
        public static MenuFlyout CreateOutgoingSharedItemsSortMenu(OutgoingSharesViewModel sharedItems,
            bool areContactIncomingShares = false)
        {
            MenuFlyout menuFlyout = new MenuFlyout();

            menuFlyout.Items?.Add(new MenuFlyoutItem()
            {
                Text = ResourceService.UiResources.GetString("UI_SortOptionName"),
                Foreground = GetSortMenuItemForeground(sharedItems.CurrentOrder, OutgoingSharesSortOrderType.ORDER_NAME),
                Command = new RelayCommand(() =>
                {
                    sharedItems.CurrentOrder = OutgoingSharesSortOrderType.ORDER_NAME;
                    sharedItems.SortBy(sharedItems.CurrentOrder, sharedItems.ItemCollection.CurrentOrderDirection);
                })
            });

            return menuFlyout;
        }
    }
}

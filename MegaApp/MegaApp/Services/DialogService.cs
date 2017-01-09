using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
    }

    public class InputDialogSettings
    {
        public string InputText { get; set; } = string.Empty;

        public bool IsTextSelected { get; set; }

        public bool IgnoreExtensionInSelection { get; set; }
    }
}

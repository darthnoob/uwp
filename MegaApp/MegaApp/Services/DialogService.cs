using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace MegaApp.Services
{
    internal static class DialogService
    {
        public static async Task ShowAlertAsync(string title, string content)
        {
            var dialog = new MessageDialog(content, title);
            await UiService.OnUiThread(async() => await dialog.ShowAsync());
        }
    }
}

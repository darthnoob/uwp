using System;
using Windows.ApplicationModel.DataTransfer;

namespace MegaApp.Services
{
    internal static class ShareService
    {
        /// <summary>
        /// Copy a link to the clipboard
        /// </summary>
        /// <param name="link">Link to copy</param>
        public static void CopyLinkToClipboard(string link)
        {
            try
            {
                var data = new DataPackage();
                data.SetText(link);
                Clipboard.SetContent(data);

                UiService.OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_LinkCopiedToClipboard_Title"),
                        ResourceService.AppMessages.GetString("AM_LinkCopiedToClipboard"));
                });
            }
            catch (Exception)
            {
                UiService.OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_CopyLinkToClipboardFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_CopyLinkToClipboardFailed"));
                });
            }
        }

        /// <summary>
        /// Share a link with other external app
        /// </summary>
        /// <param name="link">Link to share</param>
        public static void ShareLink(string link)
        {
            DataTransferManager.GetForCurrentView().DataRequested += (sender, args) =>
            {
                args.Request.Data.Properties.Title = ResourceService.AppMessages.GetString("AM_ShareLinkFromMega_Title");
                args.Request.Data.Properties.Description = ResourceService.AppMessages.GetString("AM_ShareLinkFromMega");
                args.Request.Data.SetText(link);
            };

            try { DataTransferManager.ShowShareUI(); }
            catch (Exception)
            {
                UiService.OnUiThread(async () =>
                {
                    await DialogService.ShowAlertAsync(
                        ResourceService.AppMessages.GetString("AM_ShareLinkFromMegaFailed_Title"),
                        ResourceService.AppMessages.GetString("AM_ShareLinkFromMegaFailed"));
                });
            }
        }
    }
}

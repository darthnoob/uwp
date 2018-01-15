using System.Threading.Tasks;

namespace MegaApp.Services
{
    public static class PublicLinkService
    {
        #region Properties

        public static string Link;

        #endregion

        #region Methods

        public static async Task<string> ShowDecryptionAlertAsync()
        {
            var decryptionKey = await DialogService.ShowInputDialogAsync(
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertTitle"),
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertMessage"));

            if (decryptionKey == null) return null;
            if (string.IsNullOrWhiteSpace(decryptionKey)) return Link;
            return FormatLink(decryptionKey);
        }

        public static async Task<string> ShowDecryptionKeyNotValidAlertAsync()
        {
            var decryptionKey = await DialogService.ShowInputDialogAsync(
                ResourceService.AppMessages.GetString("AM_DecryptionKeyNotValid"),
                ResourceService.AppMessages.GetString("AM_DecryptionKeyAlertMessage"));

            if (decryptionKey == null) return null;
            if (string.IsNullOrWhiteSpace(decryptionKey)) return Link;
            return FormatLink(decryptionKey);
        }

        public static async void ShowLinkNoValidAlert()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_OpenLinkFailed_Title"),
                ResourceService.AppMessages.GetString("AM_InvalidLink"));
        }

        public static async void ShowAssociatedUserAccountTerminatedFileLinkAlert()
        {
            await DialogService.ShowAlertAsync(
                ResourceService.AppMessages.GetString("AM_LinkUnavailableTitle"),
                ResourceService.AppMessages.GetString("AM_AssociatedUserAccountTerminated"));
        }

        /// <summary>
        /// Format a MEGA file link providing its decryption key.        
        /// </summary>        
        /// <param name="decryptionKey">Decryption key of the link.</param>
        private static string FormatLink(string decryptionKey)
        {
            string[] splittedLink = SplitLink(Link);

            // If the decryption key already includes the "!" character, delete it.
            if (decryptionKey.StartsWith("!"))
                decryptionKey = decryptionKey.Substring(1);

            string link = string.Format("{0}!{1}!{2}", splittedLink[0],
                splittedLink[1], decryptionKey);

            return link;
        }

        /// <summary>
        /// Split the MEGA link in its three parts, separated by the "!" chartacter.
        /// <para>1. MEGA Url address.</para>
        /// <para>2. Node handle.</para>
        /// <para>3. Decryption key.</para>
        /// </summary>        
        /// <param name="link">Link to split.</param>
        /// <returns>Char array with the parts of the link.</returns>
        private static string[] SplitLink(string link)
        {
            string delimStr = "!";
            return link.Split(delimStr.ToCharArray(), 3);
        }

        #endregion        
    }
}

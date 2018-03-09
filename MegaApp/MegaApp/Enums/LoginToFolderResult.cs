namespace MegaApp.Enums
{
    /// <summary>
    /// Possible results of a "login to folder" request.
    /// </summary>
    public enum LoginToFolderResult
    {
        Success,                        // Request was successfull.
        InvalidHandleOrDecryptionKey,   // Folder link Handle length or Key length no valid.
        InvalidDecryptionKey,           // Folder link no valid decryption key.
        NoDecryptionKey,                // Folder link has not decryption key.
        Unknown                         // Unknown error.
    }
}

namespace MegaApp.Enums
{
    public enum GetPublicNodeResult
    {
        Success,                            // Request was successfull
        InvalidHandleOrDecryptionKey,       // Handle length or Key length no valid
        InvalidDecryptionKey,               // No valid decryption key
        NoDecryptionKey,                    // Link has not decryption key
        UnavailableLink,                    // Taken down link or link not exists or has been deleted by user.
        AssociatedUserAccountTerminated,    // Taken down link and the link owner's account is blocked
        Unknown                             // Unknown error
    }
}

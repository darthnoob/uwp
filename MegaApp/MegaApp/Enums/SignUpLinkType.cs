namespace MegaApp.Enums
{
    public enum SignUpLinkType
    {
        Valid,              // Valid and operative confirmation link.
        AutoConfirmed,      // Valid confirmation link. Auto confirmed account.
        Invalid,            // Incomplete confirmation link.
        AlreadyConfirmed,   // Already confirmed account.
        Expired,            // Expired confirmation link.
        Unknown             // Unknown error.
    }
}

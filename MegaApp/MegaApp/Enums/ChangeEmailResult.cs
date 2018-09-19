namespace MegaApp.Enums
{
    public enum ChangeEmailResult
    {
        Success,                    // Successfull get change email link process
        AlreadyRequested,           // Change email already requested
        UserNotLoggedIn,            // No user is logged in
        MultiFactorAuthInvalidCode, // Invalid Multi-factor authentication code.
        Unknown                     // Unknown result, but not successful
    }
}
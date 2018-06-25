namespace MegaApp.Enums
{
    public enum ChangeEmailResult
    {
        Success,            // Successfull get change email link process
        AlreadyRequested,   // Change email already requested
        UserNotLoggedIn,    // No user is logged in
        MultiFactorAuth,    // Invalid MFA code.
        Unknown             // Unknown result, but not successful
    }
}
namespace MegaApp.Enums
{
    public enum ChangePasswordResult
    {
        Success,                            // Successful change password.
        MultiFactorAuth,                    // Invalid MFA code.
        Unknown                             // Unknown result, but not successful.
    }
}

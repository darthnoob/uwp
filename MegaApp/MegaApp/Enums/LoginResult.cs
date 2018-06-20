namespace MegaApp.Enums
{
    public enum LoginResult
    {
        Success,                            // Successful login.
        UnassociatedEmailOrWrongPassword,   // Email unassociated with a MEGA account or Wrong password.
        TooManyLoginAttempts,               // Too many failed login attempts. Wait one hour.
        AccountNotConfirmed,                // Account not confirmed.
        MultiFactorAuth,                    // Invalid MFA code.
        Unknown                             // Unknown result, but not successful.
    }
}
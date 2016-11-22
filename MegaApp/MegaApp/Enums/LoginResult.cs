namespace MegaApp.Enums
{
    public enum LoginResult
    {
        Success,
        UnassociatedEmailOrWrongPassword,
        TooManyLoginAttempts,
        AccountNotConfirmed,
        Unknown
    }
}
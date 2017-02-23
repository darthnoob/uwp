namespace MegaApp.Enums
{
    public enum TransferStatus
    {
        Downloading = 0,
        Uploading   = 10,
        Paused      = 30,
        Queued      = 50,
        Preparing   = 55,        
        NotStarted  = 60,
        Downloaded  = 100,
        Uploaded    = 110,
        Canceled    = 140,        
        Error       = 999
    }
}
namespace MegaApp.Classes
{
    public class AppInformation
    {
        public AppInformation()
        {
            PickerOrAsyncDialogIsOpen = false;
            IsNewlyActivatedAccount = false;
            IsStartedAsAutoUpload = false;
            IsStartupModeActivate = false;

            HasPinLockIntroduced = false;
        }
        
        public bool PickerOrAsyncDialogIsOpen { get; set; }
        public bool IsNewlyActivatedAccount { get; set; }
        public bool IsStartedAsAutoUpload { get; set; }
        public bool IsStartupModeActivate { get; set; }
        
        public bool HasPinLockIntroduced { get; set; }
    }
}

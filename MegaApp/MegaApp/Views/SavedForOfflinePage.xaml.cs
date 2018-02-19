using MegaApp.UserControls;
using MegaApp.ViewModels;

namespace MegaApp.Views
{
    // Helper class to define the viewmodel of this page
    // XAML cannot use generics in it's declaration.
    public class BaseSavedForOfflinePage : PageEx<SavedForOfflineViewModel> { }

    public sealed partial class SavedForOfflinePage : BaseSavedForOfflinePage
    {
        public SavedForOfflinePage()
        {
            this.InitializeComponent();
        }
    }
}

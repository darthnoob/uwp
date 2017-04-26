using Windows.ApplicationModel.Resources;

namespace MegaApp.Services
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public static class ResourceService
    {
        public static ResourceLoader AppMessages { get; } = ResourceLoader.GetForViewIndependentUse("AppMessages");

        public static ResourceLoader AppResources { get; } = ResourceLoader.GetForViewIndependentUse("AppResources");

        public static ResourceLoader EmptyStates { get; } = ResourceLoader.GetForViewIndependentUse("EmptyStates");

        public static ResourceLoader ProgressMessages { get; } = ResourceLoader.GetForViewIndependentUse("ProgressMessages");

        public static ResourceLoader SettingsResources { get; } = ResourceLoader.GetForViewIndependentUse("SettingsResources");

        public static ResourceLoader UiResources { get; } = ResourceLoader.GetForViewIndependentUse("UiResources");

        public static ResourceLoader VisualResources { get; } = ResourceLoader.GetForViewIndependentUse("VisualResources");
    }
}
using Windows.ApplicationModel.Resources;

namespace CameraUploadService.Services
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    internal static class ResourceService
    {
        public static ResourceLoader SettingsResources { get; } = ResourceLoader.GetForViewIndependentUse("SettingsResources");
    }
}
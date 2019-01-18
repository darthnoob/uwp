#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.Enums
#else
namespace MegaApp.Enums
#endif
{
    public enum CameraUploadsFileType
    {
        PhotoAndVideo   = 0,
        PhotoOnly       = 1,
        VideoOnly       = 2
    }
}

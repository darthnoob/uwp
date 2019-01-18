#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.Enums
#else
namespace MegaApp.Enums
#endif
{
    public enum CameraUploadsConnectionType
    {
        EthernetWifiOnly    = 0,
        Any                 = 1
    }
}

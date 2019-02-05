using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using mega;

#if CAMERA_UPLOADS_SERVICE
namespace BackgroundTaskService.MegaApi
#else
namespace MegaApp.MegaApi
#endif
{
    class MegaRandomNumberProvider : MRandomNumberProvider
    {
        public virtual void GenerateRandomBlock(byte[] value)
        {
            CryptographicBuffer.GenerateRandom(Convert.ToUInt32(value.Length)).ToArray();
        }
    }
}

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using mega;

namespace CameraUploadService.MegaApi
{
    internal class MegaRandomNumberProvider : MRandomNumberProvider
    {
        public virtual void GenerateRandomBlock(byte[] value)
        {
            CryptographicBuffer.GenerateRandom(Convert.ToUInt32(value.Length)).ToArray();
        }
    }
}

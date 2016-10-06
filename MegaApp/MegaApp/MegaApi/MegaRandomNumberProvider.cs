using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Security.Cryptography;
using mega;

namespace MegaApp.MegaApi
{
    class MegaRandomNumberProvider : MRandomNumberProvider
    {
        public virtual void GenerateRandomBlock(byte[] value)
        {
            CryptographicBuffer.GenerateRandom(Convert.ToUInt32(value.Length)).ToArray();
        }
    }
}

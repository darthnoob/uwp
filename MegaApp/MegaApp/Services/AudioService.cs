using System;
using System.IO;

namespace MegaApp.Services
{
    static class AudioService
    {
        public static bool IsAudio(string filename)
        {
            try
            {
                string extension = Path.GetExtension(filename);

                if (extension == null) return false;

                switch (extension.ToLower())
                {
                    case ".rm":
                    case ".ra":
                    case ".ram":
                    case ".mp3":
                    case ".wav":
                    case ".3ga":
                    case ".aif":
                    case ".aiff":
                    case ".flac":
                    case ".iff":
                    case ".m4a":
                    case ".wma":
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

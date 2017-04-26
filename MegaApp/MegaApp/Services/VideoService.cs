using System;
using System.IO;

namespace MegaApp.Services
{
    static class VideoService
    {
        public static bool IsVideo(string filename)
        {
            try
            {
                string extension = Path.GetExtension(filename);

                if (extension == null) return false;

                switch (extension.ToLower())
                {
                    case ".mkv":
                    case ".webm":
                    case ".avi":
                    case ".mp4":
                    case ".m4v":
                    case ".mpg":
                    case ".mpeg":
                    case ".mov":
                    case ".3g2":
                    case ".asf":
                    case ".vob":
                    case ".wmv":
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

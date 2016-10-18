using System;
using System.IO;

namespace MegaApp.Services
{
    static class ImageService
    {
        public static bool IsImage(string filename)
        {
            try 
            { 
                string extension = Path.GetExtension(filename);

                if (extension == null) return false;

                switch (extension.ToLower())
                {
                    case ".jpg":
                    case ".jpeg":
                    case ".gif":
                    case ".png":
                    case ".tif":
                    case ".tiff":
                    case ".tga":
                    case ".bmp":
                        {
                            return true;
                        }
                    default:
                        {
                            return false;
                        }
                }
            }
            catch(Exception)
            {
                return false;
            }            
        }

        /// <summary>
        /// Get the vector Path data for a specific filetype extension
        /// </summary>
        /// <param name="filename">filename to extract extension and retrieve vector data</param>
        /// <returns>vector data string</returns>
        public static string GetDefaultFileTypePathData(string filename)
        {
            string fileExtension;

            try
            {
                fileExtension = Path.GetExtension(filename);
            }
            catch (Exception)
            {
                return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");
            }

            if (String.IsNullOrEmpty(fileExtension) || String.IsNullOrWhiteSpace(fileExtension))
                return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");

            switch (fileExtension.ToLower())
            {
                case ".3ds":
                case ".3dm":
                case ".max":
                case ".obj":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_3d");
                    }
                case ".aep":
                case ".aet":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_aftereffects");
                    }
                case ".dxf":
                case ".dwg":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_cad");
                    }
                case ".dwt":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_dreamweaver");
                    }
                case ".accdb":
                case ".sql":
                case ".db":
                case ".dbf":
                case ".mdb":
                case ".pdb":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_data");
                    }
                case ".exe":
                case ".com":
                case ".bin":
                case ".apk":
                case ".app":
                case ".msi":
                case ".cmd":
                case ".gadget":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_executable");
                    }
                case ".as":
                case ".asc":
                case ".ascs":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_fla_lang");
                    }
                case ".fla":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_flash");
                    }
                case ".fnt":
                case ".otf":
                case ".ttf":
                case ".fon":
                    {
                        // TODO FONT PATH??
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");
                    }
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".gif":
                case ".tif":
                case ".tiff":
                case ".tga":
                case ".png":
                case ".ico":
                    {
                        // TODO IMAGE PATH DATA??
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_graphic");
                    }
                case ".gpx":
                case ".kml":
                case ".kmz":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_gis");
                    }
                case ".html":
                case ".htm":
                case ".dhtml":
                case ".xhtml":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_html");
                    }
                case ".ai":
                case ".ait":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_illustrator");
                    }
                case ".indd":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_indesign");
                    }
                case ".jar":
                case ".java":
                case ".class":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_java");
                    }
                case ".midi":
                case ".mid":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_midi");
                    }
                case ".abr":
                case ".psb":
                case ".psd":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_photoshop");
                    }
                case ".pls":
                case ".m3u":
                case ".asx":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_playlist");
                    }
                case ".pcast":
                    {
                        // TODO PODCAST PATH??
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");
                    }
                case ".prproj":
                case ".ppj":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_premiere");
                    }
                case ".3fr":
                case ".arw":
                case ".bay":
                case ".cr2":
                case ".dcr":
                case ".dng":
                case ".fff":
                case ".mef":
                case ".mrw":
                case ".nef":
                case ".pef":
                case ".rw2":
                case ".srf":
                case ".orf":
                case ".rwl":
                    {
                        // TODO RAW PATH
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");
                    }
                case ".rm":
                case ".ra":
                case ".ram":
                    {
                        // TODO REAL AUDIO PATH DATA??
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_audio");
                    }
                case ".sh":
                case ".c":
                case ".cc":
                case ".cpp":
                case ".cxx":
                case ".h":
                case ".hpp":
                case ".dll":
                case ".cs":
                case ".vb":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_sourcecode");
                    }
                case ".torrent":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_torrent");
                    }
                case ".vcf":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_vcard");
                    }
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
                case ".wmv":
                    {
                        // TODO VIDEO PATH DATA??
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_video_vob");
                    }
                case ".srt":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_video_subtitle");
                    }
                case ".vob":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_video_vob");
                    }
                case ".xml":
                case ".shtml":
                case ".js":
                case ".css":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_web_data");
                    }
                case ".php":
                case ".php3":
                case ".php4":
                case ".php5":
                case ".phtml":
                case ".inc":
                case ".asp":
                case ".aspx":
                case ".pl":
                case ".cgi":
                case ".py":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_web_lang");
                    }
                case ".doc":
                case ".docx":
                case ".dotx":
                case ".wps":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_word");
                    }
                case ".eps":
                case ".svg":
                case ".svgz":
                case ".cdr":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_vector");
                    }
                case ".mp3":
                case ".wav":
                case ".3ga":
                case ".aif":
                case ".aiff":
                case ".flac":
                case ".iff":
                case ".m4a":
                case ".wma":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_audio");
                    }
                case ".pdf":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_pdf");
                    }
                case ".ppt":
                case ".pptx":
                case ".pps":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_powerpoint");
                    }
                case ".swf":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_swf");
                    }
                case ".txt":
                case ".rtf":
                case ".ans":
                case ".ascii":
                case ".log":
                case ".odt":
                case ".wpd":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_text");
                    }
                case ".xls":
                case ".xlsx":
                case ".xlt":
                case ".xltm":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_excel");
                    }
                case ".zip":
                case ".rar":
                case ".tgz":
                case ".gz":
                case ".bz2":
                case ".tbz":
                case ".tar":
                case ".7z":
                case ".sitx":
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_compressed");
                    }
                default:
                    {
                        return ResourceService.VisualResources.GetString("VR_FileTypePath_generic");
                    }
            }
        }
    }
}

using System;
using System.Collections.ObjectModel;
using MegaApp.Enums;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Base signature for Node models in the MegaApp
    /// </summary>
    public interface IBaseNode
    {
        #region Properties

        /// <summary>
        /// The display name of the node
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The creation time is the time when the file was uploaded to the MEGA Cloud or
        /// when the file was saved for offline
        /// </summary>
        string CreationTime { get; }

        /// <summary>
        /// The modification time is the modification time of the original file
        /// </summary>
        string ModificationTime { get; }

        /// <summary>
        /// Returns the default location to load or save the thumbnail image for this node
        /// </summary>
        string ThumbnailPath { get; }

        /// <summary>
        /// Unique identifier of the node
        /// </summary>
        string Base64Handle { get; set; }

        /// <summary>
        /// The size of the node in bytes
        /// </summary>
        ulong Size { get; set; }

        /// <summary>
        /// Indicates if the node is currently selected in a multi-select scenario
        /// Needed as path for the RadDatabounndListbox to auto select/deselect
        /// </summary>
        bool IsMultiSelected { get; set; }

        /// <summary>
        /// Returns if a node is a folder.        
        /// </summary>
        bool IsFolder { get; set; }

        /// <summary>
        /// Returns if a node is an image. Based on its file extension.
        /// Not 100% proof because file extensions can be wrong
        /// </summary>
        bool IsImage { get; }

        /// <summary>
        /// The uniform resource identifier of the current thumbnail for this node
        /// Could be a default file/folder type image or a thumbnail preview of the real picture
        /// </summary>
        Uri ThumbnailImageUri { get; set; }

        /// <summary>
        /// Vector data that represents the default image for a specific filetype / folder
        /// </summary>
        string DefaultImagePathData { get; set; }

        /// <summary>
        /// Indicates how the node should be drawn on the screen
        /// </summary>
        NodeDisplayMode DisplayMode { get; set; }

        ObservableCollection<IBaseNode> ParentCollection { get; set; }

        ObservableCollection<IBaseNode> ChildCollection { get; set; }

        #endregion
    }
}

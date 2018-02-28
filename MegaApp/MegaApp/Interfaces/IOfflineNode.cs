using System.Threading.Tasks;
using MegaApp.Enums;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Signature for OfflineNode models in the MegaApp
    /// </summary>
    public interface IOfflineNode : IBaseNode
    {
        #region Properties

        /// <summary>
        /// The display path of the node
        /// </summary>
        string NodePath { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Delete the node from offline
        /// </summary>        
        Task<NodeActionResult> RemoveAsync(bool isMultiRemove);

        /// <summary>
        /// Load node thumbnail if available on disk
        /// </summary>
        void SetThumbnailImage();

        /// <summary>
        /// Open the file that is represented by this node
        /// </summary>
        void Open();

        #endregion
    }
}

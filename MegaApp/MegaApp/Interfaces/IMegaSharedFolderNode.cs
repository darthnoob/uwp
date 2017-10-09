using System.Threading.Tasks;
using mega;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Signature for shared folder node modesl in the MegaApp
    /// </summary>
    public interface IMegaSharedFolderNode : IMegaNode
    {
        #region Methods

        /// <summary>
        /// Stop sharing a folder in MEGA
        /// </summary>
        /// <returns>Result of the action</returns>
        Task<bool> RemoveSharedAccessAsync();

        #endregion

        #region Properties

        /// <summary>
        /// Owner of the incoming shared node
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Acces level to the incoming shared node
        /// </summary>
        MShareType AccessLevel { get; set; }

        #endregion
    }
}

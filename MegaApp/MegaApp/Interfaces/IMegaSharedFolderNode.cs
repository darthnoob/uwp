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
        /// Update core data associated with the SDK MNode object
        /// </summary>
        /// <param name="megaNode">Node to update</param>
        /// <param name="externalUpdate">Indicates if is an update external to the app. For example from an `onNodesUpdate`</param>
        new void Update(MNode megaNode, bool externalUpdate = false);

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

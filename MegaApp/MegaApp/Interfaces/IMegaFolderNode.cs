using System.Threading.Tasks;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Interface for folder node models in the MegaApp
    /// </summary>
    public interface IMegaFolderNode : IMegaNode
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
        /// List of contacts with the folder is shared
        /// </summary>
        ContactsListOutgoingSharedFolderViewModel ContactsList { get; set; }

        #endregion
    }
}

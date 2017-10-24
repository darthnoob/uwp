using System.Threading.Tasks;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Interface for outgoing shared folder node models in the MegaApp
    /// </summary>
    public interface IMegaOutgoingSharedFolderNode : IMegaSharedFolderNode
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
        /// Folder location of the outgoing shared folder
        /// </summary>
        string FolderLocation { get; set; }

        /// <summary>
        /// List of contacts with the folder is shared
        /// </summary>
        ContactsListViewModel ContactsList { get; set; }

        #endregion
    }
}

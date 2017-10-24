using MegaApp.ViewModels.SharedFolders;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Interface for incoming shared folder node models in the MegaApp
    /// </summary>
    public interface IMegaIncomingSharedFolderNode : IMegaSharedFolderNode
    {
        #region Methods

        /// <summary>
        /// Leave the incoming shared folder
        /// </summary>
        void LeaveShare();

        #endregion

        #region Properties

        /// <summary>
        /// Owner of the incoming shared folder
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Access level to the incoming shared folder
        /// </summary>
        SharedFolderAccessLevelViewModel AccessLevel { get; set; }

        #endregion
    }
}

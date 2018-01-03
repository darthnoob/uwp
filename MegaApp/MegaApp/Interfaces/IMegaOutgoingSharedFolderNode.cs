namespace MegaApp.Interfaces
{
    /// <summary>
    /// Interface for outgoing shared folder node models in the MegaApp
    /// </summary>
    public interface IMegaOutgoingSharedFolderNode : IMegaSharedFolderNode
    {
        #region Properties

        /// <summary>
        /// Folder location of the outgoing shared folder
        /// </summary>
        string FolderLocation { get; set; }

        #endregion
    }
}

using MegaApp.ViewModels;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Signature for of contact of an outgoing shared folder models in the MegaApp
    /// </summary>
    public interface IMegaContactOutgoingSharedFolder : IMegaContact
    {
        #region Properties

        /// <summary>
        /// Access level of the contact to the outgoing shared folder
        /// </summary>
        AccessLevelViewModel AccessLevel { get; set; }

        #endregion
    }
}

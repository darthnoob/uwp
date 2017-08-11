using Windows.UI;
using mega;

namespace MegaApp.Interfaces
{
    public interface IMegaContactRequest
    {
        #region Public Methods

        /// <summary>
        /// Accept the contact request
        /// </summary>
        void AcceptContactRequest();

        /// <summary>
        /// Decline the contact request
        /// </summary>
        void DeclineContactRequest();

        /// <summary>
        /// Remind the contact request
        /// </summary>
        void RemindContactRequest();

        /// <summary>
        /// Cancel the contact request
        /// </summary>
        void CancelContactRequest();

        #endregion

        #region Properties

        /// <summary>
        /// Original MContactRequest from the Mega SDK that is the base of the contact request
        /// </summary>
        MContactRequest MegaContactRequest { get; }
        
        /// <summary>
        /// Unique identifier of the contact request
        /// </summary>
        ulong Handle { get; }

        /// <summary>
        /// Status of the contact request
        /// </summary>
        int Status { get; }

        /// <summary>
        /// Returns if the request is an outgoing contact request
        /// </summary>
        bool IsOutgoing { get; }

        /// <summary>
        /// The message that the creator of the contact request has added
        /// </summary>
        string SourceMessage { get; }

        /// <summary>
        /// The email of the request creator
        /// </summary>
        string SourceEmail { get; }

        /// <summary>
        /// The email of the recipient
        /// </summary>
        string TargetEmail { get; }

        /// <summary>
        /// Color for the contact request avatar
        /// </summary>
        Color AvatarColor { get; }

        /// <summary>
        /// The creation time of the contact request
        /// </summary>
        long CreationTime { get; }

        /// <summary>
        /// The last update time of the contact request
        /// </summary>
        long ModificationTime { get; }

        /// <summary>
        /// Indicates if the contact request is currently selected in a multi-select scenario
        /// Needed as path for the ListView to auto select/deselect
        /// </summary>
        bool IsMultiSelected { get; set; }

        #endregion
    }
}

using System;
using mega;
using Windows.UI;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Base signature for MegaContact models in the MegaApp
    /// </summary>
    public interface IMegaContact
    {
        #region Public Methods

        /// <summary>
        /// View the profile of the contact
        /// </summary>
        void ViewProfile();

        /// <summary>
        /// Share a folder with the contact
        /// </summary>
        void ShareFolder();

        /// <summary>
        /// Remove the contact from the contact list
        /// </summary>
        void RemoveContact();

        #endregion

        #region Properties

        /// <summary>
        /// Original MUser from the Mega SDK that is the base of the contact
        /// </summary>
        MUser MegaUser { get; set; }

        /// <summary>
        /// Unique identifier of the contact
        /// </summary>
        ulong Handle { get; set; }

        /// <summary>
        /// Email associated with the contact
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Firstname of the contact
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// Lastname of the contact
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// Full name of the contact
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Timestamp when the contact was added to the contact list (in seconds since the epoch)
        /// </summary>
        ulong Timestamp { get; set; }

        /// <summary>
        /// Visibility of the contact
        /// </summary>
        MUserVisibility Visibility { get; set; }

        /// <summary>
        /// Avatar letter for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        string AvatarLetter { get; }

        /// <summary>
        /// Background color for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        Color AvatarColor { get; set; }

        /// <summary>
        /// The uniform resource identifier of the avatar image of the contact
        /// </summary>
        Uri AvatarUri { get; set; }

        /// <summary>
        /// Path to store the contact avatar image
        /// </summary>
        string AvatarPath { get; }

        /// <summary>
        /// List of folders shared by the contact
        /// </summary>
        MNodeList InSharesList { get; set; }

        #endregion
    }
}

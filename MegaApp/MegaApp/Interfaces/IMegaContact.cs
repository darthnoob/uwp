using System;
using System.Threading.Tasks;
using Windows.UI;
using mega;
using MegaApp.ViewModels.Contacts;

namespace MegaApp.Interfaces
{
    /// <summary>
    /// Base interface for contat models in the MegaApp
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
        /// <param name="isMultiSelect">True if the contact is in a multi-select scenario</param>
        /// <returns>Result of the action</returns>
        Task<bool> RemoveContactAsync(bool isMultiSelect = false);

        #endregion

        #region Properties

        /// <summary>
        /// Original MUser from the Mega SDK that is the base of the contact
        /// </summary>
        MUser MegaUser { get;  }

        /// <summary>
        /// Unique identifier of the contact
        /// </summary>
        ulong Handle { get;  }

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
        ulong Timestamp { get; }

        /// <summary>
        /// Returns the visibility of the contact
        /// </summary>
        MUserVisibility Visibility { get; }

        /// <summary>
        /// Background color for the contact avatar in case of the contact has not an avatar image
        /// </summary>
        Color AvatarColor { get; set; }

        /// <summary>
        /// The uniform resource identifier of the avatar image of the contact
        /// </summary>
        Uri AvatarUri { get; set; }

        /// <summary>
        /// Returns the path to store the contact avatar image
        /// </summary>
        string AvatarPath { get; }

        /// <summary>
        /// Folders shared with or by the contact
        /// </summary>
        ContactSharedItemsViewModel SharedItems { get; set; }

        /// <summary>
        /// Indicates if the contact is currently selected in a multi-select scenario
        /// Needed as path for the ListView to auto select/deselect
        /// </summary>
        bool IsMultiSelected { get; set; }

        #endregion
    }
}

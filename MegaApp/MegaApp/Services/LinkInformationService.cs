using System.Collections.Generic;
using mega;
using MegaApp.Enums;
using MegaApp.Interfaces;

namespace MegaApp.Services
{
    /// <summary>
    /// Service to provide easy access to useful information of links.
    /// </summary>
    public static class LinkInformationService
    {
        #region Properties

        /// <summary>
        /// Link which is being processed or will be processed by the app.
        /// </summary>
        public static string ActiveLink { get; set; }

        /// <summary>
        /// Type of the current link.
        /// </summary>
        public static UriLinkType UriLink { get; set; }

        /// <summary>
        /// Operation to realize with the current active link.
        /// </summary>
        public static LinkAction LinkAction { get; set; }

        /// <summary>
        /// Node obtained from a file link.
        /// </summary>
        public static MNode PublicNode { get; set; }

        /// <summary>
        /// Selected nodes to process from a folder link.
        /// </summary>
        public static List<IBaseNode> SelectedNodes => SelectedNodesService.SelectedNodes;

        /// <summary>
        /// The download path for the selected nodes in case of download operation.
        /// </summary>
        public static string DownloadPath { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Method to reset all the class properties to the default values.
        /// </summary>
        public static void Reset()
        {
            ActiveLink = null;
            UriLink = UriLinkType.None;
            LinkAction = LinkAction.None;
            PublicNode = null;
            DownloadPath = null;
            SelectedNodes.Clear();
        }

        #endregion
    }
}

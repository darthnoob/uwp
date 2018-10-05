using System.Collections.Generic;
using mega;
using MegaApp.Services;

namespace MegaApp.Database
{
    public class SavedForOfflineDB : DatabaseHelper<SavedForOfflineDB>
    {
        #region Properties

        private const string DB_TABLE_NAME = "SavedForOfflineDB";

        private const string FIELD_LOCAL_PATH = "LocalPath";
        private const string FIELD_FINGERPRINT = "Fingerprint";
        private const string FIELD_BASE_64_HANDLE = "Base64Handle";
        private const string FIELD_PARENT_BASE_64_HANDLE = "ParentBase64Handle";

        // The LocalPath property is marked as the Primary Key
        [SQLite.Net.Attributes.PrimaryKey]
        public string LocalPath { get; set; }        
        public string Fingerprint { get; set; }
        public string Base64Handle { get; set; }        
        public string ParentBase64Handle { get; set; }

        #endregion

        #region DatabaseHelper

        /// <summary>
        /// Indicate if exists a node with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistsNodeByLocalPath(string localPath) =>
            ExistsItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);

        /// <summary>
        /// Retrieve the first node found with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the node to search.</param>
        /// <returns>The first node with the specified local path.</returns>
        public static SavedForOfflineDB SelectNodeByLocalPath(string localPath) =>
            SelectItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);

        /// <summary>
        /// Retrieve the list of nodes found with the specified local path in the database.
        /// </summary>
        /// <param name="localPath">Local path of the nodes to search.</param>
        /// <returns>The list of nodes with the specified local path.</returns>
        public static List<SavedForOfflineDB> SelectNodesByLocalPath(string localPath) =>
            SelectItems(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);

        /// <summary>
        /// Indicate if exists a node with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByFingerprint(string fingerprint) =>
            ExistsItem(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);

        /// <summary>
        /// Retrieve the first node found with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the node to search.</param>
        /// <returns>The first node with the specified fingerprint.</returns>
        public static SavedForOfflineDB SelectNodeByFingerprint(string fingerprint) =>
            SelectItem(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);

        /// <summary>
        /// Retrieve the list of nodes found with the specified fingerprint in the database.
        /// </summary>
        /// <param name="fingerprint">Fingerprint of the nodes to search.</param>
        /// <returns>The list of nodes with the specified fingerprint.</returns>
        public static List<SavedForOfflineDB> SelectNodesByFingerprint(string fingerprint) =>
            SelectItems(DB_TABLE_NAME, FIELD_FINGERPRINT, fingerprint);

        /// <summary>
        /// Indicate if exists a node with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByBase64Handle(string base64Handle) =>
            ExistsItem(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);

        /// <summary>
        /// Retrieve the first node found with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the node to search.</param>
        /// <returns>The first node with the specified handle.</returns>
        public static SavedForOfflineDB SelectNodeByBase64Handle(string base64Handle) =>
            SelectItem(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);

        /// <summary>
        /// Retrieve the list of nodes found with the specified handle in the database.
        /// </summary>
        /// <param name="base64Handle">Handle of the nodes to search.</param>
        /// <returns>The list of nodes with the specified handle.</returns>
        public static List<SavedForOfflineDB> SelectNodesByBase64Handle(string base64Handle) =>
            SelectItems(DB_TABLE_NAME, FIELD_BASE_64_HANDLE, base64Handle);

        /// <summary>
        /// Indicate if exists a node with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>TRUE if exists or FALSE in other case.</returns>
        public static bool ExistNodeByParentBase64Handle(string parentBase64Handle) =>
            ExistsItem(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);

        /// <summary>
        /// Retrieve the first node found with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>The first node with the specified parent handle.</returns>
        public static SavedForOfflineDB SelectNodeByParentBase64Handle(string parentBase64Handle) =>
            SelectItem(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);

        /// <summary>
        /// Retrieve the list of nodes found with the specified parent handle in the database.
        /// </summary>
        /// <param name="parentBase64Handle">Parent handle of the node to search.</param>
        /// <returns>The list of nodes with the specified parent handle.</returns>
        public static List<SavedForOfflineDB> SelectNodesByParentBase64Handle(string parentBase64Handle) =>
            SelectItems(DB_TABLE_NAME, FIELD_PARENT_BASE_64_HANDLE, parentBase64Handle);

        /// <summary>
        /// Retrieve all nodes from the database table.
        /// </summary>
        /// <returns>List of all nodes.</returns>
        public static List<SavedForOfflineDB> SelectAllNodes() => SelectAllItems();

        /// <summary>
        /// Update an existing node.
        /// </summary>
        /// <param name="node">Node to update.</param>
        public static void UpdateNode(SavedForOfflineDB node) => UpdateItem(node);

        /// <summary>
        /// Update existing node.
        /// </summary>
        /// <param name="megaNode">Node to update.</param>
        public static void UpdateNode(MNode megaNode)
        {
            var offlineNodePath = OfflineService.GetOfflineNodePath(megaNode);
            var parentNode = SdkService.MegaSdk.getParentNode(megaNode);

            var sfoNode = new SavedForOfflineDB()
            {
                Fingerprint = SdkService.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = offlineNodePath,
                ParentBase64Handle = parentNode != null ?
                    parentNode.getBase64Handle() : string.Empty
            };

            UpdateNode(sfoNode);
        }

        /// <summary>
        /// Insert a node in the database.
        /// </summary>
        /// <param name="node">Node to insert.</param>
        public static void InsertNode(SavedForOfflineDB node) => InsertItem(node);

        /// <summary>
        /// Insert a node in the database.
        /// </summary>
        /// <param name="megaNode">Node to insert.</param>
        public static void InsertNode(MNode megaNode)
        {
            var offlineNodePath = OfflineService.GetOfflineNodePath(megaNode);
            var parentNode = SdkService.MegaSdk.getParentNode(megaNode);

            var sfoNode = new SavedForOfflineDB()
            {
                Fingerprint = SdkService.MegaSdk.getNodeFingerprint(megaNode),
                Base64Handle = megaNode.getBase64Handle(),
                LocalPath = offlineNodePath,
                ParentBase64Handle = parentNode != null ? 
                    parentNode.getBase64Handle() : string.Empty
            };

            InsertItem(sfoNode);            
        }

        /// <summary>
        /// Delete the first node found with the specified local path.
        /// </summary>
        /// <param name="localPath">Local path of the node to delete.</param>
        /// <returns>TRUE if the transaction finished successfully or FALSE in other case.</returns>
        public static bool DeleteNodeByLocalPath(string localPath) =>
            DeleteItem(DB_TABLE_NAME, FIELD_LOCAL_PATH, localPath);

        /// <summary>
        /// Delete specific node.
        /// </summary>
        /// <param name="node">Node to delete.</param>
        public static void DeleteNode(SavedForOfflineDB node) => DeleteItem(node);

        /// <summary>
        /// Delete all node list or delete table.
        /// </summary>
        /// <returns>TRUE if all went well or FALSE in other case.</returns>
        public static bool DeleteAllNodes() => DeleteAllItems();

        #endregion
    }
}

using SQLite.Net;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using mega;
using MegaApp.Services;

namespace MegaApp.Database
{
    public class Database
    {
        #region Properties

        /// <summary>
        /// Path of the database file.
        /// </summary>
        public static readonly string DatabasePath =
            Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, "MEGA.db"));

        /// <summary>
        /// SQLite platform.
        /// </summary>
        public static readonly SQLitePlatformWinRT SQLitePlatform = new SQLitePlatformWinRT();

        #endregion

        #region Methods

        /// <summary>
        /// Check if the database file exists.
        /// </summary>
        /// <returns>TRUE if the database exists or FALSE in other case.</returns>
        public static bool ExistsDatabaseFile() => FileService.FileExists(DatabasePath);

        #endregion
    }

    public class DatabaseHelper<T> where T : class
    {
        #region Methods

        /// <summary>
        /// Create the table in the database if not exist.
        /// </summary>
        public static void CreateTable()
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    db.CreateTable<T>();
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error creating the DB", e);
            }
        }

        /// <summary>
        /// Indicate if an item exists in the database table.
        /// </summary>
        /// <param name="tableName">Name of the database table.</param>
        /// <param name="fieldName">Field by which to search the database.</param>
        /// <param name="fieldValue">Field value to search in the database table.</param>
        /// <returns>TRUE if the item exists or FALSE in other case.</returns>
        public static bool ExistsItem(string tableName, string fieldName, string fieldValue) =>
            SelectItem(tableName, fieldName, fieldValue) != null ? true : false;

        /// <summary>
        /// Retrieve the first item found in the database table.
        /// </summary>
        /// <param name="tableName">Name of the database table.</param>
        /// <param name="fieldName">Field by which to search the database.</param>
        /// <param name="fieldValue">Field value to search in the database table.</param>
        /// <returns>The first item found in the database table.</returns>
        public static T SelectItem(string tableName, string fieldName, string fieldValue)
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath, SQLiteOpenFlags.ReadOnly))
                {
                    return db.Query<T>(
                        "select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'")
                        .FirstOrDefault();
                }
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting item from DB", e);
                return null;
            }
        }

        /// <summary>
        /// Retrieve the list of items found in the database table.
        /// </summary>
        /// <param name="tableName">Name of the database table.</param>
        /// <param name="fieldName">Field by which to search the database.</param>
        /// <param name="fieldValue">Field value to search in the database table.</param>
        /// <returns>List of items found in the database table.</returns>
        public static List<T> SelectItems(string tableName, string fieldName, string fieldValue)
        {
            try 
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath, SQLiteOpenFlags.ReadOnly))
                {
                    return db.Query<T>(
                        "select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'")
                        .ToList();
                }            
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting items from DB", e);
                return null;
            }
        }

        /// <summary>
        /// Retrieve all items from the database table.
        /// </summary>
        /// <returns>List of all items from the database table.</returns>
        public static List<T> SelectAllItems()
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath, SQLiteOpenFlags.ReadOnly))
                {
                    return db.Table<T>().ToList();
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error selecting all items from DB", e);
                return null;
            }
        }

        /// <summary>
        /// Update existing item.
        /// </summary>
        /// <param name="item">Item to update.</param>
        public static void UpdateItem(T item)
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    db.RunInTransaction(() =>
                    {
                        try { db.Update(item); }
                        catch (Exception e){ LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating item of the DB", e); }
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error updating item of the DB", e);
            }
        }

        /// <summary>
        /// Insert a new item in the database.
        /// </summary>
        /// <param name="newItem">Item to insert.</param>
        public static void InsertItem(T newItem)
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    db.RunInTransaction(() =>
                    {
                        try { db.Insert(newItem); }
                        catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error inserting item in the DB", e); }
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error inserting item in the DB", e);
            }
        }

        /// <summary>
        /// Delete the first item found with the specified field value.
        /// </summary>
        /// <param name="tableName">Name of the database table.</param>
        /// <param name="fieldName">Field by which to search the database.</param>
        /// <param name="fieldValue">Field value to search in the database table.</param>
        public static void DeleteItem(string tableName, string fieldName, string fieldValue)
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    var existingItem = db.Query<T>("select * from " + tableName + " where " + fieldName + " = '" + fieldValue + "'").FirstOrDefault();
                    if (existingItem != null)
                    {
                        db.RunInTransaction(() =>
                        {
                            try { db.Delete(existingItem); }
                            catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e); }
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e);
            }
        }

        /// <summary>
        /// Delete specific item.
        /// </summary>
        /// <param name="item">Item to delete.</param>
        public static void DeleteItem(T item)
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    db.RunInTransaction(() =>
                    {
                        try { db.Delete(item); }
                        catch (Exception e) { LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e); }
                    });
                }
            }
            catch (Exception e)
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting item from the DB", e);
            }
        }

        /// <summary>
        /// Delete all items or delete table 
        /// </summary>
        /// <returns>TRUE if all went well or FALSE in other case</returns>
        public static bool DeleteAllItems()
        {
            try
            {
                using (var db = new SQLiteConnection(Database.SQLitePlatform, Database.DatabasePath))
                {
                    db.DropTable<T>();
                    db.CreateTable<T>();
                    db.Dispose();
                    db.Close();

                    return true;
                }
            }
            catch (Exception e) 
            {
                LogService.Log(MLogLevel.LOG_LEVEL_ERROR, "Error deleting DB table", e);
                return false; 
            }
        }

        #endregion
    }
}

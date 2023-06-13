// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Manages a master DialogueDatabase and its data in the global Lua environment.
    /// </summary>
    public class DatabaseManager
    {

        /// <summary>
        /// The default database to use at startup or when resetting the database manager.
        /// </summary>
        public DialogueDatabase defaultDatabase { get; set; }

        /// <summary>
        /// The master database containing the assets of all loaded databases.
        /// </value>
        public DialogueDatabase masterDatabase { get { return GetMasterDatabase(); } }

        /// @cond FOR_V1_COMPATIBILITY
        public DialogueDatabase DefaultDatabase { get { return defaultDatabase; } set { defaultDatabase = value; } }
        public DialogueDatabase MasterDatabase { get { return masterDatabase; } }
        /// @endcond

        private DialogueDatabase m_masterDatabase = null;
        private List<DialogueDatabase> m_loadedDatabases = new List<DialogueDatabase>();

        /// <summary>
        /// The list of databases currently loaded into the masterDatabase.
        /// </summary>
        public List<DialogueDatabase> loadedDatabases { get { return m_loadedDatabases; } }

        /// <summary>
        /// Initializes a new DatabaseManager. Loading of the default database is delayed until the
        /// first time the database is accessed. If you want to manually load the database, you can
        /// reset it or add a database to it.
        /// </summary>
        /// <param name='defaultDatabase'>
        /// (Optional) The default database.
        /// </param>
        public DatabaseManager(DialogueDatabase defaultDatabase = null)
        {
            m_masterDatabase = DatabaseUtility.CreateDialogueDatabaseInstance();
            this.defaultDatabase = defaultDatabase;
        }

        /// <summary>
        /// Gets the master database. If no databases have been loaded, this method loads the 
        /// default database first.
        /// </summary>
        /// <returns>
        /// The master database.
        /// </returns>
        private DialogueDatabase GetMasterDatabase()
        {
            if (m_loadedDatabases.Count == 0) Add(defaultDatabase);
            return m_masterDatabase;
        }

        /// <summary>
        /// Adds a database to the master database, and updates the Lua environment.
        /// </summary>
        /// <param name='database'>
        /// The database to add.
        /// </param>
        public void Add(DialogueDatabase database)
        {
            if ((database != null) && !m_loadedDatabases.Contains(database))
            {
                if (m_loadedDatabases.Count == 0) DialogueLua.InitializeChatMapperVariables();
                m_masterDatabase.Add(database);
                DialogueLua.AddChatMapperVariables(m_masterDatabase, m_loadedDatabases);
                m_loadedDatabases.Add(database);
            }
        }

        /// <summary>
        /// Removes a database from the master database, and updates the Lua environment.
        /// Does not remove any assets that are also defined in other loaded databases.
        /// </summary>
        /// <param name='database'>
        /// The database to remove.
        /// </param>
        public void Remove(DialogueDatabase database)
        {
            if (database != null)
            {
                m_loadedDatabases.Remove(database);
                m_masterDatabase.Remove(database, m_loadedDatabases);
                DialogueLua.RemoveChatMapperVariables(database, m_loadedDatabases);
            }
        }

        /// <summary>
        /// Removes all loaded databases from the master database and clears the Lua environment.
        /// </summary>
        public void Clear()
        {
            DialogueLua.InitializeChatMapperVariables();
            m_masterDatabase.Clear();
            m_loadedDatabases.Clear();
        }

        /// <summary>
        /// Resets the master database using the specified DatabaseResetOptions.
        /// </summary>
        /// <param name='databaseResetOptions'>
        /// Database reset options.
        /// - DatabaseResetOptions.RevertToDefault: Unloads everything and reloads only the default
        /// database.
        /// - DatabaseResetOptions.KeepAllLoaded: Keeps everything loaded but resets their values.
        /// </param>
        public void Reset(DatabaseResetOptions databaseResetOptions = DatabaseResetOptions.RevertToDefault)
        {
            switch (databaseResetOptions)
            {
                case DatabaseResetOptions.RevertToDefault:
                    ResetToDefaultDatabase();
                    break;
                case DatabaseResetOptions.KeepAllLoaded:
                    ResetToLoadedDatabases();
                    break;
            }
        }

        private void ResetToDefaultDatabase()
        {
            Clear();
            Add(defaultDatabase);
        }

        private void ResetToLoadedDatabases()
        {
            var previousLoadedDatabases = new List<DialogueDatabase>(m_loadedDatabases);
            Clear();
            Add(defaultDatabase);
            foreach (DialogueDatabase database in previousLoadedDatabases)
            {
                if (database != defaultDatabase) Add(database);
            }
        }

    }

}

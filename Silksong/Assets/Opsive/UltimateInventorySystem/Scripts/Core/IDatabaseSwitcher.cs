/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    using Opsive.UltimateInventorySystem.Storage;
    using UnityEngine;

    /// <summary>
    /// The database switcher interface is used on objects which have references to inventory database objects.
    /// The interface defines methods which allow you to replace database objects from one database to matching objects from a different database.
    /// </summary>
    public interface IDatabaseSwitcher
    {
        /// <summary>
        /// Check if the object contained by this component are part of the database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>True if all the objects in the component are part of that database.</returns>
        bool IsComponentValidForDatabase(InventorySystemDatabase database);

        /// <summary>
        /// Replace any object that is not in the database by an equivalent object in the specified database.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <returns>The objects that have been changed.</returns>
        ModifiedObjectWithDatabaseObjects ReplaceInventoryObjectsBySelectedDatabaseEquivalents(InventorySystemDatabase database);
    }

    /// <summary>
    /// The return type of the IDatabaseSwitcher, it is used to give information to the objects affected by the database switcher script. 
    /// </summary>
    public class ModifiedObjectWithDatabaseObjects
    {
        public Object[] m_ObjectsToDirty;

        /// <summary>
        /// The constructor takes in an array of objects that should be dirtied in the editor scripts.
        /// </summary>
        /// <param name="objectsToDirty">The objects to dirty.</param>
        public ModifiedObjectWithDatabaseObjects(Object[] objectsToDirty)
        {
            m_ObjectsToDirty = objectsToDirty;
        }

        public static implicit operator ModifiedObjectWithDatabaseObjects(Object[] objectsToDirty)
            => new ModifiedObjectWithDatabaseObjects(objectsToDirty);
    }
}
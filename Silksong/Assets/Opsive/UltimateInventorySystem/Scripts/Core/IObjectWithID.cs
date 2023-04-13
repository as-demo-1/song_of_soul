/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Core
{
    /// <summary>
    /// Interface for an object with an ID, getters only.
    /// </summary>
    public interface IObjectWithIDReadOnly
    {
        /// <summary>
        /// Get the ID.
        /// </summary>
        uint ID { get; }

        /// <summary>
        /// Get the names.
        /// </summary>
        string name { get; }
    }

    /// <summary>
    /// An internal interface for object with an ID.
    /// </summary>
    internal interface IObjectWithID : IObjectWithIDReadOnly
    {
        /// <summary>
        /// Get and Set the ID.
        /// </summary>
        new uint ID { get; set; }

        /// <summary>
        /// Get and Set the name.
        /// </summary>
        new string name { get; set; }
    }

    /// <summary>
    /// An interface to describe an object that can be dirtied.
    /// </summary>
    internal interface IDirtyable
    {
        bool Dirty { get; set; }
    }
}
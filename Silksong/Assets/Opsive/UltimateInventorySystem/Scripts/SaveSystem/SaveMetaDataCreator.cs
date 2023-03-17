/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.SaveSystem
{
    using UnityEngine;

    /// <summary>
    /// A scriptable object used to customize how the meta data is created.
    /// </summary>
    public abstract class SaveMetaDataCreator : ScriptableObject
    {
        public abstract SaveMetaData CreateMetaData(SaveSystemManager saveSystemManager, SaveDataInfo saveDataInfo);

        public abstract SaveMetaData CreateEmpty();
    }
}
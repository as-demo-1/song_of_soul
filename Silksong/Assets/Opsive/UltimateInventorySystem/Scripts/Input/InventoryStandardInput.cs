/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Input
{
    using UnityEngine;
    
    /// <summary>
    /// Deprecated.
    /// </summary>
    public class InventoryStandardInput : InventoryInput
    {
        // Start is called before the first frame update
        void Start()
        {
            Debug.LogWarning("Inventory Standard Input has been deprecated since v1.1.5, please remove it from the player and use the new PlayerInput system. Read more about this and other changes that occured in v1.1.5 in the release notes.",gameObject);

        }
    }
}

/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI
{
    using UnityEngine;

    /// <summary>
    /// Destroys the GameObject when the game starts.
    /// </summary>
    public class DestroyOnStart : MonoBehaviour
    {
        /// <summary>
        /// Destroys the GameObject.
        /// </summary>
        private void Start()
        {
            Destroy(gameObject);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gamekit2D
{
    public class BossDoorStateManager : MonoBehaviour
    {
        [Serializable]
        public class DoorState
        {
            public string keyInventoryName;
            public Sprite sprite;
            public Light light;

            public void UpdateState (InventoryController inventoryController, SpriteRenderer spriteRenderer)
            {
                bool hasKey = inventoryController.HasItem (keyInventoryName);
                if (hasKey)
                    spriteRenderer.sprite = sprite;
                light.enabled = hasKey;
            }
        }


        public InventoryController inventoryController;
        public SpriteRenderer spriteRenderer;
        public DoorState[] doorStates;
    
        protected int m_Keys;
    
        void Start ()
        {
            UpdateStates ();
        }

        public void UpdateStates ()
        {
            for (int i = 0; i < doorStates.Length; i++)
            {
                doorStates[i].UpdateState(inventoryController, spriteRenderer);
            }
        }
    }
}
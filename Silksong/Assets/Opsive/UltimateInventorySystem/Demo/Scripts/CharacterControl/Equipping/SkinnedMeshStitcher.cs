/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.CharacterControl.Equiping
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Attach this script to the character
    /// </summary>
    public class SkinnedMeshStitcher : MonoBehaviour
    {

        [Tooltip("Main skinned mesh such as head or body, etc.")]
        public SkinnedMeshRenderer m_MainSkinnedMeshRenderer;
        [Tooltip("Skinned Mesh Equipment Prefabs, only the skinned mesh object does not require the rig.")]
        public List<GameObject> m_SkinnedMeshEquipmentPrefabs;

        private void Start()
        {
            if (m_SkinnedMeshEquipmentPrefabs == null) { return; }

            for (int i = 0; i < m_SkinnedMeshEquipmentPrefabs.Count; i++) {
                AddEquipmentToRig(m_SkinnedMeshEquipmentPrefabs[i]);
            }
        }

        /// <summary>
        /// Instantiate the equipment prefab and skin it on the same rig as the main skin mesh renderer
        /// </summary>
        /// <param name="equipmentPrefab">The equipment prefab.</param>
        public void AddEquipmentToRig(GameObject equipmentPrefab)
        {
            var equipmentInstance = Instantiate(equipmentPrefab, transform);
            SkinnedMeshRenderer[] equipmentSkinnedMeshRenderers = equipmentInstance.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (var i = 0; i < equipmentSkinnedMeshRenderers.Length; i++) {
                SkinnedMeshRenderer equipmentSkinnedMeshRenderer = equipmentSkinnedMeshRenderers[i];

                equipmentSkinnedMeshRenderer.rootBone = m_MainSkinnedMeshRenderer.rootBone;
                equipmentSkinnedMeshRenderer.bones = m_MainSkinnedMeshRenderer.bones;
            }
        }
    }
}


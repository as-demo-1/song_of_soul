/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Demo.UI
{
    using UnityEngine;

    [ExecuteInEditMode]
    public class FixedScale : MonoBehaviour
    {
        [SerializeField] protected Vector3 m_FixedScale = Vector3.one;

        // Update is called once per frame
        void Update()
        {
            var parent = transform.parent;
            if (parent == null) { return; }

            var parentScale = parent.lossyScale;
            transform.localScale = new Vector3(m_FixedScale.x / parentScale.x,
                m_FixedScale.y / parentScale.y, m_FixedScale.z / parentScale.z);

        }
    }
}
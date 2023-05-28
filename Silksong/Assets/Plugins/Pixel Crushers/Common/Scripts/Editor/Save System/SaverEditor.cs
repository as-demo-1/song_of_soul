// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using UnityEditor;

namespace PixelCrushers
{

    [CustomEditor(typeof(Saver), true)]
    public class SaverEditor : Editor
    {

        protected Saver[] m_saversOnGameObject;

        protected virtual void OnEnable()
        {
            m_saversOnGameObject = (target is Saver) ? (target as Saver).GetComponents<Saver>() : null;
        }

        public override void OnInspectorGUI()
        {
            CheckSaverKeys();
            base.OnInspectorGUI();
        }

        protected virtual void CheckSaverKeys()
        {
            if (m_saversOnGameObject == null) return;
            var numSavers = 0;
            var anyBlankKeys = false;
            for (int i = 0; i < m_saversOnGameObject.Length; i++)
            {
                var saver = m_saversOnGameObject[i];
                if (saver != null)
                {
                    numSavers++;
                    if (!saver.appendSaverTypeToKey && string.IsNullOrEmpty(saver._internalKeyValue))
                    {
                        anyBlankKeys = true;
                    }
                }
            }
            if (numSavers > 1 && anyBlankKeys)
            {
                EditorGUILayout.HelpBox("GameObject has more than one Saver component. Make sure each Saver has a unique Key.", MessageType.Warning);
            }
        }

    }

}

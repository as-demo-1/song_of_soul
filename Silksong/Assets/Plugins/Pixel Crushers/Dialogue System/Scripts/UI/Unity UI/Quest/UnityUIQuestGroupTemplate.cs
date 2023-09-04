// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component hooks up the elements of a Unity UI quest group template.
    /// Add it to your quest group template and assign the properties.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class UnityUIQuestGroupTemplate : MonoBehaviour
    {

        [Header("Quest Group Heading")]
        [Tooltip("The quest group name")]
        public UnityEngine.UI.Text heading;

        public bool ArePropertiesAssigned
        {
            get
            {
                return (heading != null);
            }
        }

        public void Initialize() { }

    }

}

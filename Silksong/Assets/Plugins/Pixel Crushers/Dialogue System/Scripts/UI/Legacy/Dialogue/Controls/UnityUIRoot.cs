using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// Unity UI root, implemented as a container for GUIRoot.
    /// </summary>
    [System.Serializable]
    public class UnityUIRoot : AbstractUIRoot
    {

        private GUIRoot guiRoot;

        public UnityUIRoot(GUIRoot guiRoot)
        {
            this.guiRoot = guiRoot;
        }

        /// <summary>
        /// Shows the GUIRoot.
        /// </summary>
        public override void Show()
        {
            if (guiRoot != null)
            {
                guiRoot.gameObject.SetActive(true);
                guiRoot.ManualRefresh();
            }
        }

        /// <summary>
        /// Hides the GUIRoot.
        /// </summary>
        public override void Hide()
        {
            if (guiRoot != null) guiRoot.gameObject.SetActive(false);
        }

    }

}

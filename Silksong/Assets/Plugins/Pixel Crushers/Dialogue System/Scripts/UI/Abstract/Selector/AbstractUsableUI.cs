// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This abstract class forms the base for StandardUsableUI and UsableUnityUI.
    /// </summary>
    public abstract class AbstractUsableUI : MonoBehaviour
    {

        public abstract void Show(string useMessage);
        public abstract void Hide();
        public abstract void UpdateDisplay(bool inRange);

    }

}

// Copyright (c) Pixel Crushers. All rights reserved.

using PixelCrushers.DialogueSystem.UnityGUI;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This component modifies the behavior of a Selector or ProximitySelector to 
    /// draw the heading and reticle on top of the selection instead an absolute
    /// screen position.
    /// 
    /// If you're using this with Standard UI, size the Main Graphic appropriately so
    /// this script can determine its size.
    /// </summary>
    [AddComponentMenu("")] // Use wrapper.
    public class SelectorFollowTarget : MonoBehaviour
    {

        public Vector3 offset = Vector3.zero;

        private Selector selector = null;
        private ProximitySelector proximitySelector = null;
        private bool previousUseDefaultGUI = false;

        private Usable lastUsable = null;
        private string heading = string.Empty;
        private string useMessage = string.Empty;

        private GameObject lastSelectionDrawn = null;
        private float selectionHeight = 0;
        private Vector2 selectionHeadingSize = Vector2.zero;
        private Vector2 selectionUseMessageSize = Vector2.zero;

        private SelectorUseStandardUIElements selectorUseStandardUIElements = null;

        void Awake()
        {
            selector = GetComponent<Selector>();
            proximitySelector = GetComponent<ProximitySelector>();
            selectorUseStandardUIElements = GetComponent<SelectorUseStandardUIElements>();

        }

        void OnEnable()
        {
            if (selector != null)
            {
                previousUseDefaultGUI = selector.useDefaultGUI;
                selector.useDefaultGUI = false;
            }
            if (proximitySelector != null)
            {
                previousUseDefaultGUI = proximitySelector.useDefaultGUI;
                proximitySelector.useDefaultGUI = false;
            }
        }

        void OnDisable()
        {
            if (selector != null)
            {
                selector.useDefaultGUI = previousUseDefaultGUI;
            }
            if (proximitySelector != null)
            {
                proximitySelector.useDefaultGUI = previousUseDefaultGUI;
            }
        }

        /// <summary>
        /// If using Standard UI, positions the UI elements on top of the usable target.
        /// </summary>
        public virtual void Update()
        {
            if (selectorUseStandardUIElements == null || !selectorUseStandardUIElements.enabled || StandardUISelectorElements.instance == null) return;
            Usable usable = null;
            if (selector != null && selector.enabled)
            {
                usable = selector.CurrentUsable;
            }
            else if (proximitySelector != null && proximitySelector.enabled)
            {
                usable = proximitySelector.CurrentUsable;
            }
            if (usable == null) return;
            var mainGraphic = selectorUseStandardUIElements.elements.mainGraphic;
            var selection = usable.gameObject;
            var screenPos = UnityEngine.Camera.main.WorldToScreenPoint(selection.transform.position + (Vector3.up * selectionHeight));
            screenPos += offset;
            screenPos += new Vector3(-mainGraphic.rectTransform.sizeDelta.x / 2, mainGraphic.rectTransform.sizeDelta.y / 2, 0);
            if (screenPos.z < 0) return;
            mainGraphic.rectTransform.position = screenPos;
        }

        /// <summary>
        /// If using legacy Unity GUI, draws the selection UI on top of the selection target.
        /// </summary>
        public virtual void OnGUI()
        {
            // Use using Standard UI elements, don't draw legacy Unity GUI:
            if (selectorUseStandardUIElements != null && selectorUseStandardUIElements.enabled) return;

            // Otherwise draw it at usable's position:
            if (selector != null && selector.useDefaultGUI)
            {
                DrawOnSelection(selector.CurrentUsable, selector.CurrentDistance, selector.reticle, selector.GuiStyle, selector.defaultUseMessage, selector.inRangeColor, selector.outOfRangeColor, selector.textStyle, selector.textStyleColor);
            }
            else if (proximitySelector != null && proximitySelector.useDefaultGUI)
            {
                DrawOnSelection(proximitySelector.CurrentUsable, 0, null, proximitySelector.GuiStyle, proximitySelector.defaultUseMessage, proximitySelector.color, proximitySelector.color, proximitySelector.textStyle, proximitySelector.textStyleColor);
            }
        }

        protected void DrawOnSelection(Usable usable, float distance, Selector.Reticle reticle, GUIStyle guiStyle, string defaultUseMessage,
                                       Color inRangeColor, Color outOfRangeColor, TextStyle textStyle, Color textStyleColor)
        {
            if (usable == null) return;
            if ((usable != lastUsable) || string.IsNullOrEmpty(heading))
            {
                lastUsable = usable;
                heading = usable.GetName();
                useMessage = string.IsNullOrEmpty(usable.overrideUseMessage) ? defaultUseMessage : usable.overrideUseMessage;
            }
            GameObject selection = usable.gameObject;
            if (selection != lastSelectionDrawn)
            {
                selectionHeight = Tools.GetGameObjectHeight(selection);
                selectionHeadingSize = guiStyle.CalcSize(new GUIContent(heading));
                selectionUseMessageSize = guiStyle.CalcSize(new GUIContent(useMessage));
            }

            // Set text color based on distance:
            bool inUseRange = (distance <= usable.maxUseDistance);
            guiStyle.normal.textColor = inUseRange ? inRangeColor : outOfRangeColor;

            // Draw heading:
            Vector3 screenPos = UnityEngine.Camera.main.WorldToScreenPoint(selection.transform.position + (Vector3.up * selectionHeight));
            screenPos += offset;
            screenPos = new Vector3(screenPos.x, screenPos.y + selectionUseMessageSize.y + selectionHeadingSize.y, screenPos.z);
            if (screenPos.z < 0) return;
            Rect rect = new Rect(screenPos.x - (selectionHeadingSize.x / 2), (Screen.height - screenPos.y) - (selectionHeadingSize.y / 2), selectionHeadingSize.x, selectionHeadingSize.y);
            UnityGUITools.DrawText(rect, heading, guiStyle, textStyle, textStyleColor);

            // Draw use message:
            screenPos = UnityEngine.Camera.main.WorldToScreenPoint(selection.transform.position + (Vector3.up * (selectionHeight)));
            screenPos += offset;
            screenPos = new Vector3(screenPos.x, screenPos.y + selectionUseMessageSize.y, screenPos.z);
            rect = new Rect(screenPos.x - (selectionUseMessageSize.x / 2), (Screen.height - screenPos.y) - (selectionUseMessageSize.y / 2), selectionUseMessageSize.x, selectionUseMessageSize.y);
            UnityGUITools.DrawText(rect, useMessage, guiStyle, textStyle, textStyleColor);

            // Draw reticle:
            if (reticle != null)
            {
                Texture2D reticleTexture = inUseRange ? reticle.inRange : reticle.outOfRange;
                if (reticleTexture != null)
                {
                    screenPos = UnityEngine.Camera.main.WorldToScreenPoint(selection.transform.position + (Vector3.up * 0.5f * selectionHeight));
                    rect = new Rect(screenPos.x - (reticle.width / 2), (Screen.height - screenPos.y) - (reticle.height / 2), reticle.width, reticle.height);
                    GUI.Label(rect, reticleTexture);
                }
            }
        }

    }

}

/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes
{
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// The unity object field with a preview.
    /// </summary>
    public class UnityObjectFieldWithPreview : ObjectField
    {
        protected VisualElement m_Preview;
        private IVisualElementScheduledItem m_ScheduleAction;

        /// <summary>
        /// Create an object field with a preview.
        /// </summary>
        public UnityObjectFieldWithPreview()
        {
            m_Preview = new VisualElement();
            m_Preview.AddToClassList(CommonStyles.s_ObjectPreview);
            m_Preview.AddToClassList(CommonStyles.s_ObjectPreviewSmall);

            //GetAssetPreview is async therefore we need to refresh the view until it gets loaded
            m_ScheduleAction = m_Preview.schedule.Execute(() =>
            {
                m_Preview.RemoveFromClassList(CommonStyles.s_ShrinkZero);
                if (value == null) {
                    m_Preview.AddToClassList(CommonStyles.s_ShrinkZero);
                    m_Preview.style.backgroundImage = null;
                    m_ScheduleAction?.Pause();
                    return;
                }
                var needsRefresh = false;
                var texture = new StyleBackground(AssetPreview.GetAssetPreview(value));
                if (texture == null) {
                    needsRefresh = true;
                    texture = new StyleBackground(AssetPreview.GetMiniThumbnail(value));
                }

                m_Preview.style.backgroundImage = texture;
                if (!needsRefresh) { m_ScheduleAction?.Pause(); }
            });

            Insert(0, m_Preview);

            this.RegisterValueChangedCallback(evt => { Refresh(); });
        }

        /// <summary>
        /// Set the value without notifying external listeners.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public override void SetValueWithoutNotify(Object newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Refresh();
        }

        /// <summary>
        /// Redraw the preview.
        /// </summary>
        public void Refresh()
        {
            m_ScheduleAction.Every(100);
            m_ScheduleAction.Resume();
        }
    }
}
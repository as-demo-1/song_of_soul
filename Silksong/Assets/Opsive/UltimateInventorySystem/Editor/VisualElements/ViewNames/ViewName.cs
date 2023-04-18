/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.VisualElements.ViewNames
{
    using UnityEngine.UIElements;

    /// <summary>
    /// ViewName is used to display a bit more than just a label as a name.
    /// </summary>
    public abstract class ViewName : VisualElement
    {
        protected Label m_Label;
        protected bool m_ShowType = false;

        public Label Label => m_Label;

        public abstract void Refresh();
        public abstract void Refresh(object obj);
        public abstract void SetShowType(bool showType);
    }

    /// <summary>
    /// Generic ViewName.
    /// </summary>
    /// <typeparam name="T">The type of the Object.</typeparam>
    public class ViewName<T> : ViewName where T : class
    {
        protected T m_Object;

        public T Object => m_Object;

        /// <summary>
        /// Constructor to set up the properties.
        /// </summary>
        public ViewName()
        {
            m_Label = new Label();

            Add(m_Label);
        }

        /// <summary>
        /// Refresh after setting a new object.
        /// </summary>
        /// <param name="obj">The new object.</param>
        public virtual void Refresh(T obj)
        {
            m_Object = obj;
            Refresh();
        }

        /// <summary>
        /// Refresh the view.
        /// </summary>
        public override void Refresh()
        {
            SetText(m_Object == null ? "(null)" : m_Object.ToString());
        }

        /// <summary>
        /// Refresh after setting a new object.
        /// </summary>
        /// <param name="obj">The new object.</param>
        public override void Refresh(object obj)
        {
            Refresh(obj as T);
        }

        /// <summary>
        /// Show the Type in the name
        /// </summary>
        /// <param name="showType">Should the type be shown?</param>
        public override void SetShowType(bool showType)
        {
            m_ShowType = showType;
        }

        /// <summary>
        /// Set the text and add the type if showType is true.
        /// </summary>
        /// <param name="text">The new text.</param>
        protected void SetText(string text)
        {
            var suffix = string.Empty;
            if (m_Object != null && m_ShowType) { suffix = $" ({m_Object.GetType().Name})"; }
            m_Label.text = $"{text}{suffix}";
        }
    }
}
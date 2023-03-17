/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Views
{
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    /// <summary>
    /// Box Ui parent.
    /// </summary>
    public class ViewSlot : MonoBehaviour, IViewSlot
    {
        [FormerlySerializedAs("m_BoxObject")]
        [Tooltip("The child of the Box UI parent.")]
        [SerializeField] protected View m_View;
        [Tooltip("The slot index.")]
        [SerializeField] protected int m_Index;

        public View View => m_View;

        public int Index => m_Index;

        /// <summary>
        /// Set the child box.
        /// </summary>
        /// <param name="child">The child box.</param>
        public virtual void SetView(View child)
        {
            m_View = child;
        }

        /// <summary>
        /// Disable the Image.
        /// </summary>
        public void DisableImage()
        {
            var image = GetComponent<Image>();
            if (image != null) { image.enabled = false; }
        }
    }

    public interface IViewSlot
    {
        GameObject gameObject { get; }
        Transform transform { get; }

        View View { get; }
        bool isActiveAndEnabled { get; }
        int Index { get; }

        void SetView(View view);

        void DisableImage();
    }

    public interface IViewModuleWithSlot
    {
        IViewSlot ViewSlot { get; }
        void SetViewSlot(IViewSlot viewSlot);
    }
}
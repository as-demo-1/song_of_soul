using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.UnityGUI
{

    /// <summary>
    /// The basic GUI control, which simply contains child controls.
    /// </summary>
    public class GUIControl : MonoBehaviour
    {

        /// <summary>
        /// The drawing order depth. Higher numbers get drawn after lower numbers.
        /// </summary>
        public int depth = 0;

        /// <summary>
        /// If <c>true</c>, children are drawn in depth order; otherwise no specific order.
        /// </summary>
        public bool depthSortChildren = false;

        /// <summary>
        /// The scaled rect defining the position of the control.
        /// </summary>
        public ScaledRect scaledRect = new ScaledRect(ScaledRect.wholeScreen);

        /// <summary>
        /// Auto-size settings. These specify whether and how to auto-size to fit the contents.
        /// </summary>
        public AutoSize autoSize;

        /// <summary>
        /// Fit settings. These specify how to fit around other controls.
        /// </summary>
        public Fit fit;

        /// <summary>
        /// Keyboard/controller navigation settings.
        /// </summary>
        public Navigation navigation;

        /// <summary>
        /// The pixel rect represented by scaledRect.
        /// </summary>
        /// <value>
        /// The pixel rect.
        /// </value>
        public Rect rect { get; set; }

        /// <summary>
        /// If <c>true</c>, this control and its children are visible.
        /// </summary>
        public bool visible = true;

        /// <summary>
        /// Clip children to the control's bounds?
        /// </summary>
        public bool clipChildren = true;

        /// <summary>
        /// Gets or sets the offset to apply to the screen rect for this control; useful for manual
        /// repositioning outside the normal GUI control system.
        /// </summary>
        /// <value>The offset.</value>
        public Vector2 Offset { get; set; }

        /// <summary>
        /// The child controls.
        /// </summary>
        protected List<GUIControl> Children
        {
            get { return children; }
        }

        /// <summary>
        /// When <c>true</c>, the control needs to update its style, size, position, etc.
        /// Use the Refresh() method to set this to <c>true</c>.
        /// </summary>
        public bool NeedToUpdateLayout
        {
            get { return needToUpdateLayout; }
            set { needToUpdateLayout = value; }
        }

        /// <summary>
        /// The size of the window most recently passed to Refresh(). Used to update
        /// the control's layout.
        /// </summary>
        protected Vector2 WindowSize
        {
            get { return windowSize; }
            set { windowSize = value; }
        }

        /// <summary>
        /// Gets a value indicating whether keyboard/controller navigation is enabled.
        /// </summary>
        public bool IsNavigationEnabled
        {
            get { return (navigation != null) && navigation.enabled; }
        }

        /// <summary>
        /// Gets the full name of the GameObject, used to focus the control when using
        /// keyboard/controller navigation.
        /// </summary>
        /// <value>The full name.</value>
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(fullName)) fullName = Tools.GetFullName(this.gameObject);
                return fullName;
            }
        }

        /// <summary>
        /// Gets or sets dRect, which offsets the rect when the parent window isn't clipping.
        /// </summary>
        public Vector2 dRect { get; set; }

        /// <summary>
        /// The cached full name of the GameObject.
        /// </summary>
        private string fullName = null;
        private List<GUIControl> children = new List<GUIControl>();
        private bool needToUpdateLayout = true;
        private Vector2 windowSize = Vector2.zero;
        private bool navigationSelectButtonClicked = false;

        public virtual void Awake()
        {
            dRect = Vector2.zero;
            rect = scaledRect.GetPixelRect();
            Offset = Vector2.zero;
        }

        /// <summary>
        /// Checks if the control needs to enable the first child for key/controller navigation.
        /// </summary>
        public virtual void OnEnable()
        {
            Refresh();
            if (IsNavigationEnabled && navigation.focusFirstControlOnEnable) navigation.FocusFirstControl();
        }

        /// <summary>
        /// Draw the control and its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public void Draw(Vector2 relativeMousePosition)
        {
            if (visible && gameObject.activeSelf)
            {
                UpdateLayout();
                DrawSelf(relativeMousePosition);
                DrawChildren(relativeMousePosition);
            }
        }

        /// <summary>
        /// Draws the control, but not its children.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position within the window containing this control.</param>
        public virtual void DrawSelf(Vector2 relativeMousePosition)
        {
        }

        /// <summary>
        /// Draws the children, taking into account key/controller navigation if enabled.
        /// </summary>
        /// <param name="relativeMousePosition">Relative mouse position.</param>
        public virtual void DrawChildren(Vector2 relativeMousePosition)
        {
            if (Children.Count == 0) return;
            if (clipChildren) GUI.BeginGroup(rect);
            try
            {
                bool isNavigationEnabled = IsNavigationEnabled;
                if (isNavigationEnabled && (navigation.click != KeyCode.Space))
                {
                    if ((Event.current.type == EventType.KeyDown) && (Event.current.character == ' ')) return;
                }
                GUIControl clickedControl = null;
                bool navigationClicked = IsNavigationEnabled && (navigation.IsClicked || navigationSelectButtonClicked);
                Vector2 childMousePosition = new Vector2(relativeMousePosition.x - rect.x, relativeMousePosition.y - rect.y);
                if (isNavigationEnabled) navigation.CheckNavigationInput(childMousePosition);
                foreach (var child in Children)
                {
                    if (IsNavigationEnabled)
                    {
                        GUI.SetNextControlName(child.FullName);
                        if (navigationClicked && string.Equals(GUI.GetNameOfFocusedControl(), child.FullName))
                        {
                            navigationSelectButtonClicked = false;
                            clickedControl = child;
                        }
                    }
                    child.Draw(childMousePosition);
                }
                if (isNavigationEnabled) GUI.FocusControl(navigation.FocusedControlName);
                if ((clickedControl != null) && (clickedControl is GUIButton)) (clickedControl as GUIButton).Click();
            }
            finally
            {
                if (clipChildren) GUI.EndGroup();
            }
        }

        /// <summary>
        /// If navigation is enabled, check if the selection button was pressed.
        /// Remember the value so we can check it in DrawChildren.
        /// </summary>
        public virtual void Update()
        {
            if (IsNavigationEnabled)
            {
                navigationSelectButtonClicked = DialogueManager.getInputButtonDown(navigation.clickButton);
            }
        }

        /// <summary>
        /// Marks a control as needing to update its layout.
        /// </summary>
        /// <param name="windowSize">Window size.</param>
        public virtual void Refresh(Vector2 windowSize)
        {
            NeedToUpdateLayout = true;
            WindowSize = windowSize;
        }

        public virtual void Refresh()
        {
            NeedToUpdateLayout = true;
        }

        /// <summary>
        /// Updates the layout (size, position, formatting, etc.) of the control and its children.
        /// </summary>
        public virtual void UpdateLayout()
        {
            if (NeedToUpdateLayout)
            {
                UpdateLayoutSelf();
                FitSelf();
                UpdateLayoutChildren();
                FitChildren();
                //NeedToUpdateLayout = false;
            }
        }

        /// <summary>
        /// Updates the control's layout but not its children.
        /// </summary>
        public virtual void UpdateLayoutSelf()
        {
            NeedToUpdateLayout = false;
            if (WindowSize.x == 0) WindowSize = new Vector2(Screen.width, Screen.height);
            rect = scaledRect.GetPixelRect(WindowSize);
            if ((Offset.x != 0) || (Offset.y != 0)) rect = new Rect(rect.x + Offset.x, rect.y + Offset.y, rect.width, rect.height);
            if ((dRect.x != 0) || (dRect.y != 0)) rect = new Rect(rect.x + dRect.x, rect.y + dRect.y, rect.width, rect.height);
            if (autoSize != null) AutoSizeSelf();
        }

        /// <summary>
        /// Auto-sizes the control according to the autoSize settings.
        /// </summary>
        public virtual void AutoSizeSelf()
        {
        }

        /// <summary>
        /// Fits the control according to the fit settings.
        /// </summary>
        protected virtual void FitSelf()
        {
            if ((fit != null) && (fit.IsSpecified))
            {
                float xMin = rect.xMin;
                float xMax = rect.xMax;
                float yMin = rect.yMin;
                float yMax = rect.yMax;
                if (fit.above != null)
                {
                    yMax = fit.above.rect.yMin;
                    if ((fit.below == null) && !fit.expandToFit)
                    {
                        yMin = yMax - rect.height;
                    }
                }
                if (fit.below != null)
                {
                    yMin = fit.below.rect.yMax;
                    if ((fit.above == null) && !fit.expandToFit)
                    {
                        yMax = yMin + rect.height;
                    }
                }
                if (fit.leftOf != null)
                {
                    xMax = fit.leftOf.rect.xMin;
                    if ((fit.rightOf == null) && !fit.expandToFit)
                    {
                        xMin = xMax - rect.width;
                    }
                }
                if (fit.rightOf != null)
                {
                    xMin = fit.rightOf.rect.xMax;
                    if ((fit.rightOf == null) && !fit.expandToFit)
                    {
                        xMax = xMin + rect.width;
                    }
                }
                rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
            }
        }

        /// <summary>
        /// Refreshes the list of children and updates their layouts.
        /// </summary>
        private void UpdateLayoutChildren()
        {
            FindChildren();
            if (depthSortChildren) SortChildren();
            Vector2 childWindowSize = new Vector2(rect.width, rect.height);
            foreach (var child in Children)
            {
                UpdateLayoutChild(child, childWindowSize);
            }
        }

        private void UpdateLayoutChild(GUIControl child, Vector2 childWindowSize)
        {
            child.Refresh(childWindowSize);
            child.dRect = clipChildren ? Vector2.zero : new Vector2(rect.x, rect.y);
            child.UpdateLayout();
            //---Was (replaced by dRect above): if (!clipChildren) child.rect = new Rect(child.rect.x + rect.x, child.rect.y + rect.y, child.rect.width, child.rect.height);
        }

        private void FitChildren()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].FitSelf();
            }
        }

        /// <summary>
        /// Updates the children list with all child controls found on child objects.
        /// </summary>
        private void FindChildren()
        {
            Children.Clear();
            foreach (Transform t in transform)
            {
                GUIControl[] components = t.GetComponents<GUIControl>();
                Children.AddRange(components);
                if (components.Length > 0)
                {
                    components[0].FindChildren();
                }
            }
        }

        private void SortChildren()
        {
            Children.Sort((x, y) => x.depth.CompareTo(y.depth));
        }

    }

}

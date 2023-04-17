/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.UI.Grid
{
    using Opsive.UltimateInventorySystem.Utility;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// The UI layout navigation forces selectables in a grid to only move between each other. 
    /// </summary>
    public class LayoutGroupNavigation : MonoBehaviour
    {
        [Tooltip("Disable the validation process.")]
        [SerializeField] protected bool m_Disabled = false;
        [Tooltip("Should the grid selection wrap.")]
        [SerializeField] protected bool m_GridWrap;

        private GridLayoutGroup m_GridLayoutGroup;
        private VerticalLayoutGroup m_VerticalLayout;
        private HorizontalLayoutGroup m_HorizontalLayout;

        public GridLayoutGroup GridLayoutGroup =>
            m_GridLayoutGroup != null ? m_GridLayoutGroup : GetComponent<GridLayoutGroup>();
        public VerticalLayoutGroup VerticalLayoutGroup =>
            m_VerticalLayout != null ? m_VerticalLayout : GetComponent<VerticalLayoutGroup>();
        public HorizontalLayoutGroup HorizontalLayoutGroup =>
            m_HorizontalLayout != null ? m_HorizontalLayout : GetComponent<HorizontalLayoutGroup>();

        /// <summary>
        /// Validate by setting the selectables movement references.
        /// </summary>
        private void OnValidate()
        {
            if (OnValidateUtility.IsPrefab(this)) { return; }
            RefreshNavigation();
        }

        /// <summary>
        /// Refresh the navigation by setting the selectables movement references.
        /// </summary>
        [ContextMenu("Refresh Navigation")]
        public void RefreshNavigation()
        {
            if (m_Disabled) { return; }

            if (m_GridLayoutGroup == null
                && m_VerticalLayout == null
                && m_HorizontalLayout == null) {
                m_GridLayoutGroup = GetComponent<GridLayoutGroup>();
                m_VerticalLayout = GetComponent<VerticalLayoutGroup>();
                m_HorizontalLayout = GetComponent<HorizontalLayoutGroup>();
            }

            if (m_GridLayoutGroup != null) {
                SetGridLayoutGroupNavigation(m_GridLayoutGroup);
                return;
            }

            if (m_VerticalLayout != null) {
                SetOneDimensionalNavigation(false);
                return;
            }

            if (m_HorizontalLayout != null) {
                SetOneDimensionalNavigation(true);
                return;
            }

            //No Layout Group, Connect children selectables.
            SetChildrenAutoNavigation();
        }

        /// <summary>
        /// Try to get the selectable at the index provided.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="selectable">The selectable output.</param>
        /// <returns>True if it exists.</returns>
        private bool TryGetSelectableChild(int index, out Selectable selectable)
        {
            if (index < 0 || index >= transform.childCount) {
                selectable = null;
                return false;
            }

            var child = transform.GetChild(index);
            if (child.gameObject.activeSelf == false) {
                selectable = null;
                return false;
            }

            selectable = transform.GetChild(index).GetComponent<Selectable>();
            return selectable != null;
        }

        /// <summary>
        /// Set a single dimensional navigation Horizontal or vertical.
        /// </summary>
        /// <param name="horizontal">Horizontal or Vertical.</param>
        private void SetOneDimensionalNavigation(bool horizontal)
        {
            Selectable previous = null;
            for (int i = 0; i < transform.childCount; i++) {
                if (TryGetSelectableChild(i, out var selectable)) {
                    if (horizontal) {
                        previous = HorizontalNavigation(selectable, previous);
                    } else {
                        previous = VerticalNavigation(selectable, previous);
                    }

                }
            }
        }

        /// <summary>
        /// Vertical Navigation.
        /// </summary>
        /// <param name="selectable">The selectable.</param>
        /// <param name="previous">The previous selectable.</param>
        /// <returns>The new previous selectable.</returns>
        private static Selectable VerticalNavigation(Selectable selectable, Selectable previous)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = previous;
            navigation.selectOnDown = null;
            navigation.selectOnLeft = null;
            navigation.selectOnRight = null;

            selectable.navigation = navigation;

            if (previous != null) {
                var previousNavigation = previous.navigation;
                previousNavigation.selectOnDown = selectable;
                previous.navigation = previousNavigation;
            }

            previous = selectable;
            return previous;
        }

        /// <summary>
        /// Horizontal Navigation.
        /// </summary>
        /// <param name="selectable">The selectable.</param>
        /// <param name="previous">The previous selectable.</param>
        /// <returns>The new previous selectable.</returns>
        private static Selectable HorizontalNavigation(Selectable selectable, Selectable previous)
        {
            var navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = null;
            navigation.selectOnDown = null;
            navigation.selectOnLeft = previous;
            navigation.selectOnRight = null;

            selectable.navigation = navigation;

            if (previous != null) {
                var previousNavigation = previous.navigation;
                previousNavigation.selectOnRight = selectable;
                previous.navigation = previousNavigation;
            }

            previous = selectable;
            return previous;
        }

        /// <summary>
        /// Set the grid layout group navigation.
        /// </summary>
        /// <param name="gridLayoutGroup">The grid layout group.</param>
        public void SetGridLayoutGroupNavigation(GridLayoutGroup gridLayoutGroup)
        {
            Vector2Int gridSize = Vector2Int.zero;

            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.Flexible) {
                return;
            }

            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount) {
                gridSize = new Vector2Int(gridLayoutGroup.constraintCount,
                    transform.childCount / gridLayoutGroup.constraintCount);
            }
            if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount) {
                gridSize = new Vector2Int(transform.childCount / gridLayoutGroup.constraintCount,
                    gridLayoutGroup.constraintCount);
            }

            for (int i = 0; i < gridSize.y; i++) {
                for (int j = 0; j < gridSize.x; j++) {

                    var nextChildIndex = -1;
                    var previousXIndex = -1;
                    var previousYIndex = -1;
                    bool noWrapX = false;
                    bool noWrapY = false;

                    if (gridLayoutGroup.startAxis == GridLayoutGroup.Axis.Horizontal) {
                        nextChildIndex = i * gridSize.x + j;
                        previousXIndex = i * gridSize.x + (j - 1);
                        previousYIndex = (i - 1) * gridSize.x + j;
                        noWrapX = !m_GridWrap && j == 0;
                        noWrapY = false;
                    } else {
                        nextChildIndex = j * gridSize.y + i;
                        previousXIndex = (j - 1) * gridSize.y + i;
                        previousYIndex = j * gridSize.y + (i - 1);
                        noWrapX = false;
                        noWrapY = !m_GridWrap && i == 0;
                    }


                    if (TryGetSelectableChild(nextChildIndex, out var selectable)) {

                        TryGetSelectableChild(previousXIndex, out var previousX);
                        if (noWrapX) { previousX = null; }

                        TryGetSelectableChild(previousYIndex, out var previousY);
                        if (noWrapY) { previousY = null; }

                        var navigation = selectable.navigation;
                        navigation.mode = Navigation.Mode.Explicit;
                        navigation.selectOnUp = previousY;
                        navigation.selectOnDown = null;
                        navigation.selectOnLeft = previousX;
                        navigation.selectOnRight = null;

                        selectable.navigation = navigation;

                        if (previousX != null) {
                            var previousNavigation = previousX.navigation;
                            previousNavigation.selectOnRight = selectable;
                            previousX.navigation = previousNavigation;
                        }

                        if (previousY != null) {
                            var previousNavigation = previousY.navigation;
                            previousNavigation.selectOnDown = selectable;
                            previousY.navigation = previousNavigation;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the children to navigate between each other, not outside.
        /// </summary>
        private void SetChildrenAutoNavigation()
        {
            var selectables = new List<Selectable>();

            for (int i = 0; i < transform.childCount; i++) {
                if (TryGetSelectableChild(i, out var selectable)) {
                    selectables.Add(selectable);
                }
            }

            UINavigationUtility.SetExplicitAutoNavigation(selectables);
        }
    }

    /// <summary>
    /// The UI Navigation Utility copied from the Unity source code.
    /// </summary>
    public static class UINavigationUtility
    {
        /// <summary>
        /// Set the selectables.
        /// </summary>
        /// <param name="selectables">The selectables.</param>
        public static void SetExplicitAutoNavigation(List<Selectable> selectables)
        {
            for (int i = 0; i < selectables.Count; i++) {
                var selectable = selectables[i];

                var navigation = selectable.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                navigation.selectOnUp = FindSelectable(selectable, Vector3.up, selectables);
                navigation.selectOnDown = FindSelectable(selectable, Vector3.down, selectables);
                navigation.selectOnLeft = FindSelectable(selectable, Vector3.left, selectables);
                navigation.selectOnRight = FindSelectable(selectable, Vector3.right, selectables);

                selectable.navigation = navigation;
            }
        }

        /// <summary>
        /// Find the selectable.
        /// </summary>
        /// <param name="selectable">The selectable.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="selectables">List of selectables.</param>
        /// <returns>The best selectable.</returns>
        public static Selectable FindSelectable(Selectable selectable, Vector3 dir, List<Selectable> selectables)
        {
            dir = dir.normalized;
            var transform = selectable.transform;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            Selectable bestPick = null;

            for (int i = 0; i < selectables.Count; ++i) {
                Selectable sel = selectables[i];

                if (sel == selectable)
                    continue;

                if (!sel.IsInteractable() || sel.navigation.mode == Navigation.Mode.None)
                    continue;

#if UNITY_EDITOR
                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(sel.gameObject, Camera.current))
                    continue;
#endif

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? (Vector3)selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                float score = dot / myVector.sqrMagnitude;

                if (score > maxScore) {
                    maxScore = score;
                    bestPick = sel;
                }
            }
            return bestPick;
        }

        /// <summary>
        /// Get the point On Rect Edge.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="dir">The direction.</param>
        /// <returns>The point.</returns>
        private static Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
        {
            if (rect == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
            return dir;
        }
    }
}

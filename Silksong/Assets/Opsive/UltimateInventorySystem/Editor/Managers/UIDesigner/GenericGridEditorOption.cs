/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Editor.VisualElements.ControlTypes;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Grid;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;
    using Image = UnityEngine.UI.Image;
    using Toggle = UnityEngine.UIElements.Toggle;

    public abstract class GenericGridEditorOption : UIDesignerBoxBase
    {
        protected GridBase m_GridBase;
        protected RectTransform m_DisplayPanelMainContent;

        public void Refresh(GridBase grid)
        {
            m_GridBase = grid;

            if (m_GridBase == null) {
                m_DisplayPanelMainContent = null;
                return;
            }

            m_DisplayPanelMainContent = m_GridBase?.ParentPanel?.MainContent;
            if (m_DisplayPanelMainContent == null) {
                m_DisplayPanelMainContent = m_GridBase.gameObject.GetComponentInParent<DisplayPanel>(true)?.MainContent;

                if (m_DisplayPanelMainContent == null) {
                    m_DisplayPanelMainContent = m_GridBase.transform.parent as RectTransform;
                }
            }

            Refresh();
        }

        public abstract void Refresh();
    }

    public class LayoutGroupOption : GenericGridEditorOption
    {
        public override string Title => "Grid Size & Layout Group";

        public override string Description =>
            "The layout group is optional, it is used to size up and space out the Grid elements (View Slots / Action Buttons) evenly.\n" +
            "Choose between list or grid layouts (recommended grid layout even when using a list)";

        public override Func<Component> SelectTargetGetter => () => m_LayoutGroup;

        private enum GridLayoutEnum
        {
            Grid,
            List,
            None
        }

        protected GridSizeField m_GridSizeField;

        protected Transform m_Content;
        protected LayoutGroup m_LayoutGroup;
        protected LayoutGroupNavigation m_LayoutGroupNavigation;
        protected EnumField m_LayoutGroupField;
        protected CreateSelectDeleteContainer m_CreateSelectDeleteNavigationContainer;

        public LayoutGroupOption()
        {
            Add(new SubTitleLabel("Grid Size"));

            m_GridSizeField = new GridSizeField();
            m_GridSizeField.OnValueChanged += ResizeGrid;
            Add(m_GridSizeField);

            Add(new SubTitleLabel("Layout Group"));

            m_LayoutGroupField = new EnumField("Grid Layout", GridLayoutEnum.Grid);
            m_LayoutGroupField.RegisterValueChangedCallback(evt =>
            {
                RemoveComponent(m_LayoutGroup);
                var option = (GridLayoutEnum)evt.newValue;
                switch (option) {
                    case GridLayoutEnum.Grid:
                        m_Content.gameObject.AddComponent<GridLayoutGroup>();
                        break;
                    case GridLayoutEnum.List:
                        m_Content.gameObject.AddComponent<VerticalLayoutGroup>();
                        break;
                }
            });
            Add(m_LayoutGroupField);

            Add(new SubTitleLabel("Layout Group Navigation"));

            m_CreateSelectDeleteNavigationContainer = new CreateSelectDeleteContainer(
                "Layout Group Navigation",
                AddRemoveNavigation,
                AddRemoveNavigation,
                () => m_LayoutGroupNavigation);
            Add(m_CreateSelectDeleteNavigationContainer);
        }

        private void ResizeGrid(Vector2Int newSize)
        {
            m_GridBase.GridSize = newSize;
            m_GridBase.OnValidate();
        }

        private void AddRemoveNavigation()
        {
            if (m_Content == null) {
                Debug.LogWarning("The Content is missing, cannot find layout group");
                return;
            }

            if (m_LayoutGroupNavigation == null) {
                m_Content.gameObject.AddComponent<LayoutGroupNavigation>();
                return;
            }

            RemoveComponent(m_LayoutGroupNavigation);
            Refresh();
        }

        public override void Refresh()
        {

            m_GridSizeField.SetValueNoNotify(m_GridBase?.GridSize ?? Vector2Int.one);

            m_Content = m_GridBase?.GridEventSystem?.Content;

            if (m_Content == null) {
                Debug.LogWarning("The Grid Content is missing, cannot find layout group");
                return;
            }

            m_LayoutGroup = m_Content.GetComponent<LayoutGroup>();

            if (m_LayoutGroup == null) {
                m_LayoutGroupField.SetValueWithoutNotify(GridLayoutEnum.None);
            } else if (m_LayoutGroup is GridLayoutGroup) {
                m_LayoutGroupField.SetValueWithoutNotify(GridLayoutEnum.Grid);
            } else {
                m_LayoutGroupField.SetValueWithoutNotify(GridLayoutEnum.List);
            }

            m_LayoutGroupNavigation = m_Content.GetComponent<LayoutGroupNavigation>();

            m_CreateSelectDeleteNavigationContainer.Refresh();
        }
    }

    public class GridSizeField : VisualElement
    {
        public event Action<Vector2Int> OnValueChanged;
        protected Vector2Int m_SizeLimits = new Vector2Int(1, 100);

        private Vector2Int m_GridSize = Vector2Int.one;

        protected AddRemoveField m_AddRemoveColumn;
        protected AddRemoveField m_AddRemoveRow;

        public GridSizeField()
        {
            m_AddRemoveRow = new AddRemoveField("Columnss : 1");
            m_AddRemoveRow.OnAddRemove += (add) =>
            {
                var newX = Mathf.Clamp(m_GridSize.x + (add ? 1 : -1), m_SizeLimits.x, m_SizeLimits.y);

                SetValue(new Vector2Int(
                    newX,
                    m_GridSize.y));
            };

            Add(m_AddRemoveRow);

            m_AddRemoveColumn = new AddRemoveField("Rows : 1");
            m_AddRemoveColumn.OnAddRemove += (add) =>
            {
                var newY = Mathf.Clamp(m_GridSize.y + (add ? 1 : -1), m_SizeLimits.x, m_SizeLimits.y);

                SetValue(new Vector2Int(
                    m_GridSize.x,
                    newY));
            };

            Add(m_AddRemoveColumn);
        }

        public void SetValue(Vector2Int gridSize)
        {
            SetValueNoNotify(gridSize);

            OnValueChanged?.Invoke(m_GridSize);
        }

        public void SetValueNoNotify(Vector2Int gridSize)
        {
            m_GridSize = gridSize;
            m_AddRemoveRow.Label.text = "Columns : " + gridSize.x;
            m_AddRemoveColumn.Label.text = "Rows : " + gridSize.y;
        }
    }

    public class GridNavigationOptions : GenericGridEditorOption
    {
        public override string Title => "Grid Navigation";
        public override string Description => "The Grid Navigation is used to scroll through an inventory, using buttons, scrollers, etc.\n" +
                                              "Tabs are sometimes linked with the navigation to swap tab after reaching the limit of the current tab.";
        public override Func<Component> SelectTargetGetter => () => m_Navigator;

        protected GridNavigatorBase m_Navigator;

        protected EnumField m_NavigationOption;
        protected Toggle m_Vertical;
        protected Toggle m_LinkNavigationAndTabs;
        protected Button m_BuildButton;

        public GridNavigationOptions() : base()
        {

            m_NavigationOption = new EnumField("Navigation Option", NavigationOption.None);
            Add(m_NavigationOption);

            m_Vertical = new Toggle("Vertical");
            Add(m_Vertical);


            m_LinkNavigationAndTabs = new Toggle("Link Navigation and Tabs.");
            Add(m_LinkNavigationAndTabs);

            m_BuildButton = CreateButton("Build", this, BuildAction);
        }

        protected bool BuildCondition()
        {
            if (m_GridBase == null) {
                Debug.LogError("The grid is missing");
                return false;
            }

            return true;
        }

        public override void Refresh()
        {
            if (m_GridBase == null) { return; }

            m_Navigator = m_GridBase.GetComponent<GridNavigatorBase>();

            if (m_Navigator == null) {
                m_NavigationOption.value = NavigationOption.None;
                m_Vertical.value = true;
                m_LinkNavigationAndTabs.value = false;
                m_BuildButton.text = "Build";
                return;
            }

            if (m_Navigator.m_TabControl != null) { m_LinkNavigationAndTabs.value = true; }

            if (m_Navigator is GridNavigator gridNavigator) {
                m_NavigationOption.value = NavigationOption.Buttons;
            }

            if (m_Navigator is GridNavigatorWithScrollbar gridNavigatorWithScrollbar) {
                m_NavigationOption.value = NavigationOption.ScrollStep;
            }

            if (m_Navigator is GridNavigatorWithScrollView gridNavigatorWithScrollView) {
                m_NavigationOption.value = NavigationOption.ScrollView;
            }

            var layoutGroup = m_Navigator.GetComponent<GridEventSystem>()?.Content?.GetComponent<LayoutGroup>();

            if (layoutGroup is HorizontalLayoutGroup) {
                m_Vertical.value = false;
            } else if (layoutGroup is VerticalLayoutGroup) {
                m_Vertical.value = true;
            } else if (layoutGroup is GridLayoutGroup gridLayoutGroup) {
                var axis = gridLayoutGroup.startAxis;
                m_Vertical.value = axis == GridLayoutGroup.Axis.Horizontal;
            }

            m_BuildButton.text = "Replace";
        }

        private void BuildAction()
        {
            if (BuildCondition() == false) { return; }

            m_Navigator = m_GridBase.GetComponent<GridNavigatorBase>();

            var replace = m_Navigator != null;

            if (replace) {

                if (m_Navigator is GridNavigator gridNavigator) {
                    DestroyGameObject(gridNavigator.m_NextButton);
                    DestroyGameObject(gridNavigator.m_PreviousButton);
                }

                if (m_Navigator is GridNavigatorWithScrollbar gridNavigatorWithScrollbar) {
                    DestroyGameObject(gridNavigatorWithScrollbar.m_Scrollbar);
                }

                if (m_Navigator is GridNavigatorWithScrollView gridNavigatorWithScrollView) {
                    DestroyGameObject(gridNavigatorWithScrollView.m_Scrollbar);
                    RemoveComponent(gridNavigatorWithScrollView.m_Content.transform.parent.GetComponent<Mask>());
                }

                RemoveComponent(m_Navigator);
            }

            if (m_Navigator != null) {
                if (m_LinkNavigationAndTabs.value) {
                    m_Navigator.m_TabControl = m_GridBase.GetComponent<TabControl>();
                } else {
                    m_Navigator.m_TabControl = null;
                }
            }

            switch ((NavigationOption)m_NavigationOption.value) {
                case NavigationOption.None:
                    return;
                case NavigationOption.Buttons:
                    BuildNavigationButton();
                    break;
                case NavigationOption.ScrollStep:
                    BuildNavigationScrollStep();
                    break;
                case NavigationOption.ScrollView:
                    BuildNavigationScrollView();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            UIDesignerManager.Refresh();
        }

        private void UpdateGridLayoutAxis(GridNavigatorBase gridNavigator)
        {
            var gridEventSystem = gridNavigator.GetComponent<GridEventSystem>();
            if (gridEventSystem == null) {
                Debug.LogWarning("The Grid Event System component is missing from the same gameobject as the Grid Navigator and Inventory Grid");
                return;
            }

            if (gridEventSystem.GridLayoutGroup == null) {
                Debug.LogWarning("The Grid Layout Group could not be found, Please make sur you are using a Grid Layout Group and not a Vertical or Horizontal Layout Group.");
                return;
            }
            
            gridEventSystem.GridLayoutGroup.startAxis =
                m_Vertical.value ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
        }

        private ScrollbarWithButtons CreateScrollbar()
        {
            var scrollbar = UIDesignerManager.InstantiateSchemaPrefab<ScrollbarWithButtons>(UIDesignerSchema.ScrollbarVertical, m_DisplayPanelMainContent);

            if (m_Vertical.value == false) {
                var rectTransform = scrollbar.transform as RectTransform;
                RectTransformUtility.MoveCenterRightToCenterDown(rectTransform, false);
            }

            return scrollbar;
        }

        private void BuildNavigationScrollStep()
        {
            var gridNavigator = m_GridBase.gameObject.AddComponent<GridNavigatorWithScrollbar>();
            UpdateGridLayoutAxis(gridNavigator);

            var scrollbar = CreateScrollbar();

            gridNavigator.m_Scrollbar = scrollbar;
            gridNavigator.m_Vertical = m_Vertical.value;
            gridNavigator.m_Grid = m_GridBase;
            gridNavigator.m_NextButton = scrollbar.PositiveButton;
            gridNavigator.m_PreviousButton = scrollbar.NegativeButton;
        }

        private void BuildNavigationScrollView()
        {
            var gridNavigator = m_GridBase.gameObject.AddComponent<GridNavigatorWithScrollView>();
            UpdateGridLayoutAxis(gridNavigator);

            var scrollbar = CreateScrollbar();

            var gridLayoutGroup = m_GridBase.GridLayoutGroup;
            var gridContent = gridLayoutGroup.transform as RectTransform;

            if (m_Vertical.value) {
                gridContent.anchorMax = new Vector2(0.5f, 1);
                gridContent.anchorMin = new Vector2(0.5f, 1);
                gridContent.pivot = new Vector2(0.5f, 1);
                gridContent.anchoredPosition = new Vector2();
            } else {
                gridContent.anchorMax = new Vector2(0, 0.5f);
                gridContent.anchorMin = new Vector2(0, 0.5f);
                gridContent.pivot = new Vector2(0, 0.5f);
                gridContent.anchoredPosition = new Vector2();
            }

            var viewPort = gridContent.parent as RectTransform;

            var viewPortImage = viewPort.GetComponent<Image>();
            if (viewPortImage == null) {
                viewPortImage = viewPort.gameObject.AddComponent<Image>();
            }

            var mask = viewPort.GetComponent<Mask>();
            if (mask == null) {
                mask = viewPort.gameObject.AddComponent<Mask>();
                mask.showMaskGraphic = false;
            }

            var xSize = gridLayoutGroup.cellSize.x + gridLayoutGroup.spacing.x;
            var ySize = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
            var gridSize = m_GridBase.GridSize;

            if (m_Vertical.value) {
                viewPort.sizeDelta = new Vector2(
                    gridSize.x * xSize,
                    (gridSize.y - 1) * ySize
                );
            } else {
                viewPort.sizeDelta = new Vector2(
                    (gridSize.x - 1) * xSize,
                    gridSize.y * ySize
                );
            }

            gridNavigator.m_Content = gridContent;
            gridNavigator.m_Scrollbar = scrollbar;
            gridNavigator.m_Vertical = m_Vertical.value;
            gridNavigator.m_Grid = m_GridBase;
            gridNavigator.m_NextButton = scrollbar.PositiveButton;
            gridNavigator.m_PreviousButton = scrollbar.NegativeButton;
        }

        private void BuildNavigationButton()
        {
            var gridNavigator = m_GridBase.gameObject.AddComponent<GridNavigator>();
            UpdateGridLayoutAxis(gridNavigator);

            var buttonNext = UIDesignerManager.InstantiateSchemaPrefab<UnityEngine.UI.Button>(UIDesignerSchema.ButtonRight, m_DisplayPanelMainContent);
            var buttonPrevious = UIDesignerManager.InstantiateSchemaPrefab<UnityEngine.UI.Button>(UIDesignerSchema.ButtonRight, m_DisplayPanelMainContent);

            buttonNext.name = "Button Next";
            buttonPrevious.name = "Button Previous";

            gridNavigator.m_Grid = m_GridBase;

            if (m_Vertical.value) {
                RectTransformUtility.MoveCenterRightToCenterDown(buttonNext.transform as RectTransform);
                RectTransformUtility.MoveCenterRightToCenterUp(buttonPrevious.transform as RectTransform);
            } else {
                RectTransformUtility.MoveCenterRightToCenterLeft(buttonPrevious.transform as RectTransform);
            }


            gridNavigator.m_NextButton = buttonNext;
            gridNavigator.m_PreviousButton = buttonPrevious;
        }
    }
}
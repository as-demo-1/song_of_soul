/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.CompoundElements;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Events;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UIElements;

    public class MainMenuDesigner : UIDesignerCreateEditTabContent<
        MainMenu,
        MainMenuBuilderCreator,
        MainMenuBuilderEditor>
    {
        public override string Title => "Main Menu";
        public override string Description => "The main menu is an optional panel which allows you to gather up multiple sub panels in one place.\n" +
                                              "You may populate it using the Inventory Grid, Shop Menu, Crafting Menu, etc.";

        public void AddInnerPanel(string title, DisplayPanel displayPanel)
        {
            var mainMenu = displayPanel.gameObject.GetComponentInParent<MainMenu>(true);
            var button = UIDesignerManager.InstantiateSchemaPrefab<ActionButton>(UIDesignerSchema.MainMenuTabButton, mainMenu.MenuTabs);
            button.SetButtonName(title);
            button.transform.SetSiblingIndex(0);
            mainMenu.Panels.Add(new PanelButton() { Panel = displayPanel, Button = button });
            Shared.Editor.Utility.EditorUtility.SetDirty(mainMenu);
        }
    }

    public class MainMenuBuilderCreator : UIDesignerCreator<MainMenu>
    {
        public override string DocumentationURL => "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/main-menu/";

        protected UnicodeTextField m_NameTextField;
        protected Toggle m_Vertical;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_NameTextField = new UnicodeTextField("Panel name");
            m_NameTextField.value = "Main Menu";
            container.Add(m_NameTextField);

            m_Vertical = new Toggle("Vertical");
            m_Vertical.value = true;
            container.Add(m_Vertical);
        }

        protected override MainMenu BuildInternal()
        {
            var parentRect = m_ParentTransform.value as RectTransform;
            var panelManager = parentRect.gameObject.GetComponentInParent<DisplayPanelManager>(true);

            var prefab = m_Vertical.value ? UIDesignerSchema.MainMenuVertical : UIDesignerSchema.MainMenuHorizontal;

            var mainMenu = UIDesignerManager.InstantiateSchemaPrefab<MainMenu>(prefab, parentRect);

            mainMenu.gameObject.name = m_NameTextField.value;
            mainMenu.GetComponent<DisplayPanel>().m_UniqueName = m_NameTextField.value;

            panelManager.MainMenu = mainMenu.GetComponent<DisplayPanel>();

            var gameplayPanel = panelManager.GameplayPanel;

            if (gameplayPanel != null) {
                var button = UIDesignerManager.InstantiateSchemaPrefab<ActionButton>(UIDesignerSchema.MainMenuTabButton, gameplayPanel.MainContent);

                UnityAction callback = new UnityAction(mainMenu.DisplayPanel.SmartOpen);
                UnityEventTools.AddPersistentListener(button.m_OnClickEvent, callback);

                button.SetButtonName("Main Menu");
            }

            return mainMenu;
        }
    }

    public class MainMenuBuilderEditor : UIDesignerEditor<MainMenu>
    {
        public override string Description => "Edit the Main Menu by adding and removing subMenus.\n" +
                                              "The submenus within the Main Menu can be added from the other tabs by selecting 'Main Menu' as panel option. " +
                                              "You may add 'empty' (blank) sub panels which you can manually fill in for non inventory related UI or any custom sub panel.";

        private List<PanelButton> m_List;
        private ReorderableList m_ReorderableList;

        protected int m_SelectedIndex;

        protected VisualElement m_SelectionContainer;
        protected VisualElement m_ButtonsContainer;
        protected ComponentSelectionButton m_SelectPanel;
        protected ComponentSelectionButton m_SelectButton;

        public MainMenuBuilderEditor()
        {
            m_List = new List<PanelButton>();
            m_ReorderableList = new ReorderableList(
                m_List,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;
                    listElement.text = m_List[index].Panel?.name ?? "NULL";
                }, (parent) =>
                {
                    parent.Add(new Label("Main Menu Sub Panels"));
                },
                HandleSelection,
                AddNewSubPanel,
                RemoveSubPanel,
                (i1, i2) =>
                {
                    RefreshSubPanelList();
                });

            m_SelectionContainer = new VisualElement();
            Add(m_SelectionContainer);

            m_ButtonsContainer = new VisualElement();
            m_ButtonsContainer.AddToClassList(CommonStyles.s_HorizontalAlignCenter);

            m_SelectPanel = new ComponentSelectionButton("Select Panel", () => m_List[m_SelectedIndex].Panel);
            m_SelectButton = new ComponentSelectionButton("Select Button", () => m_List[m_SelectedIndex].Button);
        }

        private void AddNewSubPanel()
        {

            var displayPanel = UIDesignerManager.InstantiateSchemaPrefab<DisplayPanel>(UIDesignerSchema.MainMenuInnerPanel, DisplayPanel.MainContent);

            var panelName = "BlankSubPanel";
            displayPanel.SetName(panelName);
            displayPanel.gameObject.name = panelName;

            UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel("BlankSubPanel", displayPanel);

            Refresh();
        }

        private void RemoveSubPanel(int index)
        {
            if (index < 0 || index >= m_List.Count) { return; }

            var panelName = m_List[m_SelectedIndex].Panel?.name ?? "Null";

            var removeSubPanel = EditorUtility.DisplayDialog("Remove Sub Panel?",
                $"Removing the sub Panel '{panelName}' will completely remove the button, the panel and the reference to it in the Main Menu component.\n" +
                $"Are you sure you wish to remove this subPanel",
                "Yes",
                "No");

            if (removeSubPanel == false) { return; }

            DestroyGameObject(m_List[m_SelectedIndex].Button);
            DestroyGameObject(m_List[m_SelectedIndex].Panel);

            m_List.RemoveAt(index);

            RefreshSubPanelList();
        }

        private void RefreshSubPanelList()
        {
            m_Target.m_Panels = new List<PanelButton>(m_List);
            Shared.Editor.Utility.EditorUtility.SetDirty(m_Target);
            Refresh();
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_TargetOptionsContainer.Add(m_ReorderableList);

            m_List.Clear();
            m_List.AddRange(m_Target.m_Panels);
            m_ReorderableList.Refresh(m_List);

            HandleSelection(m_SelectedIndex);
        }

        private void HandleSelection(int index)
        {
            m_SelectionContainer.Clear();

            m_SelectedIndex = index;

            if (index < 0 || index >= m_List.Count) {
                return;
            }

            var button = m_List[m_SelectedIndex].Button;
            var panel = m_List[m_SelectedIndex].Panel;

            m_SelectionContainer.Add(new SubTitleLabel(panel?.name ?? "NULL"));
            m_SelectionContainer.Add(new Label("Panel name: " + (panel?.UniqueName ?? "NULL")));
            m_SelectionContainer.Add(new Label("Button text: " + (button?.GetButtonName() ?? "NULL")));
            m_SelectionContainer.Add(m_ButtonsContainer);

            m_ButtonsContainer.Add(m_SelectPanel);
            m_ButtonsContainer.Add(m_SelectButton);
        }
    }
}
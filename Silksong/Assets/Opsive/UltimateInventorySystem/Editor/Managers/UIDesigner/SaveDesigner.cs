/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.UltimateInventorySystem.SaveSystem;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Panels.Save;
    using System;
    using System.Collections.Generic;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    public enum SaveMenuGridDisplayPanelOption
    {
        Basic,
        MainMenu,
    }

    public class SaveDesigner : UIDesignerCreateEditTabContent<
        SaveMenu,
        SaveDesignerCreator,
        SaveDesignerEditor>
    {
        public override string Title => "Save";
        public override string Description => "Create a save menu.";
    }

    public class SaveDesignerCreator : UIDesignerCreator<SaveMenu>
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/save/";
        protected EnumField m_PanelOption;

        protected override void CreateOptionsContent(VisualElement container)
        {
            m_PanelOption = new EnumField("Panel Option", CraftingMenuGridDisplayPanelOption.Basic);
            m_PanelOption.RegisterValueChangedCallback(evt => { Refresh(); });
            container.Add(m_PanelOption);
        }

        public override bool BuildCondition(bool logWarnings)
        {
            var result = base.BuildCondition(logWarnings);
            if (result == false) { return false; }

            if (GameObject.FindObjectsOfType<SaveSystemManager>() == null) {
                m_ConditionHelpBox.SetMessage("You must set up the Saver Manager before creating the Saver UI. Use the Main Setup Manager to do so.");
                return false;
            }

            var panelOption = (InventoryGridDisplayPanelOption)m_PanelOption.value;
            if (panelOption == InventoryGridDisplayPanelOption.MainMenu) {
                var rectParent = m_ParentTransform.value as RectTransform;
                var mainMenu = rectParent.gameObject.GetComponentInParent<MainMenu>(true);
                if (mainMenu == null || mainMenu.DisplayPanel.MainContent != rectParent) {
                    m_ConditionHelpBox.SetMessage("The parent transform must be the main menu main content when making a main menu inner panel.");
                    return false;
                }
            }

            return true;
        }

        protected override SaveMenu BuildInternal()
        {
            var rectParent = m_ParentTransform.value as RectTransform;

            var panelOption = (ShopDisplayPanelOption)m_PanelOption.value;
            var craftingMenu = UIDesignerManager.InstantiateSchemaPrefab<SaveMenu>(UIDesignerSchema.SaveMenu, rectParent);

            if (panelOption == ShopDisplayPanelOption.MainMenu) {
                UIDesignerManager.GetTab<MainMenuDesigner>().AddInnerPanel("Save/Load", craftingMenu.DisplayPanel);
            }

            return craftingMenu;
        }
    }

    public class SaveDesignerEditor : UIDesignerEditor<SaveMenu>
    {
        protected SaveSystemOptions m_SaveSystemOptions;
        protected LayoutGroupOption m_LayoutGroupOption;
        protected GridNavigationOptions m_GridNavigationOptions;

        public SaveDesignerEditor()
        {
            m_SaveSystemOptions = new SaveSystemOptions(this);

            m_LayoutGroupOption = new LayoutGroupOption();

            m_GridNavigationOptions = new GridNavigationOptions();
        }

        protected override void NewValidTargetAssigned()
        {
            base.NewValidTargetAssigned();

            m_SaveSystemOptions.Refresh(m_Target);
            m_TargetOptionsContainer.Add(m_SaveSystemOptions);

            m_TargetOptionsContainer.Add(m_LayoutGroupOption);
            m_LayoutGroupOption.Refresh(m_Target?.SaveGrid);

            m_TargetOptionsContainer.Add(m_GridNavigationOptions);
            m_GridNavigationOptions.Refresh(m_Target?.SaveGrid);

        }
    }

    public abstract class SaveMenuEditorOption : UIDesignerBoxBase
    {
        protected SaveDesignerEditor m_SaveMenuEditor;

        protected SaveMenu m_SaveMenu;
        protected VisualElement m_Container;

        protected SaveMenuEditorOption(SaveDesignerEditor menuEditor)
        {
            m_SaveMenuEditor = menuEditor;
            m_Container = new VisualElement();
            Add(m_Container);
        }

        public virtual void Refresh(SaveMenu menu)
        {
            m_SaveMenu = menu;
            m_Container.Clear();
        }
    }

    public class SaveSystemOptions : SaveMenuEditorOption
    {
        public override string Title => "Save System Manager and Savers";

        public override string Description =>
            "The Saver components will tell the Save System Manager that they exist. " +
            "The Save System Manager will organize the serialized the data from the Savers such that it can be written to disk.\n";

        public override Func<Component> SelectTargetGetter => () => m_SaveSystemManager;

        private SaveSystemManager m_SaveSystemManager;

        List<SaverBase> m_Savers;
        private ReorderableList m_ReorderableList;

        public SaveSystemOptions(SaveDesignerEditor menuEditor) : base(menuEditor)
        {

            m_Savers = new List<SaverBase>();
            m_ReorderableList = new ReorderableList(
                m_Savers,
                (parent, index) =>
                {
                    var listElement = new Label("New");
                    parent.Add(listElement);
                }, (parent, index) =>
                {
                    var listElement = parent.ElementAt(0) as Label;

                    if (index >= m_Savers.Count) {
                        Debug.LogWarning("Index " + index + " does not exist.");
                        listElement.text = "NULL";
                        return;
                    }

                    listElement.text = m_Savers[index].ToString();
                }, (parent) =>
                {
                    parent.Add(new Label("Saver components in the scene"));
                },
                (index) =>
                {
                    ComponentSelection.Select(m_Savers[index]);
                }, null, null, null);
        }

        public override void Refresh(SaveMenu menu)
        {
            base.Refresh(menu);
            m_SaveSystemManager = GameObject.FindObjectOfType<SaveSystemManager>();

            m_Savers.Clear();
            m_Savers.AddRange(UIDesignerUtility.FindAllObjectsOfType<SaverBase>());
            m_ReorderableList.Refresh(m_Savers);

            m_Container.Add(m_ReorderableList);
        }
    }
}
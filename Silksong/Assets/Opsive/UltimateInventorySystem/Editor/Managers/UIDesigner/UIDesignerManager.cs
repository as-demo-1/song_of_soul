/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public enum NavigationOption
    {
        Buttons,
        ScrollStep,
        ScrollView,
        //Page,
        None
    }

    /// <summary>
    /// The UI Designer allows users to create their game UI in a few clicks, perfect for creating a basic layout.
    /// </summary>
    [OrderedEditorItem("UI Designer", 80)]
    [RequireDatabase]
    public class UIDesignerManager : InventoryManager
    {
        public const string UIDesignerSchemaClassicGUID = "11cd918b7dca149428d825071dbd2b87";
        public const string UIDesignerSchemaRPGGUID = "6e555dbd8631923458f1c491cdeded1d";

        // The UIBuilderConfig path is based on the project.
        private static string UIBuilderConfigGUIDKey => "Opsive.UltimateInventorySystem.UIBuilderConfigGUID." + Application.productName;
        private static string UIBuilderConfigGUID { get => EditorPrefs.GetString(UIBuilderConfigGUIDKey, string.Empty); }

        private static string UIBuilderTabIndexKey => "Opsive.UltimateInventorySystem.UIBuilderTabIndex." + Application.productName;
        private static int UIBuilderTabIndex { get => EditorPrefs.GetInt(UIBuilderTabIndexKey, 0); }

        protected TabToolbar m_Toolbar;
        protected VisualElement m_Container;

        protected List<UIDesignerTabContentBase> m_Builders;
        protected SetupDesigner m_SetupDesigner;
        protected MainMenuDesigner m_MainMenuDesigner;
        protected InventoryGridDesigner m_InventoryGridDesigner;
        protected ItemShapeGridDesigner m_ItemShapeGridDesigner;
        protected EquipmentDesigner m_EquipmentDesigner;
        protected HotbarDesigner m_HotbarDesigner;
        protected ShopDesigner m_ShopDesigner;
        protected CraftingDesigner m_CraftingDesigner;
        protected SaveDesigner m_SaveDesigner;
        protected StorageDesigner m_StorageDesigner;
        protected ChestDesigner m_ChestDesigner;
        protected ItemDescriptionDesigner m_ItemDescriptionDesigner;
        protected CurrencyDesigner m_CurrencyDesigner;
        protected InventoryMonitorDesigner m_InventoryMonitorDesigner;
        protected ItemViewDesigner m_ItemViewDesigner;
        protected AttributeViewDesigner m_AttributeViewDesigner;

        protected (bool isValid, string message) m_UIDesignerValidationResult = (false, "Not Yet Checked");
        public (bool isValid, string message) UIDesignerValidationResult => m_UIDesignerValidationResult;

        private static UIDesignerManager s_Instance;
        public static UIDesignerManager Instance => s_Instance;

        private static UIDesignerSchema s_UIDesignerSchema;

        public static UIDesignerSchema UIDesignerSchema {
            get {
                if (s_UIDesignerSchema != null) { return s_UIDesignerSchema; }

                s_UIDesignerSchema = Shared.Editor.Utility.EditorUtility.LoadAsset<UIDesignerSchema>(UIBuilderConfigGUID);
                return s_UIDesignerSchema;
            }
            set {
                s_UIDesignerSchema = value;
                if (s_UIDesignerSchema != null) {
                    EditorPrefs.SetString(UIBuilderConfigGUIDKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(s_UIDesignerSchema)));
                } else {
                    EditorPrefs.SetString(UIBuilderConfigGUIDKey, "");
                }
            }
        }

        /// <summary>
        /// Initialize the Manager by creating all the tabs.
        /// </summary>
        public override void BuildVisualElements()
        {
            if (m_InventoryMainWindow.Database == null) { return; }

            InventoryMainWindow.OnFocusEvent += OnFocus;

            m_ManagerContentContainer.Clear();
            s_Instance = this;

            m_SetupDesigner = new SetupDesigner();
            m_MainMenuDesigner = new MainMenuDesigner();
            m_InventoryGridDesigner = new InventoryGridDesigner();
            m_ItemShapeGridDesigner = new ItemShapeGridDesigner();
            m_EquipmentDesigner = new EquipmentDesigner();
            m_HotbarDesigner = new HotbarDesigner();
            m_ShopDesigner = new ShopDesigner();
            m_CraftingDesigner = new CraftingDesigner();
            m_SaveDesigner = new SaveDesigner();
            m_StorageDesigner = new StorageDesigner();
            m_ChestDesigner = new ChestDesigner();
            m_ItemDescriptionDesigner = new ItemDescriptionDesigner();
            m_CurrencyDesigner = new CurrencyDesigner();
            m_InventoryMonitorDesigner = new InventoryMonitorDesigner();
            m_ItemViewDesigner = new ItemViewDesigner();
            m_AttributeViewDesigner = new AttributeViewDesigner();

            m_Builders = new List<UIDesignerTabContentBase>()
            {
                m_SetupDesigner,
                m_MainMenuDesigner,
                m_InventoryGridDesigner,
                m_ItemShapeGridDesigner,
                m_EquipmentDesigner,
                m_HotbarDesigner,
                m_ShopDesigner,
                m_CraftingDesigner,
                m_SaveDesigner,
                m_StorageDesigner,
                m_ChestDesigner,
                m_ItemDescriptionDesigner,
                m_CurrencyDesigner,
                m_InventoryMonitorDesigner,
                m_ItemViewDesigner,
                m_AttributeViewDesigner,
            };

            var titleList = new string[m_Builders.Count];
            for (int i = 0; i < titleList.Length; i++) { titleList[i] = m_Builders[i].Title; }

            m_Toolbar = new TabToolbar(titleList, UIBuilderTabIndex, ChangeTab);
            m_Toolbar.style.flexWrap = Wrap.Wrap;
            m_ManagerContentContainer.Add(m_Toolbar);

            m_Container = new ScrollView(ScrollViewMode.Vertical);
            m_ManagerContentContainer.Add(m_Container);

            ChangeTab(m_Toolbar.Selected);
        }

        /// <summary>
        /// Refresh the Editor when it is focused.
        /// </summary>
        private void OnFocus()
        {
            if (m_MainManagerWindow.IsOpen(this) == false) { return; }

            Refresh();
        }

        /// <summary>
        /// Refresh the manager tab that is loaded.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            //In case it refreshes while the code is compiling stop before null exception.
            if (m_Builders == null) { return; }

            var result = IsSetupValid();
            m_UIDesignerValidationResult = result;

            for (int i = 1; i < m_Builders.Count; i++) {
                m_Toolbar.EnableButton(i, result.isValid);
            }

            if (result.isValid == false) {
                m_Toolbar.Selected = 0;
                m_Builders[0].Refresh();
                ChangeTab(0);
                return;
            }

            for (int i = 0; i < m_Builders.Count; i++) {
                m_Builders[i].Refresh();
            }
        }

        /// <summary>
        /// Check if the UI Designer manager can be used.
        /// </summary>
        /// <returns>True if the UI Designer can be used.</returns>
        private (bool isValid, string message) IsSetupValid()
        {
            if (Application.isPlaying) {
                return (false, "You may not use the UI Designer at runtime!");
            }

            if (m_InventoryMainWindow.Database == null) {
                return (false, "A database must be selected before UIDesigner can be used.");
            }

            if (GameObject.FindObjectOfType<InventorySystemManager>() == null) {
                return (false, "An Inventory System Manager must be present in the scene, use the Editor Setup Manager to create one.");
            }

            if (GameObject.FindObjectOfType<DisplayPanelManager>() == null) {
                return (false, "At least one Display Panel Manager must be present in the scene, please use the Create Canvas Manager 'Setup' Button above.");
            }

            if (UIDesignerSchema == null) {
                return (false, "A UI Designer schema must be created and assigned to use the UI Designer Editor Manager.");
            }

            if (IsSchemaFromOpsive()) {
                return (false, "It is required to duplicate one of the available schemas before continuing, this ensures you keep the prefabs in your project folders.");
            }

            return UIDesignerSchema.CheckIfValid();
        }

        /// <summary>
        /// Check if the UI Designer Schema was created by Opsive and prevent users from using it directly.
        /// </summary>
        /// <returns>True if the schema was created by opsive.</returns>
        public bool IsSchemaFromOpsive()
        {
#pragma warning disable 0162

#if UIS_DEV
            return false;
#endif

            if (UIDesignerSchema == null) { return false; }

            var schemaGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(UIDesignerSchema));
            if (schemaGUID == UIDesignerSchemaClassicGUID
                || schemaGUID == UIDesignerSchemaRPGGUID) {
                return true;
            }

#pragma warning restore 0162
            return false;
        }

        /// <summary>
        /// Get the UI Designer tab with a specific type.
        /// </summary>
        /// <typeparam name="T">The UI Designer Tab Content Type.</typeparam>
        /// <returns>The Tab.</returns>
        public T GetTab<T>() where T : UIDesignerTabContentBase
        {
            for (int i = 0; i < m_Builders.Count; i++) {
                if (m_Builders[i] is T builderT) { return builderT; }
            }

            return null;
        }

        /// <summary>
        /// Change the selected tab.
        /// </summary>
        /// <param name="tab">The tab to select.</param>
        public void ChangeTab(UIDesignerTabContentBase tab)
        {
            for (int i = 0; i < m_Builders.Count; i++) {
                if (tab != m_Builders[i]) { continue; }

                ChangeTab(i);
                return;
            }
        }

        /// <summary>
        /// Change the selected tab.
        /// </summary>
        /// <param name="index">The index of the tab to select.</param>
        public void ChangeTab(int index)
        {
            m_Container.Clear();

            if (index < 0 || index >= m_Builders.Count) { return; }

            EditorPrefs.SetInt(UIBuilderTabIndexKey, index);
            m_Container.Add(m_Builders[index]);

            m_Builders[index].Refresh();

            if (m_Toolbar.Selected != index) {
                m_Toolbar.Selected = index;
            }
        }

        /// <summary>
        /// Insitatiate a prefab from one of the schemas.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent of that object.</param>
        /// <typeparam name="T">The type of that object.</typeparam>
        /// <returns>The instantiated object component or gameobject.</returns>
        public T InstantiateSchemaPrefab<T>(GameObject prefab, RectTransform parent)
        {
            var gameObject = UIDesignerSchema.KeepPrefabLink ?
                PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject :
                GameObject.Instantiate(prefab, parent);

            gameObject.name = prefab.name;

            var component = gameObject.GetComponentInChildren<T>();
            return component;
        }

        /// <summary>
        /// Insitatiate a prefab from one of the schemas.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate.</param>
        /// <param name="parent">The parent of that object.</param>
        /// <typeparam name="T">The type of that object.</typeparam>
        /// <returns>The instantiated object component or gameobject.</returns>
        public T InstantiateSchemaPrefab<T>(T prefabComponent, RectTransform parent) where T : Component
        {
            var componentInstance = UIDesignerSchema.KeepPrefabLink ?
                PrefabUtility.InstantiatePrefab(prefabComponent, parent) as T :
                GameObject.Instantiate(prefabComponent, parent);

            componentInstance.name = prefabComponent.name;

            var component = componentInstance.GetComponentInChildren<T>();
            return component;
        }

        /// <summary>
        /// UIDesignerManager destructor.
        /// </summary>
        ~UIDesignerManager()
        {
            InventoryMainWindow.OnFocusEvent -= OnFocus;
        }
    }

    /// <summary>
    /// A base class for Content Boxes to keep the manager organized.
    /// </summary>
    public abstract class UIDesignerBoxBase : VisualElement
    {
        protected VisualElement m_IconOptions;

        public UIDesignerSchema UIDesignerSchema => UIDesignerManager.UIDesignerSchema;
        public UIDesignerManager UIDesignerManager => UIDesignerManager.Instance;

        public virtual string DocumentationURL => null;
        public virtual Func<Component> SelectTargetGetter => null;
        public abstract string Title { get; }
        public abstract string Description { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        public UIDesignerBoxBase()
        {
            AddToClassList(InventoryManagerStyles.SubMenu);
            style.marginTop = 20f;

            var topHorizontalLayout = new VisualElement();
            topHorizontalLayout.AddToClassList(InventoryManagerStyles.SubMenuTop);
            Add(topHorizontalLayout);

            var titleLabel = new Label(Title);
            titleLabel.AddToClassList(InventoryManagerStyles.SubMenuTitle);
            topHorizontalLayout.Add(titleLabel);

            m_IconOptions = new VisualElement();
            m_IconOptions.AddToClassList(InventoryManagerStyles.SubMenuIconOptions);
            topHorizontalLayout.Add(m_IconOptions);

            DrawIconOptions();

            var descriptionLabel = new Label();
            descriptionLabel.AddToClassList(InventoryManagerStyles.SubMenuIconDescription);
            descriptionLabel.text = Description;
            Add(descriptionLabel);
        }

        /// <summary>
        /// Draw the Icons for documentation and quick component select.
        /// </summary>
        public virtual void DrawIconOptions()
        {
            m_IconOptions.Clear();

            if (string.IsNullOrWhiteSpace(DocumentationURL) == false) {
                var documentationIcon = new IconOptionButton(IconOption.QuestionMark);
                documentationIcon.tooltip = "Open the documentation";
                documentationIcon.clicked += () =>
                {
                    Application.OpenURL(DocumentationURL);
                };
                m_IconOptions.Add(documentationIcon);
            }

            if (SelectTargetGetter != null) {
                var findIcon = CreateSelectComponentIcon(SelectTargetGetter);
                m_IconOptions.Add(findIcon);
            }
        }

        /// <summary>
        /// Create the Quick Select component Icon.
        /// </summary>
        /// <param name="getObject">The function to get the component to select it.</param>
        /// <returns>The Icon visual element.</returns>
        public static VisualElement CreateSelectComponentIcon(Func<Component> getObject)
        {
            var findIcon = new ComponentSelectionButton(getObject);
            return findIcon;
        }

        /// <summary>
        /// Create a button for boxes with a specific style.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="container">The parent container for the button.</param>
        /// <param name="action">The action when the button is clicked.</param>
        /// <returns>The Button.</returns>
        public static Button CreateButton(string text, VisualElement container, Action action)
        {
            var button = new Button();
            button.AddToClassList(InventoryManagerStyles.SubMenuButton);
            button.style.marginTop = 4;
            button.text = text;
            button.clicked += action;
            container.Add(button);

            return button;
        }

        /// <summary>
        /// Destroy a component in the scene.
        /// </summary>
        /// <param name="obj">The component to destroy.</param>
        public void RemoveComponent(Component obj)
        {
            UIDesignerUtility.RemoveComponent(obj);
        }

        /// <summary>
        /// Destroy a GameObject in the scene.
        /// </summary>
        /// <param name="obj">The GameObject to destroy.</param>
        public void DestroyGameObject(Component obj)
        {
            UIDesignerUtility.DestroyGameObject(obj);
        }

        /// <summary>
        /// Destroy a GameObject in the scene.
        /// </summary>
        /// <param name="obj">The GameObject to destroy.</param>
        public void DestroyGameObject(GameObject obj)
        {
            UIDesignerUtility.DestroyGameObject(obj);
        }

        /// <summary>
        /// Create a default Rect Transform.
        /// </summary>
        /// <param name="parent">The parent of the RectTransform.</param>
        /// <returns>The rect Transform.</returns>
        public RectTransform CreateRectTransform(Transform parent)
        {
            return UIDesignerUtility.CreateRectTransform(parent);
        }
    }

    /// <summary>
    /// A Label with a sub title style.
    /// </summary>
    public class SubTitleLabel : Label
    {
        public SubTitleLabel()
        {
            AddToClassList(InventoryManagerStyles.SubMenuTitle);
        }

        public SubTitleLabel(string text) : this()
        {
            this.text = text;
        }
    }

    /// <summary>
    /// A label with a sub description style.
    /// </summary>
    public class SubDescriptionLabel : Label
    {
        public SubDescriptionLabel()
        {
            style.whiteSpace = WhiteSpace.Normal;
        }

        public SubDescriptionLabel(string text) : this()
        {
            this.text = text;
        }
    }

    /// <summary>
    /// The base class for tab contents.
    /// </summary>
    public abstract class UIDesignerTabContentBase : VisualElement
    {
        public UIDesignerSchema UIDesignerSchema => UIDesignerManager.UIDesignerSchema;
        public UIDesignerManager UIDesignerManager => UIDesignerManager.Instance;

        public abstract string Title { get; }
        public abstract string Description { get; }

        public UIDesignerTabContentBase()
        {
            style.marginTop = 20f;

            var titleLabel = new Label(Title);
            titleLabel.AddToClassList(InventoryManagerStyles.SubMenuTitle);
            Add(titleLabel);

            var descriptionLabel = new Label();
            descriptionLabel.text = Description;
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            Add(descriptionLabel);
        }

        public abstract void Refresh();
    }

    /// <summary>
    /// base class for UI Designer tab Contents with a Create and Edit Layout.
    /// </summary>
    /// <typeparam name="To">The main component type with tab will create and edit.</typeparam>
    /// <typeparam name="Tc">The Creator part of the tab.</typeparam>
    /// <typeparam name="Te">The Editor part of the tab.</typeparam>
    public abstract class UIDesignerCreateEditTabContent<To, Tc, Te> : UIDesignerTabContentBase where Tc : UIDesignerCreator<To>, new() where Te : UIDesignerEditor<To>, new() where To : Component
    {
        protected Tc m_DesignerCreator;
        protected Te m_DesignerEditor;

        public Tc DesignerCreator => m_DesignerCreator;
        public Te DesignerEditor => m_DesignerEditor;

        protected UIDesignerCreateEditTabContent()
        {
            m_DesignerCreator = new Tc();
            m_DesignerCreator.OnCreate += (target) =>
            {
                m_DesignerEditor.SetTarget(target);
            };
            Add(m_DesignerCreator);

            m_DesignerEditor = new Te();
            Add(m_DesignerEditor);
        }

        public override void Refresh()
        {
            m_DesignerCreator.Refresh();
            m_DesignerEditor.Refresh();
        }
    }

    /// <summary>
    /// The UI Designer Creator base class is used to create a component of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the component to create.</typeparam>
    public abstract class UIDesignerCreator<T> : UIDesignerBoxBase
    {
        public event Action<T> OnCreate;

        public override string Title => "Create";
        public override string Description => $"Create a new {typeof(T).Name}";

        protected ObjectField m_ParentTransform;
        protected Button m_BuildButton;
        protected VisualElement m_OptionsContentContainer;
        protected VisualElement m_ConditionWarningsContainer;
        protected InventoryHelpBox m_ConditionHelpBox;

        public ObjectField ParentTransformField => m_ParentTransform;

        /// <summary>
        /// The constructor.
        /// </summary>
        public UIDesignerCreator()
        {
            m_ParentTransform = new ObjectField();
            m_ParentTransform.objectType = typeof(RectTransform);
            m_ParentTransform.label = "Parent Transform";
            m_ParentTransform.RegisterValueChangedCallback(ParentTransformChanged);
            Add(m_ParentTransform);

            m_OptionsContentContainer = new VisualElement();
            Add(m_OptionsContentContainer);

            m_ConditionWarningsContainer = new VisualElement();
            m_ConditionHelpBox = new InventoryHelpBox("No Warnings");
            m_ConditionWarningsContainer.Add(m_ConditionHelpBox);
            Add(m_ConditionWarningsContainer);

            CreateOptionsContent(m_OptionsContentContainer);

            m_BuildButton = new SubMenuButton("Create", Build);
            Add(m_BuildButton);

            Refresh();
        }

        /// <summary>
        /// The parent Transform change event.
        /// </summary>
        /// <param name="evt">The change event.</param>
        protected virtual void ParentTransformChanged(ChangeEvent<Object> evt)
        {
            Refresh();
        }

        /// <summary>
        /// Create the new component and instatiate it in the scene.
        /// </summary>
        public virtual void Build()
        {
            if (BuildCondition(true) == false) {
                return;
            }

            var result = BuildInternal();
            OnCreate?.Invoke(result);
        }

        /// <summary>
        /// Check if the component can be created.
        /// </summary>
        /// <param name="logWarnings">Should warnings be logged if the object cannot be created.</param>
        /// <returns>True if the object can be created.</returns>
        public virtual bool BuildCondition(bool logWarnings)
        {
            var parentRect = m_ParentTransform.value as RectTransform;
            if (parentRect == null) {
                m_ConditionHelpBox.SetMessage("Assign a parent transform indicating where spawn the object.");
                return false;
            }

            var panelManager = parentRect.gameObject.GetComponentInParent<DisplayPanelManager>(true);
            if (panelManager == null) {
                m_ConditionHelpBox.SetMessage("The parent transform must have a parent ancestor with a panel manager (usually located next to the canvas).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Refresh the content to show any updated data.
        /// </summary>
        public virtual void Refresh()
        {
            var buildCondition = BuildCondition(false);

            m_ConditionWarningsContainer.Clear();
            if (buildCondition == false) {
                m_ConditionWarningsContainer.Add(m_ConditionHelpBox);

            }

            m_BuildButton.SetEnabled(buildCondition);
        }

        protected abstract void CreateOptionsContent(VisualElement container);

        protected abstract T BuildInternal();
    }

    /// <summary>
    /// The base class for the Editor part of a create/edit tab, used the edit an existing component in the scene.
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    public abstract class UIDesignerEditor<T> : UIDesignerBoxBase where T : Component
    {
        public override string Title => "Edit";
        public override string Description => $"Edit an existing {typeof(T).Name}";
        protected virtual bool RequireDisplayPanel => true;
        public override Func<Component> SelectTargetGetter => () => m_Target;

        protected DisplayPanel m_DisplayPanel;
        protected T m_Target;
        protected GameObject m_TargetObject;
        protected ObjectField m_TargetField;
        protected Button m_FindTargetButton;

        protected VisualElement m_TargetOptionsContainer;

        public DisplayPanel DisplayPanel => m_DisplayPanel;
        public T Target => m_Target;

        /// <summary>
        /// The constructor.
        /// </summary>
        protected UIDesignerEditor()
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.style.flexDirection = FlexDirection.Row;
            Add(horizontalLayout);

            m_TargetField = new ObjectField("Target UI");
            m_TargetField.style.flexGrow = 1;
            m_TargetField.objectType = typeof(GameObject);
            m_TargetField.RegisterValueChangedCallback(evt =>
            {
                m_TargetObject = evt.newValue as GameObject;
                TargetObjectChanged();
            });
            horizontalLayout.Add(m_TargetField);

            m_FindTargetButton = new Button();
            m_FindTargetButton.style.flexShrink = 1;
            m_FindTargetButton.text = "Find Available Targets in Scene";
            m_FindTargetButton.clicked += FindTargetsInScene;
            horizontalLayout.Add(m_FindTargetButton);

            if (m_Target == null) {
                SelectFirstAvailableTarget();
            }

            m_TargetOptionsContainer = new VisualElement();
            Add(m_TargetOptionsContainer);
        }

        /// <summary>
        /// Refresh to show the up to date data.
        /// </summary>
        public void Refresh()
        {

            TargetObjectChanged();
        }

        /// <summary>
        /// Handles the target object to edit when changed.
        /// </summary>
        protected virtual void TargetObjectChanged()
        {
            m_TargetOptionsContainer.Clear();

            if (m_TargetObject == null) {
                m_Target = null;
                m_DisplayPanel = null;
                return;
            }

            m_Target = m_TargetObject.GetComponent<T>();
            m_DisplayPanel = m_TargetObject.GetComponent<DisplayPanel>();

            if (m_Target == null && m_DisplayPanel == null) {
                Debug.LogWarning($"The field only accepts objects with a a display panel with a child target type or an object with the target type {typeof(T)}.");
                m_TargetField.value = null;
                return;
            }

            if (m_Target == null) {

                m_Target = m_DisplayPanel.GetComponentInChildren<T>(true);
                if (m_Target == null) {
                    Debug.LogWarning($"The field only accepts objects with a a display panel with a child target type or an object with the target type {typeof(T)}.");
                    m_TargetField.value = null;
                    return;
                }
            }

            if (m_DisplayPanel == null) {
                m_DisplayPanel = m_Target.gameObject.GetComponentInParent<DisplayPanel>(true);

                if (RequireDisplayPanel && m_DisplayPanel == null) {
                    Debug.LogWarning($"The target must have a display panel?.");
                    m_TargetField.value = null;
                    return;
                }
            }

            m_TargetField.SetValueWithoutNotify(m_Target);
            NewValidTargetAssigned();
        }

        /// <summary>
        /// Set the new target component to edit.
        /// </summary>
        /// <param name="newTarget">The new targeted component to edit.</param>
        public void SetTarget(T newTarget)
        {
            m_Target = newTarget;
            m_TargetField.value = newTarget?.gameObject;
        }

        /// <summary>
        /// Handles a new Valid Target being assinged.
        /// </summary>
        protected virtual void NewValidTargetAssigned()
        {
        }

        /// <summary>
        /// Select the first available target in the scene.
        /// </summary>
        protected void SelectFirstAvailableTarget()
        {
            m_Target = GameObject.FindObjectOfType<T>();
            m_TargetObject = m_Target?.gameObject;
        }

        /// <summary>
        /// Find target in the scene using a generic menu allowing the user to select a target.
        /// </summary>
        private void FindTargetsInScene()
        {
            var targetOptions = new GenericMenu();

            var rootGameObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

            var optionList = new List<string>();

            for (int i = 0; i < rootGameObjects.Length; i++) {
                var availableTargets = rootGameObjects[i].GetComponentsInChildren<T>(true);

                for (int j = 0; j < availableTargets.Length; j++) {
                    var localIndex = j;

                    var optionName = GetAvailableName(availableTargets[localIndex].name, optionList);

                    targetOptions.AddItem(new GUIContent(optionName), false, () =>
                    {
                        SetTarget(availableTargets[localIndex]);
                    });

                    optionList.Add(optionName);
                }
            }

            targetOptions.ShowAsContext();
        }

        /// <summary>
        /// Get the available Name for the new component.
        /// </summary>
        /// <param name="option">The basic name.</param>
        /// <param name="optionList">The names that are not available.</param>
        /// <returns>An available name.</returns>
        private string GetAvailableName(string option, List<string> optionList)
        {
            var count = 1;
            var newOption = option;

            while (optionList.Contains(newOption)) {
                newOption = $"{option} ({count})";
                count++;
            }

            return newOption;
        }
    }

    /// <summary>
    /// A container for buttons used to create select and or delete a component. 
    /// </summary>
    public class CreateSelectDeleteContainer : VisualElement
    {
        protected string m_Title;
        protected Action m_Create;
        protected Action m_Delete;
        protected Func<Component> m_GetTarget;

        public string CreatePrefix = "Create";
        public string DeletePrefix = "Delete";
        public string SelectPrefix = "Select";

        public bool HasTarget => m_GetTarget?.Invoke() != null;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="title">The title of the container.</param>
        /// <param name="create">The create action.</param>
        /// <param name="delete">The delete action.</param>
        /// <param name="getTarget">The select action.</param>
        public CreateSelectDeleteContainer(string title, Action create, Action delete, Func<Component> getTarget)
        {
            AddToClassList(InventoryManagerStyles.CreateDeleteSelectContainer);

            m_Title = title;

            m_Create = () =>
            {
                create?.Invoke();
                Refresh();
            };

            m_Delete = () =>
            {
                delete?.Invoke();
                Refresh();
            };

            m_GetTarget = getTarget;
        }

        /// <summary>
        /// Refresh to show the up to date data.
        /// </summary>
        public void Refresh()
        {
            Clear();

            var target = m_GetTarget();
            if (target == null) {
                var createButton = new SubMenuButton(CreatePrefix + " " + m_Title, m_Create);
                Add(createButton);
                return;
            }

            var deleteButton = new SubMenuButton(DeletePrefix + " " + m_Title, m_Delete);
            Add(deleteButton);

            var componentSelect = new ComponentSelectionButton(SelectPrefix + " " + m_Title, m_GetTarget);
            Add(componentSelect);
        }
    }

    /// <summary>
    /// A button with a sub menu styling.
    /// </summary>
    public class SubMenuButton : Button
    {
        public SubMenuButton(string buttonText, Action action)
        {
            AddToClassList(InventoryManagerStyles.SubMenuButton);
            text = buttonText;
            clicked += action;

        }

        public SubMenuButton(string buttonText, float width, Action action) : this(buttonText, action)
        {
            style.width = width;
        }
    }

    /// <summary>
    /// A button used to select a component in the scene hierarchy.
    /// </summary>
    public class ComponentSelectionButton : VisualElement
    {
        private Func<Component> m_GetObject;

        public ComponentSelectionButton(Func<Component> GetObject)
        {
            m_GetObject = GetObject;

            var findIcon = new IconOptionButton(IconOption.MagnifyingGlass);
            findIcon.tooltip = "Select the component in the hierarchy.";
            findIcon.clicked += SelectInHierarchy;
            Add(findIcon);
        }

        public ComponentSelectionButton(string buttonText, Func<Component> GetObject)
        {
            m_GetObject = GetObject;

            var button = new SubMenuButton(null, SelectInHierarchy);
            button.tooltip = "Select the component in the hierarchy.";
            button.style.flexDirection = FlexDirection.Row;

            var icon = new IconOptionImage(IconOption.MagnifyingGlass);
            button.Add(icon);
            var label = new Label(buttonText);
            button.Add(label);

            Add(button);
        }

        public void SelectInHierarchy()
        {
            ComponentSelection.Select(m_GetObject());
        }
    }

    /// <summary>
    /// Static Utility class to select components in the scene hierarchy.
    /// </summary>
    public static class ComponentSelection
    {
        /// <summary>
        /// Static Utility function to select components in the scene hierarchy.
        /// </summary>
        /// <param name="obj">The component to select.</param>
        public static void Select(Component obj)
        {
            Selection.SetActiveObjectWithContext(obj, obj);
            if (obj == null) { return; }

            var allComponentsOnGameobject = obj.gameObject.GetComponents<Component>();
            for (int i = 0; i < allComponentsOnGameobject.Length; i++) {
                var component = allComponentsOnGameobject[i];
                var componentMatch = component == obj;

                UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(component, componentMatch);
            }

            Selection.SetActiveObjectWithContext(obj, obj);
        }
    }

    /// <summary>
    /// Static Utility class for the UI Designer Manager.
    /// </summary>
    public static class UIDesignerUtility
    {
        /// <summary>
        /// Destroy Component.
        /// </summary>
        /// <param name="obj">The component to destroy.</param>
        public static void RemoveComponent(Component obj)
        {
            if (obj == null) { return; }
            GameObject.DestroyImmediate(obj);
        }

        /// <summary>
        /// Destroy GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to destroy.</param>
        public static void DestroyGameObject(Component obj)
        {
            if (obj == null) { return; }
            GameObject.DestroyImmediate(obj.gameObject);
        }

        /// <summary>
        /// Destroy GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to destroy.</param>
        public static void DestroyGameObject(GameObject obj)
        {
            if (obj == null) { return; }
            GameObject.DestroyImmediate(obj);
        }

        /// <summary>
        /// Create a rect transform.
        /// </summary>
        /// <param name="parent">The paren of th rect transform.</param>
        /// <returns>The rect transform.</returns>
        public static RectTransform CreateRectTransform(Transform parent)
        {
            var rect = new GameObject().AddComponent<RectTransform>();
            rect.SetParent(parent);
            rect.anchoredPosition = Vector2.zero;
            return rect;
        }

        /// <summary>
        /// Find a component in the parent of a gameobject.
        /// </summary>
        /// <param name="obj">The gameobject.</param>
        /// <param name="includeInactive">Include inactive gameaobjects.</param>
        /// <param name="includeThis">Include this gameobject.</param>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <returns>The component found.</returns>
        public static T GetComponentInParent<T>(this GameObject obj, bool includeInactive, bool includeThis = true) where T : Component
        {
            if (includeThis) {
                var component = obj.GetComponent<T>();
                if (component != null) { return component; }
            }

            if (includeInactive == false) {
                return obj.GetComponentInParent<T>();
            }

            var targetParent = obj.transform.parent;
            while (targetParent != null) {
                var component = targetParent.GetComponent<T>();

                if (component != null) {
                    return component;
                }

                targetParent = targetParent.parent;
            }

            return null;
        }

        /// <summary>
        /// Find all object of type.
        /// </summary>
        /// <typeparam name="T">The component type to look for.</typeparam>
        /// <returns>A list of the components found.</returns>
        public static List<T> FindAllObjectsOfType<T>() where T : Component
        {
            var rootGameObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

            var allObjects = new List<T>();

            for (int i = 0; i < rootGameObjects.Length; i++) {
                var availableTargets = rootGameObjects[i].GetComponentsInChildren<T>(true);

                allObjects.AddRange(availableTargets);
            }

            return allObjects;
        }
    }
}
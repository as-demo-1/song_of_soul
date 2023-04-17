/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.Inspectors.Utility;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.Shared.Game;
    using Opsive.Shared.Input;
    using Opsive.UltimateInventorySystem.Core;
    using Opsive.UltimateInventorySystem.Core.AttributeSystem;
    using Opsive.UltimateInventorySystem.Core.InventoryCollections;
    using Opsive.UltimateInventorySystem.DropsAndPickups;
    using Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.Exchange;
    using Opsive.UltimateInventorySystem.Input;
    using Opsive.UltimateInventorySystem.Interactions;
    using Opsive.UltimateInventorySystem.ItemActions;
    using Opsive.UltimateInventorySystem.SaveSystem;
    using Opsive.UltimateInventorySystem.Storage;
    using Opsive.UltimateInventorySystem.UI.DataContainers;
    using Opsive.UltimateInventorySystem.UI.Item.ItemViewModules;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;
    using Toggle = UnityEngine.UIElements.Toggle;


    /// <summary>
    /// The SetupManager shows any project or scene related setup options.
    /// </summary>
    [OrderedEditorItem("Setup", 10)]
    public class SetupManager : InventoryManager
    {
        private string c_2DAudioManagerModuleGUID = "4ef5ebb9e2599504d921283f4e04cc6f";
        
        protected VisualElement m_Container;

        private InventorySystemDatabaseMainField m_InventorySystemDatabaseMainField;
        private ObjectField m_CharacterGameObject;
        private ObjectField m_GameobjectToSave;

        protected CreateTemplates m_CreateTemplates;

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            // Database Creation.
            m_InventorySystemDatabaseMainField = new InventorySystemDatabaseMainField(m_InventoryMainWindow);

            m_ManagerContentContainer.Add(m_InventorySystemDatabaseMainField);

            m_Container = new ScrollView(ScrollViewMode.Vertical);

            CreateMenuBox("Input Setup", "Adds the required inputs to the Unity Input Manager.", null, "Add Inputs", () =>
            {
                var setupProject = EditorUtility.DisplayDialog("Change the inputs in the Input Manager?",
                                      $"This change will affect your project settings and cannot be undone." +
                                      $"Are you sure you would like to make the change?",
                                      "Yes",
                                      "No");

                if (setupProject) {
                    InventoryInputBuilder.UpdateInputManager();
                    Debug.Log("The inputs were updated.");
                }
            }, m_Container, true);
            
            CreateMenuBox("Scene Setup", "Adds the Inventory System Manager and other requires manager components to the scene under a 'Game' GameObject.", null, "Add Components", () =>
            {
                AddSceneComponents();
                Debug.Log("The components have been added.");
            }, m_Container, true);

            CreateMenuBox("Character Setup", "Adds the Inventory, Item User, Unity Input, Inventory Interactor, Currency Owner and Inventory Identifier components on the game object.", AddCharacterOptions, "Add Components", SetupCharacter, m_Container, true);

            CreateMenuBox("Save Setup", "Adds the Inventory Saver and/or the Currency Owner Saver on the selected object.\n" +
                                        "Adds the Save Manager and Inventory System Manager Item Saver if they are missing.", AddSaveOptions, "Add Components", SetupSave, m_Container, true);

            CreateMenuBox("UI Setup", "You may set up and create UI using the UI Designer Manager.", null, "Open UI Designer", () => { InventoryMainWindow.ShowUIDesignerWindow(); }, m_Container, true);

            m_CreateTemplates = new CreateTemplates();
            m_Container.Add(m_CreateTemplates);

            m_ManagerContentContainer.Add(m_Container);
        }

        /// <summary>
        /// Check if the required assets for the menu setups are available.
        /// </summary>
        /// <returns>Return true if at least one of the required assets is missing.</returns>
        private bool AreDemoAssetsMissing()
        {
            return Shared.Editor.Utility.EditorUtility.LoadAsset<GameObject>("f93adf60a19012a4c9c35fb7595903d0") == null ||
                    Shared.Editor.Utility.EditorUtility.LoadAsset<GameObject>("25eea7b1384f975459f5e67c2b9b0bde") == null ||
                    Shared.Editor.Utility.EditorUtility.LoadAsset<CategoryItemViewSet>("b0286cb637ba1a64f81828f2dddf84ad") == null;
        }

        /// <summary>
        /// Creates a standard box that can be used to show menu content.
        /// </summary>
        /// <param name="title">The title of the box.</param>
        /// <param name="description">The description of the box.</param>
        /// <param name="options">Any additional options (can be null).</param>
        /// <param name="buttonTitle">The title of the action button.</param>
        /// <param name="buttonAction">The action of the button.</param>
        /// <param name="parent">The VisualElement that the box should be added to.</param>
        public static void CreateMenuBox(string title, string description, Action<VisualElement> options, string buttonTitle, Action buttonAction, VisualElement parent, bool enableButton)
        {
            var container = new VisualElement();
            container.AddToClassList(InventoryManagerStyles.SubMenu);
            container.style.marginTop = 20f;
            var titleLabel = new Label(title);
            titleLabel.AddToClassList(InventoryManagerStyles.SubMenuTitle);
            container.Add(titleLabel);
            var destriptionLabel = new Label();
            destriptionLabel.text = description;
            container.Add(destriptionLabel);
            if (options != null) {
                destriptionLabel.style.marginBottom = 4;
                options(container);
            }

            if (buttonAction != null) {
                var button = new Button();
                button.AddToClassList(InventoryManagerStyles.SubMenuButton);
                button.style.marginTop = 4;
                button.text = buttonTitle;
                button.clicked += buttonAction;
                button.SetEnabled(enableButton);
                container.Add(button);
            }

            parent.Add(container);
        }

        /// <summary>
        /// Adds the singleton components to the scene.
        /// </summary>
        private void AddSceneComponents()
        {
            // Create the "Game" components if it doesn't already exists.
            Scheduler scheduler;
            GameObject gameGameObject;
            if ((scheduler = GameObject.FindObjectOfType<Scheduler>()) == null) {
                gameGameObject = new GameObject("Game");
            } else {
                gameGameObject = scheduler.gameObject;
            }

            // Add the Singletons.
            var inventorySystemManager = InspectorUtility.AddComponent<Core.InventorySystemManager>(gameGameObject);
            if (m_InventoryMainWindow.Database != null) {
                inventorySystemManager.Database = m_InventoryMainWindow.Database;
            }

            var spawner = InspectorUtility.AddComponent<ItemObjectSpawner>(gameGameObject);
            spawner.ItemObjectPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    AssetDatabase.GUIDToAssetPath("99f2ccf6e56f42146ad63b866481f569"))?.GetComponent<ItemObject>();

            InspectorUtility.AddComponent<Scheduler>(gameGameObject);
            InspectorUtility.AddComponent<ObjectPool>(gameGameObject);
            
            var audiomanager = InspectorUtility.AddComponent<Shared.Audio.AudioManager>(gameGameObject);

            var defaultAudioManagerModulePath = AssetDatabase.GUIDToAssetPath(c_2DAudioManagerModuleGUID);
            if (!string.IsNullOrEmpty(defaultAudioManagerModulePath) && audiomanager.AudioManagerModule == null) {
                var audioManagerModule = AssetDatabase.LoadAssetAtPath(defaultAudioManagerModulePath, typeof(Shared.Audio.AudioManagerModule)) as Shared.Audio.AudioManagerModule;
                audiomanager.AudioManagerModule = audioManagerModule;
            }
        }

        /// <summary>
        /// Adds the options for the character setup.
        /// </summary>
        /// <param name="parent">The VisualElement that the options should be parented to.</param>
        private void AddCharacterOptions(VisualElement parent)
        {
            // The Player tag will be used to attempt to auto populate the objects.
            var player = GameObject.FindWithTag("Player");

            m_CharacterGameObject = new ObjectField();
            m_CharacterGameObject.tooltip = "The character game object where the inventory components will be added.";
            m_CharacterGameObject.label = "Character Gameobject";
            m_CharacterGameObject.objectType = typeof(GameObject);
            m_CharacterGameObject.value = player;
            parent.Add(m_CharacterGameObject);
        }

        /// <summary>
        /// Adds the options for the save menu box.
        /// </summary>
        /// <param name="parent">The VisualElement that the options should be parented to.</param>
        private void AddSaveOptions(VisualElement parent)
        {
            m_GameobjectToSave = new ObjectField();
            m_GameobjectToSave.tooltip = "The inventory or currency owner that should be saved.";
            m_GameobjectToSave.label = "Object to save";
            m_GameobjectToSave.objectType = typeof(GameObject);
            parent.Add(m_GameobjectToSave);
        }

        /*
        /// <summary>
        /// Creates a new SciptableObject with the specified name.
        /// </summary>
        /// <param name="name">The name of the ScriptableObject.</param>
        private T CreateScriptableObject<T>(string name) where T : ScriptableObject
        {
            var path = EditorUtility.SaveFilePanel($"Create {typeof(T)}", "Assets", name, "asset");
            if (path.Length == 0 || Application.dataPath.Length >= path.Length) { return null; }
            path = $"Assets/{path.Substring(Application.dataPath.Length + 1)}";

            // Persists the object.
            var obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.ImportAsset(path);
            return obj;
        }*/

        /// <summary>
        /// Instantiate a prefab.
        /// </summary>
        /// <param name="obj">The prefab to instantiate.</param>
        /// <returns>The instance of the prefab.</returns>
        private GameObject InstantiatePrefab(GameObject obj, Transform parent = null)
        {
            if (parent == null) {
                return GameObject.Instantiate(obj);
            } else {
                return GameObject.Instantiate(obj, parent);
            }
        }

        /// <summary>
        /// Set up the character components.
        /// </summary>
        private void SetupCharacter()
        {
            var character = m_CharacterGameObject.value as GameObject;

            if (character == null) { return; }

            if (character.GetComponent<InventoryIdentifier>() == null) {
                character.AddComponent<InventoryIdentifier>();
            }

            if (character.GetComponent<Inventory>() == null) { character.AddComponent<Inventory>(); }
            if (character.GetComponent<ItemUser>() == null) { character.AddComponent<ItemUser>(); }

            if (character.GetComponent<UnityInput>() == null) {
                var input = character.AddComponent<UnityInput>();
                input.EnableCursorWithEscape = false;
                input.DisableCursor = false;
                character.GetComponent<ItemUser>().InventoryInput = input;
            }

            if (character.GetComponent<CurrencyOwner>() == null) {
                character.AddComponent<CurrencyOwner>();
            }

            if (character.GetComponent<InventoryInteractor>() == null) {
                character.AddComponent<InventoryInteractor>();
            }
        }

        /// <summary>
        /// Sets up the saving and loading components/UI.
        /// </summary>
        private void SetupSave()
        {
            // Setup the managers.
            AddSceneComponents();
            var gameGameObject = GameObject.FindObjectOfType<InventorySystemManager>().gameObject;
            InspectorUtility.AddComponent<SaveSystem.SaveSystemManager>(gameGameObject);
            InspectorUtility.AddComponent<SaveSystem.InventorySystemManagerItemSaver>(gameGameObject);

            //Add the saver components on the player inventory and currency owner
            var targetGameObject = m_GameobjectToSave.value as GameObject;
            if (targetGameObject == null) { return; }

            var inventory = targetGameObject.GetComponent<Inventory>();
            if (inventory != null) {
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
                if (targetGameObject.GetComponent("UltimateInventorySystemBridge") == null) {
                    InspectorUtility.AddComponent<InventorySaver>(targetGameObject);
                } else {
                    Debug.Log("The UCC integration character cannot use the default Inventory Saver component. Please add an 'Inventory Bridge Saver' component on your character manually instead.");
                }
#else
                InspectorUtility.AddComponent<InventorySaver>(targetGameObject);
#endif

            }

            var currencyOwner = targetGameObject.GetComponent<CurrencyOwner>();
            if (currencyOwner != null) {
                InspectorUtility.AddComponent<CurrencyOwnerSaver>(targetGameObject);
            }
        }

        /// <summary>
        /// The database has been changed.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            m_InventorySystemDatabaseMainField.Refresh();
        }

        public abstract class SetupBoxBase : VisualElement
        {
            protected VisualElement m_IconOptions;

            public virtual string DocumentationURL => "yo";
            public abstract string Title { get; }
            public abstract string Description { get; }

            public SetupBoxBase()
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
            }
        }

        public class CreateTemplates : SetupBoxBase
        {
            public override string Title => "Create Templates";

            public override string Description =>
                "Create templates for different types of Pickups and Item Object which can easily be reused.\n" +
                "Instead of creating one for each item, Create just a few a replace the model/sprite/components/values dynamically when binding it with an Item, Currency, etc...";

            private List<TemplateOption> m_TemplateOptions;
            private PopupField<TemplateOption> m_OptionPopupField;
            private VisualElement m_SelectionContainer;

            private InventoryHelpBox m_HelpBox;
            private Button m_CreateButton;

            protected TemplateOption m_SelectedTemplate;

            public CreateTemplates()
            {

                m_TemplateOptions = new List<TemplateOption>();

                m_TemplateOptions.Add(new ItemPickupTemplateOption());
                m_TemplateOptions.Add(new InventoryPickupTemplateOption());
                m_TemplateOptions.Add(new RandomInventoryPickupTemplateOption());
                m_TemplateOptions.Add(new CurrencyPickupTemplateOption());

                m_SelectedTemplate = m_TemplateOptions[0];
                m_SelectedTemplate.OnValueChanged += Refresh;

                m_OptionPopupField = new PopupField<TemplateOption>("Option", m_TemplateOptions, 0,
                    (template) => template.Name,
                    (template) => template.Name);
                m_OptionPopupField.RegisterValueChangedCallback(evt =>
                {
                    m_SelectedTemplate.OnValueChanged -= Refresh;
                    m_SelectedTemplate = evt.newValue;
                    m_SelectedTemplate.OnValueChanged += Refresh;
                    Refresh();
                });
                Add(m_OptionPopupField);

                m_SelectionContainer = new VisualElement();
                Add(m_SelectionContainer);

                m_HelpBox = new InventoryHelpBox("All good");

                m_CreateButton = new Button();
                m_CreateButton.AddToClassList(InventoryManagerStyles.SubMenuButton);
                m_CreateButton.style.marginTop = 4;
                m_CreateButton.text = "Create";
                m_CreateButton.clicked += CreateTemplate;

                Add(m_CreateButton);

                Refresh();
            }

            private void CreateTemplate()
            {
                m_SelectedTemplate.CreateTemplate();
            }

            public void Refresh()
            {
                m_SelectedTemplate.Refresh();

                m_SelectionContainer.Clear();
                var description = new Label(m_SelectedTemplate.Description);
                description.style.marginLeft = 15;
                m_SelectionContainer.Add(description);

                (bool valid, string message) result = m_SelectedTemplate.CanCreate();

                if (result.valid == false) {
                    m_HelpBox.SetMessage(result.message);
                    m_SelectionContainer.Add(m_HelpBox);
                }

                m_SelectionContainer.Add(m_SelectedTemplate);

                m_CreateButton.SetEnabled(result.valid);
            }

            public abstract class TemplateOption : VisualElement
            {
                public event Action OnValueChanged;

                public virtual string Name { get; }
                public virtual string Description { get; }

                public abstract void Refresh();
                public abstract (bool valid, string message) CanCreate();
                public abstract void CreateTemplate();

                protected virtual void ValueChanged()
                {
                    OnValueChanged?.Invoke();
                }
            }

            public abstract class PickupBaseTemplateOption : TemplateOption
            {
                protected ObjectField m_DefaultPickupPrefab;
                protected Toggle m_3DPickup;

                public PickupBaseTemplateOption()
                {
                    m_DefaultPickupPrefab = new ObjectField("Default Pickup Model Prefab");
                    m_DefaultPickupPrefab.objectType = typeof(GameObject);
                    m_DefaultPickupPrefab.tooltip = "The Default Pickup Model must be specified to proceed";
                    m_DefaultPickupPrefab.RegisterValueChangedCallback(evt =>
                    {
                        ValueChanged();
                    });
                    Add(m_DefaultPickupPrefab);

                    m_3DPickup = new Toggle("3D Pickup");
                    m_3DPickup.value = true;
                    Add(m_3DPickup);
                }

                public override (bool valid, string message) CanCreate()
                {
                    if (m_DefaultPickupPrefab.value == null) {
                        return (false, "A Default Model Pickup Prefab must be specified");
                    }

                    return (true, null);
                }

                public (GameObject pickup, GameObject modelParent) CreatePickupGameObject(string pickupName)
                {
                    var pickupGameObject = new GameObject(pickupName);
                    pickupGameObject.transform.position = new Vector3(0, 1, 0);

                    if (pickupGameObject.GetComponent<Interactable>() == null) {
                        pickupGameObject.AddComponent<Interactable>();
                    }

                    var modelParent = new GameObject("Model Parent");
                    modelParent.transform.SetParent(pickupGameObject.transform);
                    modelParent.transform.localPosition = Vector3.zero;

                    if (m_3DPickup.value) {
                        //3D
                        pickupGameObject.AddComponent<Rigidbody>();
                        var boxCollider = pickupGameObject.AddComponent<BoxCollider>();
                        boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
                        boxCollider.center = new Vector3(0, -0.25f, 0f);

                        var sphereCollider = modelParent.AddComponent<SphereCollider>();
                        sphereCollider.isTrigger = true;
                    } else {
                        //2D
                        pickupGameObject.AddComponent<Rigidbody2D>();
                        pickupGameObject.AddComponent<BoxCollider2D>();

                        var circleCollider = modelParent.AddComponent<CircleCollider2D>();
                        circleCollider.isTrigger = true;
                    }

                    var model = GameObject.Instantiate(m_DefaultPickupPrefab.value as GameObject, modelParent.transform);

                    return (pickupGameObject, modelParent);
                }
            }

            public class ItemPickupTemplateOption : PickupBaseTemplateOption
            {
                public override string Name => "Item Pickup";
                public override string Description => "The Item Pickup is used to pickup an item amount from an Item Object.";

                private ObjectField m_ItemViewPickupPrefab;

                public ItemPickupTemplateOption()
                {
                    m_ItemViewPickupPrefab = new ObjectField("Item View Prefab");
                    m_ItemViewPickupPrefab.objectType = typeof(GameObject);
                    m_ItemViewPickupPrefab.tooltip = "The Item View Prefab is optional, it will be added in a world canvas on top of the item pickup";
                    m_ItemViewPickupPrefab.RegisterValueChangedCallback(evt =>
                    {
                        ValueChanged();
                    });
                    Add(m_ItemViewPickupPrefab);
                }

                public override void Refresh()
                {

                }

                public override (bool valid, string message) CanCreate()
                {
                    var result = base.CanCreate();

                    if (result.valid == false) {
                        return result;
                    }

                    var itemViewPrefab = m_ItemViewPickupPrefab.value as GameObject;
                    if (itemViewPrefab != null && itemViewPrefab.GetComponent<ItemView>() == false) {
                        return (false, "The Item View Prefab must have an Item View component at it's root");
                    }

                    return (true, null);
                }

                public override void CreateTemplate()
                {

                    var (itemPickupGameObject, modelParent) = CreatePickupGameObject("Item Pickup Template");
                    itemPickupGameObject.AddComponent<ItemObject>();
                    var pickup = itemPickupGameObject.AddComponent<ItemPickup>();
                    var visualListener = itemPickupGameObject.AddComponent<ItemObjectVisualizer>();

                    ComponentSelection.Select(pickup);

                    visualListener.m_ItemPrefabVisualizerParent = modelParent.transform;
                    visualListener.m_DefaultVisualPrefab = m_DefaultPickupPrefab.value as GameObject;

                    var itemViewPrefab = m_ItemViewPickupPrefab.value as GameObject;
                    if (itemViewPrefab == null) { return; }

                    //Add a canvas with the Item View

                    var pickupCanvas = new GameObject("Pickup Canvas").AddComponent<Canvas>();
                    var rectTransform = pickupCanvas.transform as RectTransform;
                    rectTransform.SetParent(itemPickupGameObject.transform);
                    pickupCanvas.gameObject.AddComponent<CanvasScaler>();

                    rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                    rectTransform.localPosition = new Vector3(0, 0.7f, 0);
                    rectTransform.sizeDelta = Vector2.zero;

                    var itemView = GameObject.Instantiate(itemViewPrefab, pickupCanvas.transform).GetComponent<ItemView>();

                    visualListener.ItemView = itemView;
                    pickup.m_SelectIndicators = new[] { pickupCanvas.gameObject };
                }
            }

            public class InventoryPickupTemplateOption : PickupBaseTemplateOption
            {
                public override string Name => "Inventory Pickup";
                public override string Description => "The Inventory Pickup is used to pickup a set of items.";

                public InventoryPickupTemplateOption()
                { }

                public override void Refresh()
                { }

                public override void CreateTemplate()
                {

                    var (itemPickupGameObject, modelParent) = CreatePickupGameObject("Inventory Pickup Template");
                    itemPickupGameObject.AddComponent<Inventory>();
                    var pickup = itemPickupGameObject.AddComponent<InventoryPickup>();

                    ComponentSelection.Select(pickup);
                }
            }

            public class RandomInventoryPickupTemplateOption : PickupBaseTemplateOption
            {
                public override string Name => "Random Inventory Pickup";
                public override string Description => "The Random Inventory Pickup is used to pickup a random amount of items from a set defined in the Inventory.\n" +
                                                      "The items amounts are used as a probability table.";

                public RandomInventoryPickupTemplateOption()
                { }

                public override void Refresh()
                { }

                public override void CreateTemplate()
                {

                    var (itemPickupGameObject, modelParent) = CreatePickupGameObject("Inventory Pickup Template");
                    itemPickupGameObject.AddComponent<Inventory>();
                    var pickup = itemPickupGameObject.AddComponent<RandomInventoryPickup>();

                    ComponentSelection.Select(pickup);
                }
            }

            public class CurrencyPickupTemplateOption : PickupBaseTemplateOption
            {
                public override string Name => "Currency Pickup";
                public override string Description => "The Currency Pickup is used to pickup a set amount of currency.";

                public CurrencyPickupTemplateOption()
                { }

                public override void Refresh()
                { }

                public override void CreateTemplate()
                {

                    var (itemPickupGameObject, modelParent) = CreatePickupGameObject("Currency Pickup Template");
                    itemPickupGameObject.AddComponent<CurrencyOwner>();
                    var pickup = itemPickupGameObject.AddComponent<CurrencyPickup>();

                    ComponentSelection.Select(pickup);
                }
            }
        }
    }
}
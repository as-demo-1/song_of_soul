/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers.UIDesigner
{
    using Opsive.UltimateInventorySystem.Editor.Utility;
    using Opsive.UltimateInventorySystem.Editor.VisualElements;
    using Opsive.UltimateInventorySystem.UI;
    using Opsive.UltimateInventorySystem.UI.Item;
    using Opsive.UltimateInventorySystem.UI.Panels;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Opsive.Shared.Editor.UIElements;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine.UIElements;
    using Button = UnityEngine.UIElements.Button;
    using Image = UnityEngine.UIElements.Image;
    using Object = UnityEngine.Object;

    public class SetupDesigner : UIDesignerTabContentBase
    {
        public override string Title => "Setup";

        public override string Description =>
            "UI Designer allows you to create and edit your UI related to the Inventory System.\n" +
            "The objects created and edited must be in the scene. Prefabs cannot be edited. " +
            "Get started by setting up the canvas managers and spawn a UI schema.";

        protected CanvasManagerCreator m_CanvasManagerCreator;
        protected UIDesignerSchemaSetupSubMenu m_UIDesignerSchemaSetupSubMenu;

        public SetupDesigner()
        {
            m_CanvasManagerCreator = new CanvasManagerCreator();
            Add(m_CanvasManagerCreator);

            m_UIDesignerSchemaSetupSubMenu = new UIDesignerSchemaSetupSubMenu();
            Add(m_UIDesignerSchemaSetupSubMenu);
        }

        public override void Refresh()
        {
            m_CanvasManagerCreator.Refresh();
            m_UIDesignerSchemaSetupSubMenu.Refresh();
        }
    }

    public class CanvasManagerCreator : UIDesignerBoxBase
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/ui/display-panel-manager/";

        public override string Title => "Create Canvas Managers";

        public override string Description =>
            "Create the canvas with the panel manager and other common components.\n" +
            "There can be multiple canvases with panel managers (useful for split screen setups).";

        private Button m_Button;

        public CanvasManagerCreator()
        {
            m_Button = new SubMenuButton("Setup", CreateCanvasManagers);
            Add(m_Button);
        }

        private void CreateCanvasManagers()
        {
            Debug.Log("Create Canvas with Managers");

            var canvasGameObject = new GameObject("Inventory Canvas");
            canvasGameObject.layer = 5; //UI layer

            var canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGameObject.AddComponent<GraphicRaycaster>();

            canvasGameObject.AddComponent<PreventSelectableDeselection>();
            canvasGameObject.AddComponent<ItemViewSlotCursorManager>();
            canvasGameObject.AddComponent<DisplayPanelManagerHandler>();
            var panelManager = canvasGameObject.AddComponent<DisplayPanelManager>();

            var eventSystem = GameObject.FindObjectOfType<EventSystem>();
            if (eventSystem == null) {
                eventSystem = new GameObject("Event System").AddComponent<EventSystem>();
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }

            panelManager.PanelOwner = GameObject.FindWithTag("Player");

            //CreateGameplayPanel(canvasGameObject, panelManager);

            UIDesignerManager.Refresh();
        }

        private static void CreateGameplayPanel(GameObject canvasGameObject, DisplayPanelManager panelManager)
        {
            var gamePlayPanelRect = UIDesignerUtility.CreateRectTransform(canvasGameObject.transform);

            var gamePlayPanel = gamePlayPanelRect.gameObject.AddComponent<DisplayPanel>();
            gamePlayPanel.gameObject.name = "Gameplay Panel";
            gamePlayPanel.m_UniqueName = "Gameplay Panel";

            gamePlayPanel.m_StartEnabled = true;
            gamePlayPanel.m_OpenOnStart = true;

            panelManager.GameplayPanel = gamePlayPanel;
        }

        public void Refresh()
        {
        }
    }

    public class UIDesignerSchemaSetupSubMenu : UIDesignerBoxBase
    {
        public override string DocumentationURL =>
            "https://opsive.com/support/documentation/ultimate-inventory-system/editor-window/ui-designer-schemas/";

        public override string Title => "UI Designer Schema";

        public override string Description =>
            "The schema is a collection of modular assets required by UI Designer to create and edit your inventory UI.\n" +
            "Create your own schema by duplicating one of the available ones below.\n";

        private List<UIDesignerSchema> m_Schemas;

        protected VisualElement m_SchemaSelectionContainer;
        protected PopupField<UIDesignerSchema> m_SchemaPopupField;
        protected Button m_ValidateButton;
        protected Button m_DuplicateButton;

        protected VisualElement m_ValidateResultContainer;
        protected InventoryHelpBox m_HelpBox;
        protected Button m_FullSetupButton;

        protected VisualElement m_SchemaDescription;
        protected ObjectField m_PanelManagerField;

        protected NestedScriptableObjectInspector<UIDesignerSchema, UIDesignerSchemaInspector>
            m_UIDesignerSchemaNestedInspector;

        public UIDesignerSchemaSetupSubMenu()
        {
            m_Schemas = new List<UIDesignerSchema>();
            FetchSchemas();

            m_PanelManagerField = new ObjectField("DisplayManager");
            m_PanelManagerField.objectType = typeof(DisplayPanelManager);
            m_PanelManagerField.value = GameObject.FindObjectOfType<DisplayPanelManager>();
            m_PanelManagerField.RegisterValueChangedCallback(evt => { Refresh(); });
            Add(m_PanelManagerField);

            m_SchemaSelectionContainer = new VisualElement();
            m_SchemaSelectionContainer.style.flexDirection = FlexDirection.Row;
            Add(m_SchemaSelectionContainer);

            if (UIDesignerSchema == null) {
                if (m_Schemas.Count != 0) {
                    UIDesignerManager.UIDesignerSchema = m_Schemas[0];
                }
            }

            var defaultValue = UIDesignerSchema;
            if (defaultValue == null) {
                Debug.LogError(
                    "ERROR: No Schemas were found in the project, make sure at least one exist. Try Reimporting the UIDesignerSchema asset if one does exist");
                return;
            }

            m_SchemaPopupField = new PopupField<UIDesignerSchema>("Schema ", m_Schemas, defaultValue,
                x => x.name,
                x => x.name);
            m_SchemaPopupField.style.flexGrow = 1;
            m_SchemaPopupField.RegisterValueChangedCallback(evt =>
            {
                UpdateUIDesignerSchema(evt.newValue);
            });
            m_SchemaSelectionContainer.Add(m_SchemaPopupField);

            m_ValidateButton = new Button();
            m_ValidateButton.text = "Validate";
            m_ValidateButton.clicked += ValidateSchema;
            m_SchemaSelectionContainer.Add(m_ValidateButton);

            m_DuplicateButton = new Button();
            m_DuplicateButton.text = "Duplicate";
            m_DuplicateButton.clicked += DuplicateSchema;
            m_SchemaSelectionContainer.Add(m_DuplicateButton);

            m_ValidateResultContainer = new VisualElement();
            Add(m_ValidateResultContainer);

            m_HelpBox = new InventoryHelpBox("All Good!");

            m_FullSetupButton = new SubMenuButton("Spawn In Scene", FullUISetup);
            Add(m_FullSetupButton);

            m_SchemaDescription = new VisualElement();
            Add(m_SchemaDescription);

            m_UIDesignerSchemaNestedInspector =
                new NestedScriptableObjectInspector<UIDesignerSchema, UIDesignerSchemaInspector>(null);
            m_UIDesignerSchemaNestedInspector.Refresh(UIDesignerSchema);
            Add(m_UIDesignerSchemaNestedInspector);
        }

        public void Refresh()
        {
            FetchSchemas();

            m_FullSetupButton.SetEnabled(false);

            var schema = UIDesignerSchema;
            if (schema == null) { return; }

            var isSchemaFromOpsive = UIDesignerManager.IsSchemaFromOpsive();

            m_UIDesignerSchemaNestedInspector.Refresh(schema);
            m_UIDesignerSchemaNestedInspector.SetEnabled(isSchemaFromOpsive == false);

            m_SchemaDescription.Clear();
            m_SchemaDescription.Add(new SubTitleLabel(schema.name));
            m_SchemaDescription.Add(new SubDescriptionLabel(schema.m_Description));

            var horizontal = new VisualElement();
            horizontal.style.flexDirection = FlexDirection.Row;
            horizontal.style.maxHeight = 200;
            horizontal.style.alignItems = Align.Stretch;
            m_SchemaDescription.Add(horizontal);
            if (schema.m_Images != null) {
                for (int i = 0; i < schema.m_Images.Length; i++) {
                    var image = new Image();
                    image.image = schema.m_Images[i];
                    image.scaleMode = ScaleMode.ScaleToFit;
                    horizontal.Add(image);
                }
            }

            if (m_PanelManagerField.value == null) {
                m_PanelManagerField.SetValueWithoutNotify(GameObject.FindObjectOfType<DisplayPanelManager>());
            }

            m_ValidateResultContainer.Clear();

            var editorResult = UIDesignerManager.UIDesignerValidationResult;

            if (schema == null) {
                m_ValidateButton.SetEnabled(false);
                m_DuplicateButton.SetEnabled(false);
                m_HelpBox.SetMessage(
                    "A UI Designer schema must be created and assigned to use the UI Designer Editor Manager.");
                m_ValidateResultContainer.Add(m_HelpBox);
                return;
            }

            m_ValidateButton.SetEnabled(true);
            m_DuplicateButton.SetEnabled(true);

            if (editorResult.isValid == false) {
                m_HelpBox.SetMessage($"The selected schema '{schema.name}' is not valid, the error is:\n" +
                                     editorResult.message);
                m_ValidateResultContainer.Add(m_HelpBox);
                return;
            }

            if (m_PanelManagerField.value == null) {
                m_HelpBox.SetMessage("A Display Panel Manager must be assigned to get started.");
                m_ValidateResultContainer.Add(m_HelpBox);
                return;
            }

            m_FullSetupButton.SetEnabled(true);
        }

        private void FullUISetup()
        {
            Debug.Log($"Full UI setup using schema {UIDesignerSchema}");

            var panelManager = m_PanelManagerField.value as DisplayPanelManager;
            var rectParent = panelManager.transform as RectTransform;

            var schemaInstance =
                UIDesignerManager.InstantiateSchemaPrefab<RectTransform>(UIDesignerSchema.FullSchemaLayout, rectParent);

            for (int i = 0; i < schemaInstance.transform.childCount; i++) {
                var child = schemaInstance.transform.GetChild(i);

                if (child.name == "Gameplay Panel") { panelManager.GameplayPanel = child.GetComponent<DisplayPanel>(); }

                if (child.name == "Main Menu") { panelManager.MainMenu = child.GetComponent<DisplayPanel>(); }
            }
        }

        void FetchSchemas()
        {
            var assetGUIDs = AssetDatabase.FindAssets("t:UIDesignerSchema", new[] { "Assets" });
            m_Schemas.Clear();
            for (var i = 0; i < assetGUIDs.Length; i++) {
                var guid = assetGUIDs[i];

                var path = AssetDatabase.GUIDToAssetPath(guid);
                var schema = AssetDatabase.LoadAssetAtPath<UIDesignerSchema>(path);
                m_Schemas.Add(schema);
            }
        }

        private void ValidateSchema()
        {
            //Refresh the manager to check validity.
            UIDesignerManager.Refresh();

            if (UIDesignerManager.UIDesignerValidationResult.isValid
            ) { Debug.Log("The UI Designer Schema is valid!"); } else {
                Debug.LogWarning("The UI Designer Schema is NOT valid, reasons below:\n" +
                                 UIDesignerManager.UIDesignerValidationResult.message);
            }
        }

        private void UpdateUIDesignerSchema(UIDesignerSchema schema)
        {
            UIDesignerManager.UIDesignerSchema = schema;
            if (m_SchemaPopupField.value != UIDesignerSchema) {
                // Add the UI Designer Option if it does not exist yet.
                if (m_Schemas.Contains(UIDesignerSchema) == false) { m_Schemas.Add(UIDesignerSchema); }

                m_SchemaPopupField.value = UIDesignerSchema;
            }

            UIDesignerManager.Refresh();
        }

        private void DuplicateSchema()
        {
            DuplicateSchema(UIDesignerSchema);
        }

        private void DuplicateSchema(UIDesignerSchema originalSchema)
        {
            if (originalSchema == null) {
                Debug.LogWarning("You must select a UI Designer Schema to duplicate in the object field.");
                return;
            }

            var path = EditorUtility.SaveFilePanel("Duplicate UI Designer Schema", "Assets", "UIDesignerSchema",
                "asset");
            if (path.Length != 0 && Application.dataPath.Length < path.Length) {
                path = string.Format("Assets/{0}", path.Substring(Application.dataPath.Length + 1));

                var newSchema = UIDesignerSchemaDuplicator.DuplicateSchema(originalSchema, path);

                UpdateUIDesignerSchema(newSchema);
            }
        }
    }

    public class UIDesignerSchemaDuplicator
    {
        public static UIDesignerSchema DuplicateSchema(UIDesignerSchema previousSchema, string path)
        {
            var duplicator = new UIDesignerSchemaDuplicator();
            return duplicator.Duplicate(previousSchema, path);
        }

        private UIDesignerSchema m_NewUIDesignerSchema;

        private UIDesignerSchema Duplicate(UIDesignerSchema sourceSchema, string newSchemaPath)
        {
            //Start by recompiling the scripts and refreshing the assets to make sure the assets are well imported.
            ForceCompilationAndAssetRefresh();

            var previousSchemaPath = AssetDatabase.GetAssetPath(sourceSchema);
            newSchemaPath = AssetDatabaseUtility.GetPathForNewDatabase(newSchemaPath, out var newFolderPath);

            Debug.Log($"Duplicating the UI Designer Schema to {newSchemaPath}.");

            var databaseResult = AssetDatabase.CopyAsset(previousSchemaPath, newSchemaPath);

            if (databaseResult == false) {
                Debug.LogWarning("The UI Designer Schema could not be duplicated.");
                return null;
            }

            ShowProgressBar(0.01f);

            m_NewUIDesignerSchema =
                (UIDesignerSchema)AssetDatabase.LoadAssetAtPath(newSchemaPath, typeof(UIDesignerSchema));

            //Duplicate all objects within the schema.
            var schemaSourceToNewObjectsDictionary = new Dictionary<Object, Object>();
            var schemaAssetsFolderPath = newFolderPath + "/UISchemaAssets/";

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                        BindingFlags.DeclaredOnly;
            var schemaFields = typeof(UIDesignerSchema).GetFields(flags);
            for (int i = 0; i < schemaFields.Length; i++) {
                var field = schemaFields[i];
                var fieldType = field.FieldType;

                if (fieldType != typeof(Object) && fieldType.IsSubclassOf(typeof(Object)) == false) { continue; }

                var sourceObj = field.GetValue(sourceSchema) as Object;

                if (ReferenceEquals(sourceObj, null)) {
                    Debug.LogWarning($"The object '{field.Name}' is missing from the source schema.");
                    continue;
                }

                if (sourceObj == null) {
                    Debug.LogError($"The object '{field.Name}' thinks it is null when it is not. IMPORTANT: The created schema will be broken. Delete it, recompile the scripts and try again.");
                    continue;
                }

                if (schemaSourceToNewObjectsDictionary.ContainsKey(sourceObj)) {
                    field.SetValue(m_NewUIDesignerSchema, schemaSourceToNewObjectsDictionary[sourceObj]);
                    continue;
                }

                var previousPath = AssetDatabase.GetAssetPath(sourceObj);

                var suffix = Path.GetExtension(previousPath);

                var newPath = schemaAssetsFolderPath + sourceObj.name + suffix;
                newPath = AssetDatabaseUtility.ValidatePath(newPath);
                ;

                if (newPath == null) {
                    Debug.LogWarning($"The {sourceObj.name} in the schema failed to duplicate.");
                    continue;
                }

                var result = AssetDatabase.CopyAsset(previousPath, newPath);

                if (result == false) {
                    Debug.LogWarning($"The {sourceObj.name} in the schema failed to duplicate.");
                    continue;
                }

                //Set the duplicate object to the new schema.
                var duplicateObj = (Object)AssetDatabase.LoadAssetAtPath(newPath, fieldType);
                field.SetValue(m_NewUIDesignerSchema, duplicateObj);
                schemaSourceToNewObjectsDictionary.Add(sourceObj, duplicateObj);
            }

            ShowProgressBar(0.21f);

            //Loop through the duplicate objects and replace any reference to the original schema objects
            foreach (var keyValuePair in schemaSourceToNewObjectsDictionary) {
                var source = keyValuePair.Key;
                var duplicate = keyValuePair.Value;

                if (duplicate == null) { continue; }

                var duplicateGameObject = duplicate as GameObject;
                if (duplicateGameObject == null) {
                    if (duplicate is Component component) { duplicateGameObject = component.gameObject; } else {
                        //Duplicate is not a prefab.
                        continue;
                    }
                }

                //Scan all the components in the prefab.
                var components = duplicateGameObject.GetComponentsInChildren<Component>(true);
                for (int i = 0; i < components.Length; i++) {
                    var component = components[i];

                    if (component == null) {
                        Debug.LogWarning($"The object '{duplicateGameObject}' has null components");
                        continue;
                    }

                    var componentFields = component.GetType().GetFields(flags);
                    foreach (var fieldInfo in componentFields) {
                        var fieldType = fieldInfo.FieldType;

                        if (fieldType != typeof(Object) && fieldType.IsSubclassOf(typeof(Object)) == false) {
                            continue;
                        }

                        var serializedObject = fieldInfo.GetValue(component) as Object;
                        if (serializedObject == null ||
                            schemaSourceToNewObjectsDictionary.TryGetValue(serializedObject, out var duplicateMatch) ==
                            false) { continue; }

                        //Found match, replace it.
                        fieldInfo.SetValue(component, duplicateMatch);
                    }
                }

                Shared.Editor.Utility.EditorUtility.SetDirty(duplicateGameObject);
            }

            // Save assets.
            m_NewUIDesignerSchema.m_KeepPrefabLink = false;
            m_NewUIDesignerSchema.m_Description = "A schema created by duplicating the Schema: " + sourceSchema.name;

            Shared.Editor.Utility.EditorUtility.SetDirty(m_NewUIDesignerSchema);
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();

            return m_NewUIDesignerSchema;
        }

        /// <summary>
        /// Show the progress bar.
        /// </summary>
        /// <param name="percentage">The percentage between 0 and 1.</param>
        private static void ShowProgressBar(float percentage)
        {
            EditorUtility.DisplayProgressBar("Duplicating the UI Designer Schema", "This process may take a while.",
                percentage);
        }

        /// <summary>
        /// Force all the assets and the scripts to be realoaded to avoid 
        /// </summary>
        private static void ForceCompilationAndAssetRefresh()
        {
            CompilationPipeline.RequestScriptCompilation();
            AssetDatabase.StartAssetEditing();

            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allAssetPaths.Length; i += 1) {
                MonoScript script = AssetDatabase.LoadAssetAtPath(allAssetPaths[i], typeof(MonoScript)) as MonoScript;
                if (script != null) {
                    AssetDatabase.ImportAsset(allAssetPaths[i]);
                    break;
                }
            }

            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
    }
}
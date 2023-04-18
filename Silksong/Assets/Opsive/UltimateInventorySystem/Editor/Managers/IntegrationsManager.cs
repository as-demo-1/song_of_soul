/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements;
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Draws the inspector for an integrations that has been installed.
    /// </summary>
    public abstract class IntegrationInspector
    {
        protected MainManagerWindow m_MainManagerWindow;
        public MainManagerWindow MainManagerWindow { set { m_MainManagerWindow = value; } }

        /// <summary>
        /// Draws the integration inspector.
        /// </summary>
        /// <param name="parent">The parent VisualElement.</param>
        public abstract void ShowInspector(VisualElement parent);
    }

    /// <summary>
    /// Draws a list of all of the available integrations.
    /// </summary>
    [OrderedEditorItem("Integrations", 100)]
    public class IntegrationsManager : Manager
    {
        private TabToolbar m_TabToolbar;
        private VisualElement m_IntegrationContent;

        private IntegrationInspector[] m_IntegrationInspectors;
        private string[] m_IntegrationNames;

        private UnityEngine.Networking.UnityWebRequest m_IntegrationsReqest;
        private AssetIntegration[] m_Integrations;

        /// <summary>
        /// Stores the information about the integration asset.
        /// </summary>
        private class AssetIntegration
        {
            private const int c_IconSize = 78;
            private const int c_ButtonSize = 100;

            private int m_ID;
            private string m_Name;
            private string m_IntegrationURL;
            private Texture2D m_Icon;
            private VisualElement m_Parent;
            private System.Action m_OnIconDownloaded;

            private UnityEngine.Networking.UnityWebRequest m_IconRequest;
            private UnityEngine.Networking.DownloadHandlerTexture m_TextureDownloadHandler;

            /// <summary>
            /// Constructor for the AssetIntegration class.
            /// </summary>
            public AssetIntegration(int id, string name, string iconURL, string integrationURL, VisualElement parent, Action onIconDownloaded)
            {
                m_ID = id;
                m_Name = name;
                m_IntegrationURL = integrationURL;
                m_OnIconDownloaded = onIconDownloaded;

                // Start loading the icon as soon as the url is retrieved.
                m_TextureDownloadHandler = new UnityEngine.Networking.DownloadHandlerTexture();
                m_IconRequest = UnityEngine.Networking.UnityWebRequest.Get(iconURL);
                m_IconRequest.downloadHandler = m_TextureDownloadHandler;
                m_IconRequest.SendWebRequest();

                parent.schedule.Execute(DownloadIcon).Every(10).Until(() => { return m_IconRequest == null || m_IconRequest.isDone; });
            }

            /// <summary>
            /// Updates the icon download request.
            /// </summary>
            private void DownloadIcon()
            {
                if (m_IconRequest != null) {
                    if (m_IconRequest.isDone) {
                        if (string.IsNullOrEmpty(m_IconRequest.error)) {
                            m_Icon = m_TextureDownloadHandler.texture;
                        }
                        m_IconRequest = null;
                        if (m_OnIconDownloaded != null) {
                            m_OnIconDownloaded();
                        }
                    }
                }
            }

            /// <summary>
            /// Draws the integration details at the specified position.
            /// </summary>
            public void ShowIntegration(VisualElement parent)
            {
                var horizontalLayout = new VisualElement();
                horizontalLayout.style.flexDirection = FlexDirection.Row;
                horizontalLayout.style.flexGrow = 1;
                parent.Add(horizontalLayout);

                // Draw the icon, name, and integration/Asset Store link.
                if (m_Icon != null) {
                    var image = new Image();
                    image.image = m_Icon;
                    image.style.width = image.style.height = c_IconSize;
                    image.style.alignSelf = Align.Center;
                    horizontalLayout.Add(image);
                }

                var verticalLayout = new VisualElement();
                verticalLayout.style.alignSelf = Align.Center;
                horizontalLayout.Add(verticalLayout);

                var label = new Label(m_Name);
                label.name = "integrations-title";
                verticalLayout.Add(label);

                if (!string.IsNullOrEmpty(m_IntegrationURL)) {
                    var button = new Button();
                    button.text = "Integration";
                    button.style.width = c_ButtonSize;
                    button.clicked += () =>
                    {
                        Application.OpenURL(m_IntegrationURL);
                    };
                    verticalLayout.Add(button);
                }

                if (m_ID > 0) {
                    var button = new Button();
                    button.text = "Asset Store";
                    button.style.width = c_ButtonSize;
                    button.clicked += () =>
                    {
                        Application.OpenURL("https://opsive.com/asset/UltimateInventorySystem/AssetRedirect.php?asset=" + m_ID);
                    };
                    verticalLayout.Add(button);
                }
            }
        }

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            m_TabToolbar = new TabToolbar(new string[]
            {
                "Integration Inspectors",
                "Available Integrations"
            }, 1, (int index) => UpdateContent());
            m_ManagerContentContainer.Add(m_TabToolbar);

            BuildInstalledIntegrations();

            m_IntegrationContent = new VisualElement();
            m_ManagerContentContainer.Add(m_IntegrationContent);

            // Start downloading the available integrations.
            m_IntegrationsReqest = UnityEngine.Networking.UnityWebRequest.Get("https://opsive.com/asset/UltimateInventorySystem/IntegrationsList.txt");
            m_IntegrationsReqest.SendWebRequest();
            m_IntegrationContent.schedule.Execute(DownloadIntegrations).Every(10).Until(() => { return m_IntegrationsReqest == null || m_IntegrationsReqest.isDone; });

            UpdateContent();
        }

        /// <summary>
        /// Finds and create an instance of the inspectors for all of the installed integrations.
        /// </summary>
        private void BuildInstalledIntegrations()
        {
            var integrationInspectors = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var integrationIndexes = new List<int>();
            for (int i = 0; i < assemblies.Length; ++i) {
                var assemblyTypes = assemblies[i].GetTypes();
                for (int j = 0; j < assemblyTypes.Length; ++j) {
                    // Must implement IntegrationInspector.
                    if (!typeof(IntegrationInspector).IsAssignableFrom(assemblyTypes[j])) {
                        continue;
                    }

                    // Ignore abstract classes.
                    if (assemblyTypes[j].IsAbstract) {
                        continue;
                    }

                    // A valid inspector class.
                    integrationInspectors.Add(assemblyTypes[j]);
                    var index = integrationIndexes.Count;
                    if (assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                        var item = assemblyTypes[j].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                        index = item.Index;
                    }
                    integrationIndexes.Add(index);
                }
            }

            // Do not reinitialize the inspectors if they are already initialized and there aren't any changes.
            if (m_IntegrationInspectors != null && m_IntegrationInspectors.Length == integrationInspectors.Count) {
                return;
            }

            // All of the manager types have been found. Sort by the index.
            var inspectorTypes = integrationInspectors.ToArray();
            Array.Sort(integrationIndexes.ToArray(), inspectorTypes);

            m_IntegrationInspectors = new IntegrationInspector[integrationInspectors.Count];
            m_IntegrationNames = new string[integrationInspectors.Count];

            // The inspector types have been found and sorted. Add them to the list.
            for (int i = 0; i < inspectorTypes.Length; ++i) {
                m_IntegrationInspectors[i] = Activator.CreateInstance(inspectorTypes[i]) as IntegrationInspector;
                m_IntegrationInspectors[i].MainManagerWindow = m_MainManagerWindow;

                var name = Shared.Editor.Utility.EditorUtility.SplitCamelCase(inspectorTypes[i].Name);
                if (integrationInspectors[i].GetCustomAttributes(typeof(OrderedEditorItem), true).Length > 0) {
                    var item = inspectorTypes[i].GetCustomAttributes(typeof(OrderedEditorItem), true)[0] as OrderedEditorItem;
                    name = item.Name;
                }
                m_IntegrationNames[i] = name;
            }
        }

        /// <summary>
        /// Downloads the list of available integrations.
        /// </summary>
        private void DownloadIntegrations()
        {
            if (m_IntegrationsReqest == null || !m_IntegrationsReqest.isDone) {
                return;
            }

            // The integration list has finished downloading.
            if (string.IsNullOrEmpty(m_IntegrationsReqest.error)) {
                var splitIntegrations = m_IntegrationsReqest.downloadHandler.text.Split('\n');
                m_Integrations = new AssetIntegration[splitIntegrations.Length];
                var count = 0;
                for (int i = 0; i < splitIntegrations.Length; ++i) {
                    if (string.IsNullOrEmpty(splitIntegrations[i])) {
                        continue;
                    }

                    // The data must contain info on the integration name, id, icon, and integraiton url.
                    var integrationData = splitIntegrations[i].Split(',');
                    if (integrationData.Length < 4) {
                        continue;
                    }

                    m_Integrations[count] = new AssetIntegration(int.Parse(integrationData[0].Trim()), integrationData[1].Trim(), integrationData[2].Trim(), integrationData[3].Trim(),
                                                                    m_IntegrationContent, UpdateContent);
                    count++;
                }

                if (count != m_Integrations.Length) {
                    Array.Resize(ref m_Integrations, count);
                }
                m_IntegrationsReqest = null;
            }
            m_IntegrationsReqest = null;
            UpdateContent();
        }

        /// <summary>
        /// Updates the shown content.
        /// </summary>
        private void UpdateContent()
        {
            m_IntegrationContent.Clear();

            if (m_TabToolbar.Selected == 0) { // Installed Integrations.
                ShowInstalledIntegrations();
            } else { // Available Integrations.
                ShowAvailableIntegrations();
            }
        }

        /// <summary>
        /// Shows all of the installed integrations.
        /// </summary>
        private void ShowInstalledIntegrations()
        {
            if (m_IntegrationInspectors == null || m_IntegrationInspectors.Length == 0) {
                m_IntegrationContent.Add(new Label("No integrations installed use a custom inspector.\n\n" +
                                                   "Select the \"Available Integrations\" tab to see a list of all of the available integrations."));
                return;
            }

            // Allow each integration to setup their own UIElements.
            var scrollView = new ScrollView();
            for (int i = 0; i < m_IntegrationInspectors.Length; ++i) {
                var integrationBox = new VisualElement();
                integrationBox.AddToClassList(InventoryManagerStyles.SubMenu);
                integrationBox.style.marginTop = 20f;
                var titleLabel = new Label(m_IntegrationNames[i]);
                titleLabel.AddToClassList(InventoryManagerStyles.SubMenuTitle);
                integrationBox.Add(titleLabel);
                m_IntegrationInspectors[i].ShowInspector(integrationBox);
                scrollView.Add(integrationBox);
            }
            m_IntegrationContent.Add(scrollView);
        }

        /// <summary>
        /// Shows all of the installed integrations.
        /// </summary>
        private void ShowAvailableIntegrations()
        {
            if (m_Integrations == null || m_Integrations.Length == 0) {
                SetupManager.CreateMenuBox("Integrations", "Retrieving the integrations. The integrations can also be retrieved from the button below.", null, "Integrations", () =>
                {
                    Application.OpenURL("https://opsive.com/downloads/?pid=22330");
                }, m_IntegrationContent, true);
                return;
            }

            var scrollView = new ScrollView();
            var integrationContent = new VisualElement();
            integrationContent.name = "integrations-container";
            scrollView.contentContainer.Add(integrationContent);
            m_IntegrationContent.Add(scrollView);

            for (int i = 0; i < m_Integrations.Length; ++i) {
                var integrationBox = new VisualElement();
                integrationBox.name = "integrations-box";
                m_Integrations[i].ShowIntegration(integrationBox);
                integrationContent.Add(integrationBox);
            }
        }
    }
    
    public abstract class IntegrationControlBox : VisualElement
    {
        public virtual string DocumentationURL => null;
        public abstract string Title { get; }
        public abstract string Description { get; }

        public IntegrationControlBox()
        {
            AddToClassList(InventoryManagerStyles.SubMenu);
            style.marginTop = 20f;

            var topHorizontalLayout = new VisualElement();
            topHorizontalLayout.AddToClassList(InventoryManagerStyles.SubMenuTop);
            Add(topHorizontalLayout);

            var titleLabel = new Label(Title);
            titleLabel.AddToClassList(InventoryManagerStyles.SubMenuTitle);
            topHorizontalLayout.Add(titleLabel);

            var descriptionLabel = new Label();
            descriptionLabel.AddToClassList(InventoryManagerStyles.SubMenuIconDescription);
            descriptionLabel.text = Description;
            Add(descriptionLabel);
        }

        public static Button CreateButton(string text, Action action)
        {
            var button = new Button();
            button.AddToClassList(InventoryManagerStyles.SubMenuButton);
            button.style.marginTop = 4;
            button.text = text;
            button.clicked += action;

            return button;
        }
    }
}
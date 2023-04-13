/// ---------------------------------------------
/// Ultimate Inventory System
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.UltimateInventorySystem.Editor.Managers
{
    using Opsive.Shared.Editor.UIElements.Managers;
    using Opsive.UltimateInventorySystem.Editor.Styles;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a starting window with useful links.
    /// </summary>
    [OrderedEditorItem("Welcome", 0)]
    public class WelcomeScreenManager : Manager
    {
        private const string c_DocumentationTextureGUID = "4ed35e016803fb7408050e41a75f1d93";
        private const string c_VideosTextureGUID = "0697e4652b4c4a040a26a775a24847d1";
        private const string c_IntegrationsTextureGUID = "753cf93668e9fc9449c32306d3136a19";
        private const string c_ForumTextureGUID = "231c482ec921d5d469d0039bc18dbfda";
        private const string c_DiscordTextureGUID = "c234ca342a2cb274c94da04b6db74730";
        private const string c_ReviewTextureGUID = "8af2dbe9f9221bf4882aee4d8375f7d1";
        private const string c_ShowcaseTextureGUID = "7723bcc8e25ee3443a9954c35ac939ec";

        /// <summary>
        /// Adds the visual elements to the ManagerContentContainer visual element. 
        /// </summary>
        public override void BuildVisualElements()
        {
            var centeredContent = new VisualElement();
            centeredContent.style.alignSelf = Align.Center;
            centeredContent.style.flexGrow = 1;

            var welcomeLabel = new Label();
            welcomeLabel.text = "Thank you for purchasing the Ultimate Inventory System.\nThe resources below will help you get the most out of the system.";
            welcomeLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            centeredContent.Add(welcomeLabel);

            // Documentation.
            var imageElement = new Image();
            imageElement.AddToClassList(InventoryManagerStyles.LinkCursor);
            imageElement.style.marginTop = 10f;
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(c_DocumentationTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL("https://opsive.com/support/documentation/ultimate-inventory-system/");
            });
            centeredContent.Add(imageElement);

            // Videos and Integrations.
            AddHorizontalImages(centeredContent, c_VideosTextureGUID, "https://opsive.com/videos/?pid=22330",
                                c_IntegrationsTextureGUID, "https://opsive.com/downloads/?pid=22330");
            // Forum and Discord.
            AddHorizontalImages(centeredContent, c_ForumTextureGUID, "https://opsive.com/forum/",
                                c_DiscordTextureGUID, "https://discord.gg/QX6VFgc");
            // Review and Showcase.
            AddHorizontalImages(centeredContent, c_ReviewTextureGUID, "https://assetstore.unity.com/packages/slug/166053",
                                c_ShowcaseTextureGUID, "https://opsive.com/showcase/");
            m_ManagerContentContainer.Add(centeredContent);

            // Version number at the bottom.
            var version = new Label();
            version.text = string.Format("Ultimate Inventory System version {0}.", Utility.AssetInfo.Version);
            version.style.paddingLeft = 2;
            version.style.paddingBottom = 2;
            m_ManagerContentContainer.Add(version);
        }

        /// <summary>
        /// Adds two images stacked beside each other.
        /// </summary>
        /// <param name="parent">The VisualElement that the content should be added to.</param>
        /// <param name="leftTextureGUID">The GUID for the left image.</param>
        /// <param name="leftURL">The URL for the left image.</param>
        /// <param name="rightTextureGUID">The GUID for the right image.</param>
        /// <param name="rightURL">The URL for the right image.</param>
        private void AddHorizontalImages(VisualElement parent, string leftTextureGUID, string leftURL, string rightTextureGUID, string rightURL)
        {
            var horizontalLayout = new VisualElement();
            horizontalLayout.AddToClassList(CommonStyles.s_HorizontalAlignCenter);
            horizontalLayout.style.marginTop = 2;
            var imageElement = new Image();
            imageElement.AddToClassList(InventoryManagerStyles.LinkCursor);
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(leftTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL(leftURL);
            });
            horizontalLayout.Add(imageElement);

            imageElement = new Image();
            imageElement.style.marginLeft = 2;
            imageElement.AddToClassList(InventoryManagerStyles.LinkCursor);
            imageElement.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture2D>(rightTextureGUID);
            imageElement.RegisterCallback<MouseDownEvent>(c =>
            {
                Application.OpenURL(rightURL);
            });
            horizontalLayout.Add(imageElement);

            parent.Add(horizontalLayout);
        }
    }
}
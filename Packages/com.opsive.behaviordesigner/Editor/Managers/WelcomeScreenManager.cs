/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.Shared.Editor.Managers;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Shows a starting window with useful links.
    /// </summary>
    [OrderedEditorItem("Welcome", 0)]
    public class WelcomeScreenManager : Opsive.Shared.Editor.Managers.WelcomeScreenManager
    {
        private const string c_GraphDesignerSymbol = "GRAPH_DESIGNER";
        private const string c_EditorTextureGUID = "d52eae9187aad5b41aff6dd60e49247a";
        private const string c_RepositoryTextureGUID = "020153b61b65cf34f82ccc9b186cb700";

        private VisualElement m_EntityHelpBoxContainer;

        /// <summary>
        /// The name of the asset.
        /// </summary>
        protected override string AssetName => AssetInfo.Name;

        /// <summary>
        /// The version of the asset.
        /// </summary>
        protected override string AssetVersion => AssetInfo.Version;

        /// <summary>
        /// Should the large documentation image be added?
        /// </summary>
        protected override bool AddLargeDocumentationImage => false;

        /// <summary>
        /// Returns the URL for the documentation page.
        /// </summary>
        /// <returns>The URL for the documentation page.</returns>
        protected override string GetDocumentationURL()
        {
            return "https://opsive.com/support/documentation/behavior-designer-pro/";
        }

        /// <summary>
        /// Returns the URL for the videos page.
        /// </summary>
        /// <returns>The URL for the videos page.</returns>
        protected override string GetVideosURL()
        {
            return "https://opsive.com/videos?pid=28276";
        }

        /// <summary>
        /// Returns the URL for the asset page.
        /// </summary>
        /// <returns>The URL for the asset page.</returns>
        protected override string GetAssetURL()
        {
            return "https://assetstore.unity.com/packages/slug/298743";
        }

        /// <summary>
        /// Checks to ensure the required packages are installed.
        /// </summary>
        /// <param name="parent">The parent VisualElement.</param>
        protected override void AddHeader(VisualElement parent)
        {
            EditorApplication.update += CheckForEntities;

#if !UNITY_ENTITIES
            m_EntityHelpBoxContainer = ManagerUtility.AddHelpBox(parent, "Behavior Designer requires the Entities package. Press the button below to install.", HelpBoxMessageType.Error, "Install", (HelpBox helpbox, Button actionButton) =>
            {
                helpbox.text = "Installing the entities package. Unity will reimport after the package has been installed.\n\n" +
                                     "Restart Unity if you receive compiler errors after Unity has reimported.\n\n" +
                                     "Behavior Designer can be access from the Tools/Opsive/Behavior Designer/Editor menu.";
                helpbox.messageType = HelpBoxMessageType.Info;
                actionButton.SetEnabled(false);
                m_WelcomeLabel.style.display = DisplayStyle.None;

                UnityEditor.PackageManager.Client.Add("com.unity.entities");
            });
            m_WelcomeLabel.style.display = DisplayStyle.None;
#else
            base.AddHeader(parent);
#endif
        }

        /// <summary>
        /// Checks for the Entity package installation.
        /// </summary>
        private void CheckForEntities()
        {
#if !GRAPH_DESIGNER && UNITY_ENTITIES
            AddSymbol(c_GraphDesignerSymbol);
            EditorApplication.update -= CheckForEntities;

            if (m_EntityHelpBoxContainer != null) {
                m_EntityHelpBoxContainer.style.display = DisplayStyle.None;
                m_WelcomeLabel.style.display = DisplayStyle.Flex;
            }
#elif GRAPH_DESIGNER
            if (m_EntityHelpBoxContainer != null) {
                m_EntityHelpBoxContainer.style.display = DisplayStyle.None;
                m_WelcomeLabel.style.display = DisplayStyle.Flex;
            }
#endif
        }

        /// <summary>
        /// Adds the specified symbol to the compiler definitions.
        /// </summary>
        /// <param name="symbol">The symbol to add.</param>
        private static void AddSymbol(string symbol)
        {
#if UNITY_2023_1_OR_NEWER
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup);
            var symbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
#else
            var symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            if (symbols.Contains(symbol)) {
                return;
            }
            symbols += (";" + symbol);
#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, symbols);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
#endif
        }

        /// <summary>
        /// Adds the resource images to the parent element.
        /// </summary>
        /// <param name="parent">The parent that the images should be added to.</param>
        protected override void AddImages(VisualElement parent)
        {
            AddLargeImage(parent, c_EditorTextureGUID, () => {
#if GRAPH_DESIGNER
                BehaviorDesignerWindow.ShowWindow();
#else
                UnityEngine.Debug.LogError("Error: Unable to open the Behavior Designer window. Ensure the Entities package has been installed.");
#endif
            });

            // Documentation and Videos.
            AddImageRow(parent, c_SmallDocumentationTextureGUID, GetDocumentationURL(), 
                                c_VideosTextureGUID, GetVideosURL());

            // Repository and Downloads.
            AddImageRow(parent, c_RepositoryTextureGUID, "https://opsive.com/assets/behavior-designer-pro-subscription",
                                c_DownloadsTextureGUID, IntegrationsManager.GetDownloadsLink());

            // Forum and Discord.
            AddImageRow(parent, c_ForumTextureGUID, "https://opsive.com/forum/",
                                c_DiscordTextureGUID, "https://discord.gg/QX6VFgc");
            // Review and Showcase.
            AddImageRow(parent, c_ReviewTextureGUID, GetAssetURL(),
                                c_ShowcaseTextureGUID, "https://opsive.com/showcase/");
            // Professional Services.
            AddImageRow(parent, c_ProfessionalServicesTextureGUID, "https://opsive.com", string.Empty, string.Empty);
        }
    }
}
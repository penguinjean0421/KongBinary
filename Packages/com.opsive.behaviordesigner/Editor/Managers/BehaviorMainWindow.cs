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

    /// <summary>
    /// The MainManagerWindow is an editor window which contains all of the sub managers. This window draws the high level menu options and draws
    /// the selected sub manager.
    /// </summary>
    [InitializeOnLoad]
    public class BehaviorMainWindow : MainManagerWindow
    {
        protected override string AssetName => AssetInfo.Name;
        protected override string AssetVersion => AssetInfo.Version;
        protected override string UpdateCheckURL => string.Format("https://opsive.com/asset/UpdateCheck.php?asset=BehaviorDesigner&type={0}&version={1}&unityversion={2}&devplatform={3}&targetplatform={4}",
                                            AssetInfo.Name.Replace(" ", ""), AssetInfo.Version, Application.unityVersion, Application.platform, EditorUserBuildSettings.activeBuildTarget);
        protected override string LatestVersionKey => "Opsive.BehaviorDesigner.Editor.LatestVersion";
        protected override string LastUpdateCheckKey => "Opsive.BehaviorDesigner.Editor.LastUpdateCheck";
        protected override string ManagerNamespace => "Opsive.BehaviorDesigner.Editor";

        /// <summary>
        /// Initializes the Main Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Behavior Designer/Welcome", false, 30)]
        public static MainManagerWindow ShowWindow()
        {
            var window = EditorWindow.GetWindow<BehaviorMainWindow>(false, "Behavior Window");
            window.minSize = new Vector2(680, 670);
            return window;
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Samples Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Behavior Designer/Samples", false, 31)]
        public static void ShowSamplesManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(SamplesManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Integrations Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Behavior Designer/Integrations", false, 32)]
        public static void ShowIntegrationsManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(IntegrationsManager));
        }

        /// <summary>
        /// Initializes the Main Manager and shows the Add-Ons Manager.
        /// </summary> 
        [MenuItem("Tools/Opsive/Behavior Designer/Add-Ons", false, 33)]
        public static void ShowAddOnsManagerWindow()
        {
            var window = ShowWindow();
            window.Open(typeof(AddOnsManager));
        }
    }
}
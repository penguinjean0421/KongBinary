#if !OPSIVE_IMPORT_DEBUG
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.Shared.Editor.Import;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Shows the welcome screen and check for minimum installed packages.
    /// </summary>
    [InitializeOnLoad]
    public class Startup
    {
        private const string c_ImportStatusPath = "Assets/Opsive/ImportStatus.asset";

        /// <summary>
        /// Perform editor checks as soon as the scripts are done compiling.
        /// </summary>
        static Startup()
        {
            EditorApplication.update += EditorStartup;
        }

        /// <summary>
        /// Show the editor window if it hasn't been shown before.
        /// </summary>
        private static void EditorStartup()
        {
            if (EditorApplication.isCompiling) {
                return;
            }

            EditorApplication.update -= EditorStartup;
            AssetDatabase.Refresh();
            ImportStatus importStatus = null;
            var importStatusAssets = AssetDatabase.FindAssets("t:ImportStatus");
            if (importStatusAssets != null && importStatusAssets.Length > 0) {
                for (int i = 0; i < importStatusAssets.Length; ++i) {
                    var path = AssetDatabase.GUIDToAssetPath(importStatusAssets[i]);
                    if (string.IsNullOrEmpty(path)) {
                        path = importStatusAssets[i];
                    }
                    importStatus = AssetDatabase.LoadAssetAtPath(path, typeof(ImportStatus)) as ImportStatus;
                    if (importStatus != null) {
                        break;
                    }
                }
            }
            if (importStatus == null) {
                // The import status hasn't been created yet. Create it in the same location as the Opsive folder.
                importStatus = ScriptableObject.CreateInstance<ImportStatus>();
                Directory.CreateDirectory(Path.GetDirectoryName(c_ImportStatusPath));
                AssetDatabase.CreateAsset(importStatus, c_ImportStatusPath);
                AssetDatabase.Refresh();
            }

            if (!importStatus.BehaviorWindowShown) {
                var window = BehaviorMainWindow.ShowWindow();
                window.Open(typeof(WelcomeScreenManager));
                importStatus.BehaviorWindowShown = true;
                EditorUtility.SetDirty(importStatus);
            }
        }
    }
}
#endif
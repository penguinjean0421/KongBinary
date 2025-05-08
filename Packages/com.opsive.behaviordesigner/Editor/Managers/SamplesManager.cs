/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.Shared.Editor.Managers;

    /// <summary>
    /// Draws a list of all of the available samples.
    /// </summary>
    [OrderedEditorItem("Samples", 10)]
    public class SamplesManager :Manager
    {
        public override void BuildVisualElements()
        {
            ManagerUtility.ShowControlBox("Import Samples", "Imports the sample scenes. These scenes use the universal render pipeline.", null, "Import Samples", () =>
            {
                ManagerUtility.ImportSample(AssetInfo.PackageName);
            }, m_ManagerContentContainer, true);
        }
    }
}
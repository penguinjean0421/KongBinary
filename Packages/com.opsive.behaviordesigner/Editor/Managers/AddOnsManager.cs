/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.Shared.Editor.Managers;

    /// <summary>
    /// Draws a list of all of the available add-ons.
    /// </summary>
    [OrderedEditorItem("Add-Ons", 12)]
    public class AddOnsManager : Opsive.Shared.Editor.Managers.AddOnsManager
    {
        protected override string AddOnsURL => "https://opsive.com/asset/BehaviorDesigner/AddOnsList.txt";
    }
}
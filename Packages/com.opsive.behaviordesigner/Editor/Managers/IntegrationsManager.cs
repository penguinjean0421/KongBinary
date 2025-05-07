/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Managers
{
    using Opsive.Shared.Editor.Managers;

    /// <summary>
    /// Draws a list of all of the available integrations.
    /// </summary>
    [OrderedEditorItem("Integrations", 11)]
    public class IntegrationsManager : Opsive.Shared.Editor.Managers.IntegrationsManager
    {
        protected override string IntegrationsURL => "https://opsive.com/asset/BehaviorDesigner/IntegrationsList.txt";
    }
}
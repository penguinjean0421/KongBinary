#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.GraphDesigner.Runtime;

    /// <summary>
    /// The StackedAction task allows for multiple actions to be added to the same node.
    /// </summary>
    [NodeIcon("dacf20a036b1f5e41886d84ac4a47779", "2df1cb3efc025214cbab4df573bb3515")]
    [NodeDescription("Allows multiple action tasks to be added to a single node.")]
    public class StackedAction : StackedTask, IAction
    {
    }
}
#endif
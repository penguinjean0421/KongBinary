#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Groups
{
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Unity.Entities;

    /// <summary>
    /// Grouping for the systems that should before other systems.
    /// </summary>
    [UpdateInGroup(typeof(BehaviorTreeSystemGroup), OrderFirst = true)]
    public partial class BeforeTraversalSystemGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Grouping for the task systems that should reevaluate.
    /// </summary>
    [UpdateInGroup(typeof(BeforeTraversalSystemGroup))]
    public partial class ReevaluateTaskSystemGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Grouping for the systems that run before the tree execution.
    /// </summary>
    [UpdateInGroup(typeof(BehaviorTreeSystemGroup))]
    public partial class InterruptSystemGroup : ComponentSystemGroup
    {
    }

    /// <summary>
    /// Grouping for the task systems that can cause interrupts.
    /// </summary>
    [UpdateInGroup(typeof(InterruptSystemGroup))]
    [UpdateAfter(typeof(InterruptSystem))]
    [UpdateBefore(typeof(InterruptCleanupSystem))]
    public partial class InterruptTaskSystemGroup : ComponentSystemGroup
    {
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime;
    using UnityEngine;

    /// <summary>
    /// A TaskObject implementation of the Conditional task. This class can be used when the task should not be grouped by the StackedConditional task.
    /// </summary>
    [NodeIcon("dea5c23eac9d12c4cbd380cc879816ea", "2963cf3eb0c036449829254b2074c4c3")]
    public abstract class ConditionalNode : Task, ILogicNode, IConditional, IConditionalReevaluation
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        /// <summary>
        /// Reevaluates the task logic. Returns a TaskStatus indicating how the behavior tree flow should proceed.
        /// </summary>
        /// <returns>The status of the task during the reevaluation phase.</returns>
        public virtual TaskStatus OnReevaluateUpdate() { return OnUpdate(); }
    }
}
#endif
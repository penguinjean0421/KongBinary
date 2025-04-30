#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Opsive.GraphDesigner.Runtime;
    using UnityEngine;

    /// <summary>
    /// A TaskObject implementation of the Decorator task.
    /// </summary>
    [NodeIcon("9abc6c99a8db43b49b2b0d48cca90105", "1ee7d6a0873e3d942b556d3093d8173f")]
    public abstract class DecoratorNode : Task, ILogicNode, IParentNode, IDecorator, ITaskObjectParentNode
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

        public int MaxChildCount { get => 1; }
        public ushort NextChildIndex { get => (ushort)(RuntimeIndex + 1); }
    }
}
#endif
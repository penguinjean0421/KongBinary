#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Events
{
    using Opsive.GraphDesigner.Runtime;
    using UnityEngine;

    /// <summary>
    /// A base IEventNode implementation.
    /// </summary>
    [NodeIcon("9041375773f69454792084ab67820b7e", "b1382ad24c668174c9a6e0bd00f229e3")]
    public abstract class EventNode : IEventNode, IEventNodeGameObjectReceiver
    {
        [Tooltip("The index of the ILogicNode that the IEventNode is connected to. ushort.MaxValue indicates no connection.")]
        [SerializeField] protected ushort m_ConnectedIndex;

        public ushort ConnectedIndex { get => m_ConnectedIndex; set => m_ConnectedIndex = value; }

        protected BehaviorTree m_BehaviorTree;
            
        /// <summary>
        /// Initializes the node to the specified graph.
        /// </summary>
        /// <param name="graph">The graph that is initializing the task.</param>
        public virtual void Initialize(IGraph graph)
        {
            m_BehaviorTree = graph as BehaviorTree;
        }
    }
}
#endif
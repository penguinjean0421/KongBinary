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
    /// The EventNode that is invoked when the behavior tree starts.
    /// </summary>
    [NodeIcon("8c35407905159694e8b83df15d3b039b", "df820d6e71423194188c7dcb1c1ae2e2")]
    public class Start : IEventNode
    {
        [Tooltip("The index of the ILogicNode that the IEventNode is connected to. ushort.MaxValue indicates no connection.")]
        [SerializeField] protected ushort m_ConnectedIndex;

        public ushort ConnectedIndex { get => m_ConnectedIndex; set => m_ConnectedIndex = value; }
    }
}
#endif
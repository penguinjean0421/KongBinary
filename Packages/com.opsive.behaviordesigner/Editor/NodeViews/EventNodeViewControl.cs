#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Controls.NodeViews
{
    using Opsive.Shared.Editor.UIElements.Controls;
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.GraphDesigner.Editor;
    using Opsive.GraphDesigner.Editor.Controls.NodeViews;
    using Opsive.GraphDesigner.Editor.Elements;
    using Opsive.GraphDesigner.Editor.Events;
    using Opsive.GraphDesigner.Runtime;
    using Unity.Entities;
    using UnityEngine.UIElements;
    using UnityEngine;
    using UnityEditor;

    /// <summary>
    /// Adds UI elements within the event node.
    /// </summary>
    [ControlType(typeof(IEventNode))]
    public class EventNodeViewControl : NodeViewBase
    {
        private const string c_DarkSuccessIconGUID = "240eed9b6e6dc004f94216f1e9fcc390";
        private const string c_LightSuccessIconGUID = "cf3f27e8ca1f20f4680890e078c7613a";
        private const string c_DarkFailureIconGUID = "8d159db7a8da43e41a50a77e43cfd6ba";
        private const string c_LightFailureIconGUID = "c3622912d9f7bcd41a54a95add672423";

        private IEventNode m_Node;
        private GraphWindow m_GraphWindow;
        private BehaviorTree m_BehaviorTree;
        private EventNode m_EventNode;

        private Image m_ExecutionStatusIcon;
        private Texture m_SuccessIcon;
        private Texture m_FailureIcon;

        /// <summary>
        /// Addes the UIElements for the specified runtime node to the editor Node within the graph.
        /// </summary>
        /// <param name="graphWindow">A reference to the GraphWindow.</param>
        /// <param name="parent">The parent UIElement that should contain the node UIElements.</param>
        /// <param name="node">The node that the control represents.</param>
        public override void AddNodeView(GraphWindow graphWindow, VisualElement parent, object node)
        {
            m_Node = node as IEventNode;
            m_GraphWindow = graphWindow;
            m_BehaviorTree = m_GraphWindow.Graph as BehaviorTree;
            m_EventNode = parent.GetFirstAncestorOfType<EventNode>();

            // AddNodeView can be called multiple times. Ensure there is only one execution status image.
            var previousExecutionStatus = m_EventNode.Q("event-execution-status");
            if (previousExecutionStatus != null) {
                previousExecutionStatus.parent.Remove(previousExecutionStatus);
            }
            m_ExecutionStatusIcon = new Image();
            m_ExecutionStatusIcon.name = "event-execution-status";
            parent.parent.Add(m_ExecutionStatusIcon); // The execution status icon should be placed behind every node element.
            m_ExecutionStatusIcon.SendToBack();

            m_SuccessIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkSuccessIconGUID : c_LightSuccessIconGUID);
            m_FailureIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkFailureIconGUID : c_LightFailureIconGUID);

            m_ExecutionStatusIcon.RegisterCallback<AttachToPanelEvent>(c =>
            {
                GraphEventHandler.RegisterEvent(GraphEventType.WindowUpdate, UpdateNode);
            });
            m_ExecutionStatusIcon.RegisterCallback<DetachFromPanelEvent>(c =>
            {
                GraphEventHandler.UnregisterEvent(GraphEventType.WindowUpdate, UpdateNode);
            });
        }

        /// <summary>
        /// Updates the node with the current execution status icon.
        /// </summary>
        private void UpdateNode()
        {
            if (m_BehaviorTree == null || m_BehaviorTree.Entity == Entity.Null || m_Node.ConnectedIndex == ushort.MaxValue || !m_BehaviorTree.World.EntityManager.Exists(m_BehaviorTree.Entity)) {
                return;
            }

            var connectedNode = m_GraphWindow.Graph.LogicNodes[m_Node.ConnectedIndex];
            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity);
            var taskComponent = taskComponents[connectedNode.RuntimeIndex];
            if (taskComponent.Status == TaskStatus.Success) {
                m_ExecutionStatusIcon.image = m_SuccessIcon;
            } else if (taskComponent.Status == TaskStatus.Failure) {
                m_ExecutionStatusIcon.image = m_FailureIcon;
            } else if (m_ExecutionStatusIcon.image != null) {
                m_ExecutionStatusIcon.image = null;
            }

            if (m_ExecutionStatusIcon.image != null) {
                m_ExecutionStatusIcon.style.width = m_ExecutionStatusIcon.image.width;
            }

            if (taskComponent.Status == TaskStatus.Running || taskComponent.Status == TaskStatus.Queued) {
                m_EventNode.SetColorState(ColorState.Active, 0);
            } else {
                var nodeIndex = m_GraphWindow.GraphEditor.GetNodeIndex(m_Node);
                m_EventNode.SetColorState(m_GraphWindow.Graph.IsNodeEnabled(false, nodeIndex) ? ColorState.Default : ColorState.Disabled, 0.5f);
            }
        }
    }
}
#endif
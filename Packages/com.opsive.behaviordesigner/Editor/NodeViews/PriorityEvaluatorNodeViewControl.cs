#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Controls.NodeViews
{
    using Opsive.GraphDesigner.Editor;
    using Opsive.GraphDesigner.Editor.Events;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Decorators;
    using Opsive.Shared.Editor.UIElements.Controls;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UIElements;

    /// <summary>
    /// Implements TypeControlBase for the PriorityEvaluator type.
    /// </summary>
    [ControlType(typeof(PriorityEvaluator))]
    public class PriorityEvaluatorNodeViewControl : TaskNodeViewControl
    {
        private BehaviorTree m_BehaviorTree;
        private ILogicNode m_Node;
        private ushort m_PriorityEvaluatorComponentIndex = ushort.MaxValue;

        private Label m_PriorityValueLabel;

        /// <summary>
        /// Addes the UIElements for the specified runtime node to the editor Node within the graph.
        /// </summary>
        /// <param name="graphWindow">A reference to the GraphWindow.</param>
        /// <param name="parent">The parent UIElement that should contain the node UIElements.</param>
        /// <param name="node">The node that the control represents.</param>
        public override void AddNodeView(GraphWindow graphWindow, VisualElement parent, object node)
        {
            base.AddNodeView(graphWindow, parent, node);

            if (!Application.isPlaying) {
                return;
            }

            m_BehaviorTree = graphWindow.Graph as BehaviorTree;
            m_Node = node as ILogicNode;

            parent.RegisterCallback<AttachToPanelEvent>(c =>
            {
                GraphEventHandler.RegisterEvent(GraphEventType.WindowUpdate, UpdateUtilityValue);
            });
            parent.RegisterCallback<DetachFromPanelEvent>(c =>
            {
                GraphEventHandler.UnregisterEvent(GraphEventType.WindowUpdate, UpdateUtilityValue);
            });

            m_PriorityValueLabel = new Label();
            m_PriorityValueLabel.style.alignSelf = Align.Center;
            parent.Add(m_PriorityValueLabel);
        }

        /// <summary>
        /// Updates the utility value.
        /// </summary>
        private void UpdateUtilityValue()
        {
            if (m_BehaviorTree == null || m_BehaviorTree.Entity == Entity.Null || m_Node.RuntimeIndex == ushort.MaxValue) {
                return;
            }

            var taskObjectComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskObjectComponent>(m_BehaviorTree.Entity);
            if (m_PriorityEvaluatorComponentIndex == ushort.MaxValue) {
                // Find the corresponding index of the TaskObject.
                for (int i = 0; i < taskObjectComponents.Length; ++i) {
                    if (taskObjectComponents[i].Index == m_Node.RuntimeIndex) {
                        m_PriorityEvaluatorComponentIndex = (ushort)i;
                        break;
                    }
                }

                if (m_PriorityEvaluatorComponentIndex == ushort.MaxValue) {
                    return;
                }
            }

            var priorityEvaluator = m_BehaviorTree.GetTask(taskObjectComponents[m_PriorityEvaluatorComponentIndex].Index) as PriorityEvaluator;
            if (priorityEvaluator == null) {
                return;
            }
            m_PriorityValueLabel.text = "Value: " + priorityEvaluator.GetPriorityValue();
        }
    }
}
#endif
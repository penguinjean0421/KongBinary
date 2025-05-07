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
    /// Adds UI elements within the task node.
    /// </summary>
    [ControlType(typeof(IAction))]
    [ControlType(typeof(IConditional))]
    [ControlType(typeof(IComposite))]
    [ControlType(typeof(IDecorator))]
    public class TaskNodeViewControl : NodeViewBase
    {
        private const string c_DarkConditionalAbortLowerPriorityIconGUID = "ba6528926e3f4f7438d3b9737f595ec6";
        private const string c_LightConditionalAbortLowerPriorityIconGUID = "20be04a2e46cb9d40b601dccdfbe153b";
        private const string c_DarkConditionalAbortSelfIconGUID = "ff3ba64f23e708645b24cc7509b5ebe5";
        private const string c_LightConditionalAbortSelfIconGUID = "5d44e66bacdbe51408dd30e519c2b318";
        private const string c_DarkConditionalAbortBothIconGUID = "1c01950cc0f1c994cb5ff3576969ebbf";
        private const string c_LightConditionalAbortBothIconGUID = "90b22fc04519bdb44b4d83665e86381d";

        private const string c_DarkSuccessIconGUID = "240eed9b6e6dc004f94216f1e9fcc390";
        private const string c_LightSuccessIconGUID = "cf3f27e8ca1f20f4680890e078c7613a";
        private const string c_DarkSuccessReevaluateIconGUID = "0a5037ce131729b4fa0ffa9e1e13d387";
        private const string c_LightSuccessReevaluateIconGUID = "bba6bdc3af0aac44dadd1ca3a8485b05";
        private const string c_DarkFailureIconGUID = "8d159db7a8da43e41a50a77e43cfd6ba";
        private const string c_LightFailureIconGUID = "c3622912d9f7bcd41a54a95add672423";
        private const string c_DarkFailureReevaluateIconGUID = "26de8afeb313fd84291f98e68db44df7";
        private const string c_LightFailureReevaluateIconGUID = "5a6d713911c8ec3488639e2934329033";

        private ILogicNode m_Node;
        private GraphWindow m_GraphWindow;
        private BehaviorTree m_BehaviorTree;

        private Image m_ExecutionStatusIcon;
        private Image m_ConditionalAbortIcon;
        private LogicNode m_LogicNode;

        private Texture m_SuccessIcon;
        private Texture m_SuccessReevaluateIcon;
        private Texture m_FailureIcon;
        private Texture m_FailureReevaluateIcon;

        /// <summary>
        /// Addes the UIElements for the specified runtime node to the editor Node within the graph.
        /// </summary>
        /// <param name="graphWindow">A reference to the GraphWindow.</param>
        /// <param name="parent">The parent UIElement that should contain the node UIElements.</param>
        /// <param name="node">The node that the control represents.</param>
        public override void AddNodeView(GraphWindow graphWindow, VisualElement parent, object node)
        {
            m_Node = node as ILogicNode;
            m_GraphWindow = graphWindow;
            m_BehaviorTree = m_GraphWindow.Graph as BehaviorTree;
            m_LogicNode = parent.GetFirstAncestorOfType<LogicNode>();

            if (node is IConditionalAbortParent conditionalAbortParent) {
                m_ConditionalAbortIcon = new Image();
                m_ConditionalAbortIcon.name = "conditional-abort-icon";
                parent.parent.Add(m_ConditionalAbortIcon);
                SetConditionalAbortIcon(conditionalAbortParent);

                m_ConditionalAbortIcon.RegisterCallback<AttachToPanelEvent>(c =>
                {
                    GraphEventHandler.RegisterEvent<object>(GraphEventType.NodeValueUpdated, UpdateNodeValue);
                });
                m_ConditionalAbortIcon.RegisterCallback<DetachFromPanelEvent>(c =>
                {
                    GraphEventHandler.UnregisterEvent<object>(GraphEventType.NodeValueUpdated, UpdateNodeValue);
                });
            }

            // Subtree references can click into its references.
            if (m_Node is ISubtreeReference) {
                m_LogicNode.RegisterCallback<MouseDownEvent>(OnSubtreeRferenceMouseDown);
            }

            // AddNodeView can be called multiple times. Ensure there is only one execution status image.
            var previousExecutionStatus = m_LogicNode.Q("execution-status");
            if (previousExecutionStatus != null) {
                previousExecutionStatus.parent.Remove(previousExecutionStatus);
            }
            m_ExecutionStatusIcon = new Image();
            m_ExecutionStatusIcon.name = "execution-status";
            parent.parent.Add(m_ExecutionStatusIcon); // The execution status icon should be placed behind every node element.
            m_ExecutionStatusIcon.SendToBack();

            m_SuccessIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkSuccessIconGUID : c_LightSuccessIconGUID);
            m_SuccessReevaluateIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkSuccessReevaluateIconGUID : c_LightSuccessReevaluateIconGUID);
            m_FailureIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkFailureIconGUID : c_LightFailureIconGUID);
            m_FailureReevaluateIcon = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkFailureReevaluateIconGUID : c_LightFailureReevaluateIconGUID);

            m_ExecutionStatusIcon.RegisterCallback<AttachToPanelEvent>(c =>
            {
                GraphEventHandler.RegisterEvent(GraphEventType.WindowUpdate, UpdateNode);
                m_GraphWindow.OnSelectionChange += OnSelectionChange;
            });
            m_ExecutionStatusIcon.RegisterCallback<DetachFromPanelEvent>(c =>
            {
                GraphEventHandler.UnregisterEvent(GraphEventType.WindowUpdate, UpdateNode);
                m_GraphWindow.OnSelectionChange -= OnSelectionChange;
            });
            OnSelectionChange(Selection.activeObject);
        }

        /// <summary>
        /// Sets the conditional abort icon.
        /// </summary>
        /// <param name="conditionalAbortParent">The conditional abort node.</param>
        private void SetConditionalAbortIcon(IConditionalAbortParent conditionalAbortParent)
        {
            if (conditionalAbortParent.AbortType == ConditionalAbortType.LowerPriority) {
                m_ConditionalAbortIcon.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkConditionalAbortLowerPriorityIconGUID : c_LightConditionalAbortLowerPriorityIconGUID);
            } else if (conditionalAbortParent.AbortType == ConditionalAbortType.Self) {
                m_ConditionalAbortIcon.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkConditionalAbortSelfIconGUID : c_LightConditionalAbortSelfIconGUID);
            } else if (conditionalAbortParent.AbortType == ConditionalAbortType.Both) {
                m_ConditionalAbortIcon.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkConditionalAbortBothIconGUID : c_LightConditionalAbortBothIconGUID);
            } else {
                m_ConditionalAbortIcon.image = null;
            }
        }

        /// <summary>
        /// A value has been updated for the specified node.
        /// </summary>
        /// <param name="node">The node that has been updated.</param>
        private void UpdateNodeValue(object node)
        {
            if (node != m_Node) {
                return;
            }

            SetConditionalAbortIcon(node as IConditionalAbortParent);
        }

        /// <summary>
        /// Updates the node with the current execution status.
        /// </summary>
        private void UpdateNode()
        {
            UpdateNodeInternal();
        }

        /// <summary>
        /// Internal method which updates the node with the current execution status.
        /// </summary>
        /// <returns>The status of the task.</returns>
        protected virtual TaskStatus UpdateNodeInternal()
        {
            if (m_BehaviorTree == null || m_BehaviorTree.Entity == Entity.Null || m_Node.RuntimeIndex == ushort.MaxValue || !m_BehaviorTree.World.EntityManager.Exists(m_BehaviorTree.Entity)) {
                // The task is no longer active. Reset the status while keeping the previous execution state.
                m_LogicNode.SetColorState(m_GraphWindow.GraphEditor.IsNodeHierarchyEnabled(m_Node) ? ColorState.Default : ColorState.Disabled, 0.5f);
                if (m_ExecutionStatusIcon.image != null) {
                    if (m_ExecutionStatusIcon.image == m_SuccessReevaluateIcon) {
                        m_ExecutionStatusIcon.image = m_SuccessIcon;
                    } else if (m_ExecutionStatusIcon.image == m_FailureReevaluateIcon) {
                        m_ExecutionStatusIcon.image = m_FailureIcon;
                    }
                    m_ExecutionStatusIcon.style.width = m_ExecutionStatusIcon.image.width;
                }
                return TaskStatus.Inactive;
            }

            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity);
            var taskComponent = taskComponents[m_Node.RuntimeIndex];
            if (taskComponent.Status == TaskStatus.Success) {
                if (taskComponent.Reevaluate) {
                    m_ExecutionStatusIcon.image = m_SuccessReevaluateIcon;
                } else {
                    m_ExecutionStatusIcon.image = m_SuccessIcon;
                }
            } else if (taskComponent.Status == TaskStatus.Failure) {
                if (taskComponent.Reevaluate) {
                    m_ExecutionStatusIcon.image = m_FailureReevaluateIcon;
                } else {
                    m_ExecutionStatusIcon.image = m_FailureIcon;
                }
            } else if (m_ExecutionStatusIcon.image != null) {
                m_ExecutionStatusIcon.image = null;
            }

            if (m_ExecutionStatusIcon.image != null) {
                m_ExecutionStatusIcon.style.width = m_ExecutionStatusIcon.image.width;
            }

            if (taskComponent.Status == TaskStatus.Running || taskComponent.Status == TaskStatus.Queued) {
                m_LogicNode.SetColorState(ColorState.Active, 0);
            } else {
                m_LogicNode.SetColorState(m_GraphWindow.GraphEditor.IsNodeHierarchyEnabled(m_Node) ? ColorState.Default : ColorState.Disabled, 0.5f);
            }
            return taskComponent.Status;
        }

        /// <summary>
        /// Enables or disables the NodeView.
        /// </summary>
        /// <param name="enable">True if the NodeView is enabled.</param>
        public override void SetEnabled(bool enable)
        {
            if (m_ConditionalAbortIcon != null) {
                m_ConditionalAbortIcon.SetEnabled(enable);
            }
        }

        /// <summary>
        /// The mouse has been pressed.
        /// </summary>
        /// <param name="evt">The event that triggered the press.</param>
        private void OnSubtreeRferenceMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount != 2) {
                return;
            }

            var subtreeReference = m_Node as ISubtreeReference;
            if (subtreeReference.Subtrees == null) {
                return;
            }

            for (int i = 0; i < subtreeReference.Subtrees.Length; ++i) {
                var subtree = subtreeReference.Subtrees[i];
                if (subtree == null) {
                    continue;
                }

                Selection.activeObject = subtree;
            }
        }

        /// <summary>
        /// The GraphWindow selection has changed.
        /// </summary>
        /// <param name="selection">The new selection.</param>
        private void OnSelectionChange(UnityEngine.Object selection)
        {
            if (!Application.isPlaying || m_BehaviorTree == null || m_BehaviorTree.Entity == Entity.Null || !m_BehaviorTree.enabled || !m_BehaviorTree.Baked) {
                return;
            }

            // The behavior tree was baked. Ensure the BehaviorTree component is pointing to the correct Entity. 
            var worlds = World.All;
            var foundEntity = false;
            for (int i = 0; i < worlds.Count; ++i) {
                var defaultEntities = worlds[i].EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp);
                if (defaultEntities != null) {
                    var originalGuid = m_BehaviorTree.World.EntityManager.GetComponentData<EntityGuid>(m_BehaviorTree.Entity);

                    foreach (var defaultEntity in defaultEntities) {
                        if (worlds[i].EntityManager.HasComponent<EntityGuid>(defaultEntity)) {
                            var entityGuid = worlds[i].EntityManager.GetComponentData<EntityGuid>(defaultEntity);
                            if (originalGuid.OriginatingId == entityGuid.OriginatingId) {
                                m_BehaviorTree.World = worlds[i];
                                m_BehaviorTree.Entity = defaultEntity;
                                foundEntity = true;
                                break;
                            }
                        }
                    }
                    defaultEntities.Dispose();
                    if (foundEntity) {
                        m_BehaviorTree.Baked = false;
                        break;
                    }
                }
            }
        }
    }
}
#endif
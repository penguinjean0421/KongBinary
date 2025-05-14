#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Events
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// The EventNode that is invoked when the agent exits a 2D trigger.
    /// </summary>
    [AllowMultipleTypes]
    [NodeIcon("06864c37115f11445b04701c616d0e14", "8b8a2793322238240b4f25171d772003")]
    public class OnTriggerExit2D : EventNode
    {
        [Tooltip("The tag of the GameObject that the trigger should be checked against.")]
        [SerializeField] protected SharedVariable<string> m_Tag;
        [Tooltip("The exited trigger GameObject.")]
        [SerializeField] protected SharedVariable<GameObject> m_StoredOtherColliderGameObject;

        /// <summary>
        /// Initializes the node to the specified graph.
        /// </summary>
        /// <param name="graph">The graph that is initializing the task.</param>
        public override void Initialize(IGraph graph)
        {
            base.Initialize(graph);

            m_BehaviorTree.OnBehaviorTreeDestroyed += Destroy;
            m_BehaviorTree.OnBehaviorTreeTriggerExit2D += ExitedTrigger2D;
        }

        /// <summary>
        /// The agent has exited a 2D trigger.
        /// </summary>
        /// <param name="other">The trigger that the agent exited.</param>
        private void ExitedTrigger2D(Collider2D other)
        {
            if (!string.IsNullOrEmpty(m_Tag.Value) && !other.gameObject.CompareTag(m_Tag.Value)) {
                return;
            }

            if (m_StoredOtherColliderGameObject != null && m_StoredOtherColliderGameObject.IsShared) { m_StoredOtherColliderGameObject.Value = other.gameObject; }

            m_BehaviorTree.StartBranch(this);
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        private void Destroy()
        {
            m_BehaviorTree.OnBehaviorTreeDestroyed -= Destroy;
            m_BehaviorTree.OnBehaviorTreeTriggerExit2D -= ExitedTrigger2D;
        }
    }
}
#endif
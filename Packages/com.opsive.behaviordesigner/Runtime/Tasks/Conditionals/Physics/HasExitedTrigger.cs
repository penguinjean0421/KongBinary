#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals.Physics
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Returns success when the agent exits a trigger. This task will only receive the physics callback if it is being reevaluated (with a conditional abort or under a parallel task).
    /// </summary>
    [NodeDescription("Returns success when an object exits the trigger. This task will only receive the physics callback if it is being reevaluated (with a conditional abort or under a parallel task).")]
    public class HasExitedTrigger : Conditional
    {
        [Tooltip("The tag of the GameObject that the trigger should be checked against.")]
        [SerializeField] protected SharedVariable<string> m_Tag;
        [Tooltip("The exited trigger.")]
        [SerializeField] protected SharedVariable<Collider> m_StoredOtherCollider;

        protected override bool ReceiveTriggerExitCallback => true;

        private bool m_ExitedTrigger;

        /// <summary>
        /// Returns true when the agent has exited a trigger.
        /// </summary>
        /// <returns>True when the agent has exited a trigger.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_ExitedTrigger ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// The agent has exited a trigger.
        /// </summary>
        /// <param name="other">The trigger that the agent exited.</param>
        protected override void OnTriggerExit(Collider other)
        {
            if (!string.IsNullOrEmpty(m_Tag.Value) && !other.gameObject.CompareTag(m_Tag.Value)) {
                return;
            }

            if (m_StoredOtherCollider != null && m_StoredOtherCollider.IsShared) { m_StoredOtherCollider.Value = other; }

            m_ExitedTrigger = true;
        }
    }
}
#endif
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Returns success as soon as the target object can be seen.
    /// This task is basic intended for demo purposes. For a more complete task see the Senses Pack:
    /// https://assetstore.unity.com/packages/slug/310244
    /// </summary>
    public class CanSeeObject : Conditional
    {
        [Tooltip("The object that the agent is searching for.")]
        [SerializeField] protected SharedVariable<GameObject> m_Target;
        [Tooltip("The raycast offset to apply to the target.")]
        [SerializeField] protected SharedVariable<Vector3> m_TargetOffset;
        [Tooltip("The field of view angle of the agent (in degrees).")]
        [SerializeField] protected SharedVariable<float> m_FieldOfView = 90;
        [Tooltip("The distance that the agent can see.")]
        [SerializeField] protected SharedVariable<float> m_ViewDistance = 1000;

        /// <summary>
        /// Returns success if an object was found otherwise failure
        /// </summary>
        /// <returns></returns>
        public override TaskStatus OnUpdate()
        {
            // Return success if an object was found.
            return WithinSight() ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// Determines if the target object is within sight of the agent.
        /// </summary>
        /// <returns>True if the target is within sight.</returns>
        private bool WithinSight()
        {
            if (m_Target.Value == null) {
                return false;
            }

            var targetPosition = m_Target.Value.transform.TransformPoint(m_TargetOffset.Value);
            var direction = targetPosition - m_Transform.position;
            direction.y = 0;
            var angle = Vector3.Angle(direction, m_Transform.forward);
            if (direction.magnitude < m_ViewDistance.Value && angle < m_FieldOfView.Value * 0.5f) {
                RaycastHit hit;
                if (Physics.Linecast(m_Transform.position, targetPosition, out hit)) {
                    if (hit.transform.IsChildOf(m_Target.Value.transform) || m_Target.Value.transform.IsChildOf(hit.transform)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
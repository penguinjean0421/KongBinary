/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Uses the NavMeshAgent to patrol through a set of waypoints.
    /// This task is basic intended for demo purposes. For a more complete task see the Movement Pack:
    /// https://assetstore.unity.com/packages/slug/310243
    /// </summary>
    public class RotateTowards : Action
    {
        [Tooltip("The patrol waypoints.")]
        [SerializeField] protected SharedVariable<GameObject> m_Target;
        [Tooltip("The speed the agent should rotate.")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_MaxLookAtRotationDelta")]
        [SerializeField] protected SharedVariable<float> m_MaxLookAtRotationDelta = 1;
        [Tooltip("Should the rotation only affect the Y axis?")]
        [UnityEngine.Serialization.FormerlySerializedAs("m_IgnoreY")]
        [SerializeField] protected SharedVariable<bool> m_OnlyY;

        /// <summary>
        /// Rotates towards the target.
        /// </summary>
        /// <returns>A status of running.</returns>
        public override TaskStatus OnUpdate()
        {
            var direction = m_Target.Value.transform.position - m_Transform.position;
            if (m_OnlyY.Value) {
                direction.y = 0;
            }
            direction.Normalize();
            var lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, m_MaxLookAtRotationDelta.Value);
            if (Quaternion.Angle(transform.rotation, lookRotation) < m_MaxLookAtRotationDelta.Value) {
                transform.rotation = lookRotation;
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}
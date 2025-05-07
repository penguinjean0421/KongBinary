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
    /// Returns success when the target is within the specified distance.
    /// This task is basic intended for demo purposes. For a more complete task see the Senses Pack:
    /// https://assetstore.unity.com/packages/slug/310244
    /// </summary>
    public class WithinDistance : Conditional
    {
        [Tooltip("The object that the agent is searching for.")]
        [SerializeField] protected SharedVariable<GameObject> m_Target;
        [Tooltip("The distance that should be checked against.")]
        [SerializeField] protected SharedVariable<float> m_Distance = 10;

        /// <summary>
        /// Returns success if an object was found otherwise failure
        /// </summary>
        /// <returns></returns>
        public override TaskStatus OnUpdate()
        {
            // Return success if the target is within distance.
            return TargetWithinDistance() ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// Determines if the target object is distance of the agent.
        /// </summary>
        /// <returns>True if the target is within sight.</returns>
        private bool TargetWithinDistance()
        {
            if (m_Target.Value == null) {
                return false;
            }
            return (m_Target.Value.transform.position - m_Transform.position).magnitude < m_Distance.Value ? true : false;
        }
    }
}
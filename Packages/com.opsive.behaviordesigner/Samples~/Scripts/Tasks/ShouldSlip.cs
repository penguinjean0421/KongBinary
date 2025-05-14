/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [NodeDescription("Uses the agent's speed to determine if the agent should slip.")]
    public class ShouldSlip : Conditional
    {
        [Tooltip("The speed of the agent.")]
        [SerializeField] protected SharedVariable<int> m_Speed;
        [Tooltip("The speed of the opponent.")]
        [SerializeField] protected SharedVariable<int> m_OpponentSpeed;
        [Tooltip("The error range for if the agent is faster than the opponent (0-1).")]
        [SerializeField] protected SharedVariable<RangeFloat> m_Range;
        [Tooltip("A random probability that the agent can slip.")]
        [SerializeField] protected SharedVariable<float> m_Probability;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>Success if the agent should slip.</returns>
        public override TaskStatus OnUpdate()
        {
            // The agent should slip if they are faster within an error range.
            if (m_Speed.Value + m_Range.Value.RandomValue > m_OpponentSpeed.Value) {
                return TaskStatus.Success;
            }
            // Random probability that they can slip.
            if (Random.value < m_Probability.Value) {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
    }
}
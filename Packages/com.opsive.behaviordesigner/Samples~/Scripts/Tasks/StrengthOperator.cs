/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [NodeDescription("Adjusts the health amount by a strength factor.")]
    public class StrengthOperator : Action
    {
        [Tooltip("The amount of damage to apply.")]
        [SerializeField] protected SharedVariable<float> m_DamageAmount;
        [Tooltip("The agent's strength")]
        [SerializeField] protected SharedVariable<int> m_Strength;
        [Tooltip("The variable to store the result.")]
        [RequireShared] [SerializeField] protected SharedVariable<float> m_HealthResult;

        /// <summary>
        /// Deducts the health value.
        /// </summary>
        /// <returns>Success after the deduction is applied.</returns>
        public override TaskStatus OnUpdate()
        {
            m_HealthResult.Value -= m_DamageAmount.Value * Mathf.Max(1, m_Strength.Value);
            return TaskStatus.Success;
        }
    }
}
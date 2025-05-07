#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Tasks.Actions.Math
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [Tooltip("Returns a random float between the specified values (inclusive).")]
    public class RandomFloat : Action
    {
        [Tooltip("The minimum float value (inclusive).")]
        [SerializeField] protected SharedVariable<float> m_MinimumFloat;
        [Tooltip("The maximum float value (inclusive).")]
        [SerializeField] protected SharedVariable<float> m_MaximumFloat;
        [Tooltip("The stored random float value.")]
        [RequireShared] [SerializeField] protected SharedVariable<float> m_StoreResult;
        [Tooltip("The seed of the random number generator. Set to 0 to disable.")]
        [SerializeField] protected int m_Seed;

        /// <summary>
        /// Callback when the behavior tree is initialized.
        /// </summary>
        public override void OnAwake()
        {
            if (m_Seed != 0) {
                Random.InitState(m_Seed);
            }
        }

        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_StoreResult.Value = Random.Range(m_MinimumFloat.Value, m_MaximumFloat.Value);
            return base.OnUpdate();
        }

    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions.UnityObjects
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [NodeDescription("Sets a random Transform value.")]
    public class RandomTransform : Action
    {
        [Tooltip("The list of possible Transforms.")]
        [SerializeField] protected SharedVariable<Transform[]> m_Transforms;
        [Tooltip("The variable that should be set.")]
        [RequireShared] [SerializeField] protected SharedVariable<Transform> m_StoreResult;
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
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Transforms.Value == null || m_Transforms.Value.Length == 0) {
                return TaskStatus.Failure;
            }

            m_StoreResult.Value = m_Transforms.Value[Random.Range(0, m_Transforms.Value.Length - 1)];
            return TaskStatus.Success;
        }
    }
}
#endif
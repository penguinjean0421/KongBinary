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

    [NodeDescription("Returns success as soon as the current Animator state ends.")]
    public class WaitForAnimatorState : TargetGameObjectAction
    {
        [Tooltip("The layer to wait for the state on.")]
        public SharedVariable<int> m_Layer;

        private Animator m_Animator;
        private int m_StateHash;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            m_Animator = gameObject.GetComponent<Animator>();
        }

        /// <summary>
        /// Caches the Animator state.
        /// </summary>
        public override void OnStart()
        {
            m_StateHash = (m_Animator.IsInTransition(m_Layer.Value) ? m_Animator.GetNextAnimatorStateInfo(m_Layer.Value) : m_Animator.GetCurrentAnimatorStateInfo(m_Layer.Value)).fullPathHash;
        }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Animator.IsInTransition(m_Layer.Value)) {
                return TaskStatus.Running;
            }
            var currentState = m_Animator.GetCurrentAnimatorStateInfo(m_Layer.Value).fullPathHash;
            if (currentState != m_StateHash) {
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }
    }
}
#endif
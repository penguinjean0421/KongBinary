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

    [NodeDescription("Enables or disables the specified MonoBehaviour.")]
    public class SetEnabled : Action
    {
        [Tooltip("Should the MonoBehaviour be enabled?")]
        [SerializeField] protected SharedVariable<bool> m_Enable;
        [Tooltip("The MonoBehaviour that should be enabled or disabled.")]
        [SerializeField] protected SharedVariable<MonoBehaviour> m_MonoBehaviour;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_MonoBehaviour.Value == null) {
                return TaskStatus.Failure;
            }
            m_MonoBehaviour.Value.enabled = m_Enable.Value;
            return TaskStatus.Success;
        }
    }
}
#endif
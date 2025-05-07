#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions.Math
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [NodeDescription("Flips the value of the boolean.")]
    public class BoolFlip : Action
    {
        [Tooltip("The bool that should be flipped.")]
        [SerializeField] protected SharedVariable<bool> m_Bool;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Bool.Value = !m_Bool.Value;
            return TaskStatus.Success;
        }
    }
}
#endif
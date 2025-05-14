#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals.Math
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    [NodeDescription("Compares two Vector2 values.")]
    public class Vector2Comparison : Conditional
    {
        [Tooltip("The first Vector2.")]
        [SerializeField] protected SharedVariable<Vector2> m_Vector1;
        [Tooltip("The second Vector2.")]
        [SerializeField] protected SharedVariable<Vector2> m_Vector2;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_Vector1.Value == m_Vector2.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
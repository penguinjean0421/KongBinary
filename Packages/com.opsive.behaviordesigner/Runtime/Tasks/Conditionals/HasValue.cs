#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using System.Collections;
    using UnityEngine;

    [NodeDescription("Returns true if the specified variable has a value.")]
    public class HasValue : Conditional
    {
        [Tooltip("The variable to compare.")]
        [SerializeField] protected SharedVariable m_Variable;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            object variableValue = null;
            if (m_Variable == null || (variableValue = m_Variable.GetValue()) == null) {
                return TaskStatus.Failure;
            }

            if (variableValue is IList listValue) {
                return listValue.Count > 0 ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }
}
#endif
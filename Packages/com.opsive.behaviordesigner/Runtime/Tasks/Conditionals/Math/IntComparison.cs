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

    [NodeDescription("Compares two integer values.")]
    public class IntComparison : Conditional
    {
        /// <summary>
        /// Specifies the type of comparison that should be performed.
        /// </summary>
        protected enum Operation
        {
            LessThan,
            LessThanOrEqualTo,
            EqualTo,
            NotEqualTo,
            GreaterThanOrEqualTo,
            GreaterThan
        }

        [Tooltip("The operation that should be performed.")]
        [SerializeField] protected SharedVariable<Operation> m_Operation;
        [Tooltip("The first integer.")]
        [SerializeField] protected SharedVariable<int> m_Integer1;
        [Tooltip("The second integer.")]
        [SerializeField] protected SharedVariable<int> m_Integer2;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            switch (m_Operation.Value) {
                case Operation.LessThan:
                    return m_Integer1.Value < m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.LessThanOrEqualTo:
                    return m_Integer1.Value <= m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.EqualTo:
                    return m_Integer1.Value == m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.NotEqualTo:
                    return m_Integer1.Value != m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThanOrEqualTo:
                    return m_Integer1.Value >= m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
                case Operation.GreaterThan:
                    return m_Integer1.Value > m_Integer2.Value ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Failure;
        }
    }
}
#endif
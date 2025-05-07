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

    [NodeDescription("Performs a math operation on the two integers.")]
    public class IntOperator : Action
    {
        /// <summary>
        /// Specifies the type of int operation that should be performed.
        /// </summary>
        protected enum Operation
        {
            Add,        // Returns the addition between two integers.
            Subtract,   // Returns the division between two integers.
            Multiply,   // Returns the multiplication between two integers.
            Divide,     // Returns the division between two integers.
            Modulo,     // Returns the modulo between two integers.
            Min,        // Returns the minimum of two integers.
            Max,        // Returns the maximum of two integers.
        }

        [Tooltip("The operation to perform.")]
        [SerializeField] protected SharedVariable<Operation> m_Operation;
        [Tooltip("The first integer.")]
        [SerializeField] protected SharedVariable<int> m_Integer1;
        [Tooltip("The second integer.")]
        [SerializeField] protected SharedVariable<int> m_Integer2;
        [Tooltip("The variable to store the result.")]
        [RequireShared] [SerializeField] protected SharedVariable<int> m_StoreResult;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            switch (m_Operation.Value) {
                case Operation.Add:
                    m_StoreResult.Value = m_Integer1.Value + m_Integer2.Value;
                    break;
                case Operation.Subtract:
                    m_StoreResult.Value = m_Integer1.Value - m_Integer2.Value;
                    break;
                case Operation.Multiply:
                    m_StoreResult.Value = m_Integer1.Value * m_Integer2.Value;
                    break;
                case Operation.Divide:
                    m_StoreResult.Value = m_Integer1.Value / m_Integer2.Value;
                    break;
                case Operation.Modulo:
                    m_StoreResult.Value = m_Integer1.Value % m_Integer2.Value;
                    break;
                case Operation.Min:
                    m_StoreResult.Value = Mathf.Min(m_Integer1.Value, m_Integer2.Value);
                    break;
                case Operation.Max:
                    m_StoreResult.Value = Mathf.Max(m_Integer1.Value, m_Integer2.Value);
                    break;
            }
            return TaskStatus.Success;
        }
    }
}
#endif
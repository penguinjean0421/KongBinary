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

    [NodeDescription("Performs a math operation on the two floats.")]
    public class FloatOperator : Action
    {
        /// <summary>
        /// Specifies the type of float operation that should be performed.
        /// </summary>
        protected enum Operation
        {
            Add,        // Returns the addition between two floats.
            Subtract,   // Returns the division between two floats.
            Multiply,   // Returns the multiplication between two floats.
            Divide,     // Returns the division between two floats.
            Modulo,     // Returns the modulo between two floats.
            Min,        // Returns the minimum of two floats.
            Max,        // Returns the maximum of two floats.
        }

        [Tooltip("The operation to perform.")]
        [SerializeField] protected SharedVariable<Operation> m_Operation;
        [Tooltip("The first float.")]
        [SerializeField] protected SharedVariable<float> m_Float1;
        [Tooltip("The second float.")]
        [SerializeField] protected SharedVariable<float> m_Float2;
        [Tooltip("The variable to store the result.")]
        [RequireShared] [SerializeField] protected SharedVariable<float> m_StoreResult;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            switch (m_Operation.Value) {
                case Operation.Add:
                    m_StoreResult.Value = m_Float1.Value + m_Float2.Value;
                    break;
                case Operation.Subtract:
                    m_StoreResult.Value = m_Float1.Value - m_Float2.Value;
                    break;
                case Operation.Multiply:
                    m_StoreResult.Value = m_Float1.Value * m_Float2.Value;
                    break;
                case Operation.Divide:
                    m_StoreResult.Value = m_Float1.Value / m_Float2.Value;
                    break;
                case Operation.Modulo:
                    m_StoreResult.Value = m_Float1.Value % m_Float2.Value;
                    break;
                case Operation.Min:
                    m_StoreResult.Value = Mathf.Min(m_Float1.Value, m_Float2.Value);
                    break;
                case Operation.Max:
                    m_StoreResult.Value = Mathf.Max(m_Float1.Value, m_Float2.Value);
                    break;
            }
            return TaskStatus.Success;
        }
    }
}
#endif
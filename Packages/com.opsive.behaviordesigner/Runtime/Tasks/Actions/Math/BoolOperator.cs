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

    [NodeDescription("Performs a math operation on the two booleans.")]
    public class BoolOperator : Action
    {
        /// <summary>
        /// Specifies the type of bool operation that should be performed.
        /// </summary>
        protected enum Operation
        {
            AND,    // Returns the AND between two booleans.
            OR,     // Returns the OR between two booleans.
            NAND,   // Returns the NAND between two booleans.
            XOR,    // Returns the XOR between two booleans.
        }

        [Tooltip("The operation to perform.")]
        [SerializeField] protected SharedVariable<Operation> m_Operation;
        [Tooltip("The first boolean.")]
        [SerializeField] protected SharedVariable<bool> m_Bool1;
        [Tooltip("The second boolean.")]
        [SerializeField] protected SharedVariable<bool> m_Bool2;
        [Tooltip("The variable to store the result.")]
        [RequireShared] [SerializeField] protected SharedVariable<bool> m_StoreResult;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            switch (m_Operation.Value) {
                case Operation.AND:
                    m_StoreResult.Value = m_Bool1.Value && m_Bool2.Value;
                    break;
                case Operation.OR:
                    m_StoreResult.Value = m_Bool1.Value || m_Bool2.Value;
                    break;
                case Operation.NAND:
                    m_StoreResult.Value = !(m_Bool1.Value && m_Bool2.Value);
                    break;
                case Operation.XOR:
                    m_StoreResult.Value = (m_Bool1.Value ^ m_Bool2.Value);
                    break;
            }
            return TaskStatus.Success;
        }
    }
}
#endif
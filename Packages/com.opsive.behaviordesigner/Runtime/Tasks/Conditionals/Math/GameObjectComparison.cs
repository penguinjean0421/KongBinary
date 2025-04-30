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

    [NodeDescription("Compares two GameObject values.")]
    public class GameObjectComparison : Conditional
    {
        [Tooltip("The first GameObject.")]
        [SerializeField] protected SharedVariable<GameObject> m_GameObject1;
        [Tooltip("The second GameObject.")]
        [SerializeField] protected SharedVariable<GameObject> m_GameObject2;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_GameObject1.Value == m_GameObject2.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Logs the specified string.
    /// </summary>
    [NodeIcon("c97bee71424b3e247a161d1279643506", "138439e3588de5d449b7949d68d32ad8")]
    [NodeDescription("A simple task which will output the specified text and return success. It can be used for debugging.")]
    public class Log : Action
    {
        [Tooltip("The string that should be outputted to the console.")]
        [SerializeField] protected SharedVariable<string> m_Text;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            Debug.Log(m_Text.Value);
            return TaskStatus.Success;
        }
    }
}
#endif
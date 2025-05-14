#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Implements the PriorityEvaluator returning a SharedVariable value.
    /// </summary>
    [NodeDescription("Sets the priority value to the specified SharedVariable float value.")]
    public class PriorityVariableEvaluator : PriorityEvaluator
    {
        [Tooltip("The priority of the decorator.")]
        [SerializeField] SharedVariable<float> m_Priority;

        /// <summary>
        /// Returns the priority of the decorator. The higher the priority the more likely the task will run next.
        /// </summary>
        /// <returns>The priority of the decorator.</returns>
        public override float GetPriorityValue() { return m_Priority.Value; }
    }
}
#endif
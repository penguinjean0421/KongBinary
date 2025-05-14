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
    /// Implements the UtilityEvaluator returning a SharedVariable value.
    /// </summary>
    [NodeDescription("Sets the utility value to the specified SharedVariable float value.")]
    public class UtilityVariableEvaluator : UtilityEvaluator
    {
        [Tooltip("The utility of the decorator.")]
        [SerializeField] SharedVariable<float> m_Utility;

        /// <summary>
        /// Returns the utility of the decorator. The higher the utility the more likely the task will run next.
        /// </summary>
        /// <returns>The utility of the decorator.</returns>
        public override float GetUtilityValue() { return m_Utility.Value; }
    }
}
#endif
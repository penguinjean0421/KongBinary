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
    using UnityEngine.Serialization;

    /// <summary>
    /// Implements the UtilityEvaluator returning a SharedVariable value.
    /// </summary>
    [NodeDescription("Sets the utility value to the evaluated curve value.")]
    public class UtilityCurveEvaluator : UtilityEvaluator
    {
        [Tooltip("The curve that should be evaluated for the utility value.")]
        [FormerlySerializedAs("m_Utility")]
        [SerializeField] SharedVariable<AnimationCurve> m_UtilityValue;
        [Tooltip("The time to evaluate the curve at. Set it to -1 to use Time.time.")]
        [SerializeField] SharedVariable<float> m_Time;
        [Tooltip("Should the time be reset after the task stops running?")]
        [SerializeField] SharedVariable<bool> m_ResetTimeOnEnd;

        private float m_ResetTime = -1;

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            m_UtilityValue = AnimationCurve.Linear(0, 0, 1, 1);
            m_Time = -1;
            m_ResetTimeOnEnd = false;
        }

        /// <summary>
        /// Returns the utility of the decorator. The higher the utility the more likely the task will run next.
        /// </summary>
        /// <returns>The utility of the decorator.</returns>
        public override float GetUtilityValue()
        {
            var evaluateTime = m_Time.Value == -1 ? Time.time : m_Time.Value;
            if (m_ResetTimeOnEnd.Value && m_ResetTime != -1) {
                evaluateTime = Time.time - m_ResetTime;
            }

            return m_UtilityValue.Value.Evaluate(evaluateTime);
        }

        /// <summary>
        /// The task has ended.
        /// </summary>
        public override void OnEnd()
        {
            m_ResetTime = Time.time;
        }
    }
}
#endif
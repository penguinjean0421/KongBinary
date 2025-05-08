#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Utility
{
    using UnityEngine;
    using System;

    /// <summary>
    /// Specifies a range between two floats.
    /// </summary>
    public struct RangeFloat
    {
        [Tooltip("The minimal float value (inclusive).")]
        [SerializeField] public float Min;
        [Tooltip("The maximum float value (inclusive).")]
        [SerializeField] public float Max;

        public float RandomValue { get => UnityEngine.Random.Range(Min, Max); }

        /// <summary>
        /// RangeFloat constructor.
        /// </summary>
        /// <param name="min">The minimal float value.</param>
        /// <param name="max">The maximal float value.</param>
        public RangeFloat(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// Specifies that the task name should be hidden in the node view.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class HideNameInTaskControlAttribute : Attribute { }
}
#endif
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;
    using Unity.Entities;
    using System;

    /// <summary>
    /// Simple health component. Can be baked to an entity or used with property bindings.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Tooltip("The maximum amount of health that the entity can have.")]
        [SerializeField] protected float m_MaxHealth = 100;
        [Tooltip("Should the health value be baked to a DOTS component?")]
        [SerializeField] protected bool m_BakeToEntity;

        public float MaxHealth { get => m_MaxHealth; }

        private float m_Value;
        private Action<float> m_OnUpdateValue;

        public float Value { get => m_Value; set { UpdateHealthValue(value); } }
        public Action<float> OnUpdateValue { get => m_OnUpdateValue; set => m_OnUpdateValue = value; }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Value = m_MaxHealth;
        }

        /// <summary>
        /// Updates the health value amount.
        /// </summary>
        /// <param name="value">The target value.</param>
        public void UpdateHealthValue(float value)
        {
            value = Mathf.Clamp(value, 0, m_MaxHealth);
            if (value == m_Value) {
                return;
            }

            m_Value = value;

            m_OnUpdateValue?.Invoke(value);
        }

        /// <summary>
        /// Converts the health component to a DOTS entity.
        /// </summary>
        public class HealthBaker : Baker<Health>
        {
            /// <summary>
            /// Bakes the health component to the DOTS entity.
            /// </summary>
            /// <param name="behaviorTree">The authoring behavior tree.</param>
            public override void Bake(Health health)
            {
                if (!health.m_BakeToEntity) {
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<HealthComponent>(entity, new HealthComponent()
                {
                    Value = health.m_MaxHealth
                });
            }
        }
    }

    /// <summary>
    /// The DOTS health component.
    /// </summary>
    public struct HealthComponent : IComponentData
    {
        [Tooltip("The amount of health remaining.")]
        public float Value;
    }
}
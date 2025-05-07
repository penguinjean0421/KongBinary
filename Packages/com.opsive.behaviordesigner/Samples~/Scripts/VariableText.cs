/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Sets the variable value to a text label.
    /// </summary>
    public class VariableText : MonoBehaviour
    {
        [Tooltip("The label of the variable.")]
        [SerializeField] protected string m_Label;
        [Tooltip("The behavior tree that the variable exists on.")]
        [SerializeField] protected BehaviorTree m_BehaviorTree;
        [Tooltip("The name of the variable.")]
        [SerializeField] protected string m_VariableName;
        [Tooltip("Should the variable register for health change events?")]
        [SerializeField] protected bool m_HealthChangeRegistration;

        private Text m_Text;
        private SharedVariable m_Variable;

        /// <summary>
        /// Registers for the change callback on the variable.
        /// </summary>
        private void Start()
        {
            if (m_BehaviorTree == null) {
                return;
            }
            m_Text = GetComponent<Text>();

            if (string.IsNullOrEmpty(m_VariableName)) {
                var health = m_BehaviorTree.GetComponent<Health>();
                health.OnUpdateValue += OnHealthValueChanged;
                OnHealthValueChanged(health.Value);
            } else {
                m_Variable = m_BehaviorTree.GetVariable(m_VariableName);
                if (m_Variable == null) {
                    Debug.LogWarning($"Warning: Unable to find the variable {m_VariableName}.");
                    return;
                }

                m_Variable.OnValueChange += OnValueChanged;
            }
        }

        /// <summary>
        /// The variable value has changed.
        /// </summary>
        private void OnValueChanged()
        {
            m_Text.text = m_Label + m_Variable.GetValue();
        }

        /// <summary>
        /// The behavior tree may not be changing the values.
        /// </summary>
        /// <param name="amount">The new health amount.</param>
        private void OnHealthValueChanged(float amount)
        {
            m_Text.text = m_Label + amount;
        }

        /// <summary>
        /// The component has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (m_Variable != null) {
                m_Variable.OnValueChange -= OnValueChanged;
            } else if (m_BehaviorTree != null) {
                var health = m_BehaviorTree.GetComponent<Health>();
                health.OnUpdateValue -= OnHealthValueChanged;
            }
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// A simple UI that shows the health amount.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Tooltip("A reference to the health component that the healthbar represents.")]
        [SerializeField] protected Health m_Health;
        [Tooltip("A reference to the healthbar fill image.")]
        [SerializeField] protected UnityEngine.UI.Image m_Image;
        [Tooltip("The amount that the health amount is adjusted each update.")]
        [SerializeField] protected float m_AdjustmentAmount = 0.01f;

        private Coroutine m_HealthAdjustmentCoroutine;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Health.OnUpdateValue += OnUpdateHealth;
        }

        /// <summary>
        /// The health value has been updated.
        /// </summary>
        /// <param name="value">The new health value.</param>
        private void OnUpdateHealth(float value)
        {
            if (m_HealthAdjustmentCoroutine != null) {
                StopCoroutine(m_HealthAdjustmentCoroutine);
                m_HealthAdjustmentCoroutine = null;
            }
            m_HealthAdjustmentCoroutine = StartCoroutine(AdjustFillAmount(value / m_Health.MaxHealth));
        }

        /// <summary>
        /// Adjusts the fill amount by the adjustment amount.
        /// </summary>
        /// <param name="targetValue">The target fill value.</param>
        private IEnumerator AdjustFillAmount(float targetValue)
        {
            while (m_Image.fillAmount != targetValue) {
                m_Image.fillAmount += m_AdjustmentAmount * Mathf.Sign(targetValue - m_Image.fillAmount);
                if (Mathf.Abs(targetValue - m_Image.fillAmount) < m_AdjustmentAmount) {
                    m_Image.fillAmount = targetValue;
                }
                yield return new WaitForEndOfFrame();
            }
            m_HealthAdjustmentCoroutine = null;
        }

        /// <summary>
        /// The object has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            m_Health.OnUpdateValue -= OnUpdateHealth;
        }
    }
}
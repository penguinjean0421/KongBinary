/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Rotates the coin.
    /// </summary>
    public class Coin : MonoBehaviour
    {
        [Tooltip("The speed that the coin should rotate.")]
        [SerializeField] protected Vector3 m_RotationSpeed;

        private Transform m_Transform;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Updates the Transform rotation.
        /// </summary>
        private void Update()
        {
            m_Transform.rotation *= Quaternion.Euler(m_RotationSpeed * Time.deltaTime);
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Uses Rigidbody.MovePosition to move the Rigidbody.
    /// </summary>
    public class RigidbodyController : MonoBehaviour
    {
        [Tooltip("The amount that the rigidbody should move each FixedUpdate.")]
        [SerializeField] protected Vector3 m_MoveAmount;

        private Rigidbody m_Rigidbody;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Moves the Rigidbody.
        /// </summary>
        private void FixedUpdate()
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_MoveAmount * Time.fixedDeltaTime);
        }
    }
}
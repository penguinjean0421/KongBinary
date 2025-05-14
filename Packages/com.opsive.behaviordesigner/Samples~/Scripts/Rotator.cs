/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Rotates the transform according to the specified speed.
    /// </summary>
    public class Rotater : MonoBehaviour
    {
        [Tooltip("The speed that the object should rotate.")]
        [SerializeField] protected Vector3 m_RotateSpeed;

        /// <summary>
        /// Rotates the transform.
        /// </summary>
        public void Update()
        {
            transform.Rotate(m_RotateSpeed * Time.deltaTime);
        }
    }
}
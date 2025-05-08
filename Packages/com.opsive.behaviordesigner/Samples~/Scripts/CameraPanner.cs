/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Pans to camera to always have the target in view.
    /// </summary>
    public class CameraPanner : MonoBehaviour
    {
        [Tooltip("The object that the camera should follow.")]
        [SerializeField] protected Transform m_Target;
        [Tooltip("The speed that the camera should be panned.")]
        [SerializeField] protected float m_Speed = 5;

        private Transform m_Transform;

        private Quaternion m_StartTargetRotation;
        private Vector3 m_Offset;

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
        }

        /// <summary>
        /// Sets the camera offset.
        /// </summary>
        private void OnEnable()
        {
            m_StartTargetRotation = m_Target.rotation;
            m_Offset = m_Target.InverseTransformPoint(m_Transform.position);
        }

        /// <summary>
        /// Pans the camera.
        /// </summary>
        private void LateUpdate()
        {
            m_Transform.position = Vector3.MoveTowards(m_Transform.position, TransformPoint(m_Target.position, m_StartTargetRotation, m_Offset), m_Speed * Time.deltaTime);
        }

        /// <summary>
        /// Transforms the position from local space to world space. This is similar to Transform.TransformPoint but does not require a Transform.
        /// </summary>
        /// <param name="worldPosition">The world position of the object.</param>
        /// <param name="rotation">The world rotation of the object.</param>
        /// <param name="localPosition">The local position of the object.</param>
        /// <returns>The world space position.</returns>
        private static Vector3 TransformPoint(Vector3 worldPosition, Quaternion rotation, Vector3 localPosition)
        {
            return worldPosition + (rotation * localPosition);
        }
    }
}
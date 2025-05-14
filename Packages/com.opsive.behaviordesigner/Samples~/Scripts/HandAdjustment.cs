/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Adjusts the hand position according to the specified offset.
    /// This is used within the Hide and Seek scene allowing the agent to always point the flashlight forward instead of needing a specific animation.
    /// </summary>
    public class HandAdjustment : MonoBehaviour
    {
        [Tooltip("The limb that should be adjusted.")]
        [SerializeField] protected AvatarIKGoal m_Goal;
        [Tooltip("The offset from the character's forward rotation.")]
        [SerializeField] protected Vector3 m_Offset;

        private Animator m_Animator;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
        }

        /// <summary>
        /// Callback when the animator should update IK.
        /// </summary>
        /// <param name="layerIndex">The index of the layer being updated.</param>
        private void OnAnimatorIK(int layerIndex)
        {
            m_Animator.SetIKRotation(m_Goal, Quaternion.LookRotation(transform.forward) * Quaternion.Euler(m_Offset));
            m_Animator.SetIKRotationWeight(m_Goal, 1);
        }
    }
}
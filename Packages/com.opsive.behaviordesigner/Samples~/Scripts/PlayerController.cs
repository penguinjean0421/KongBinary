/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// A small player controller using the Character Collider component.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Tooltip("The name of the horizontal axis.")]
        [SerializeField] protected string m_HorizontalAxisName = "Horizontal";
        [Tooltip("The name of the vertical axis.")]
        [SerializeField] protected string m_VerticalAxisName = "Vertical";
        [Tooltip("The speed that the character should move.")]
        [SerializeField] protected float m_MoveSpeed = 2f;
        [Tooltip("The index of the state parameter.")]
        [SerializeField] protected int m_StateParameterValue;
        [Tooltip("The button mapping of the shift key.")]
        [SerializeField] protected string m_ShiftButtonMapping = "Fire3";
        [Tooltip("The multiplier to apply to the speed when the shift key is held down.")]
        [SerializeField] protected float m_ShiftSpeedMultiplier = 1.5f;

        private Transform m_Transform;
        private Animator m_Animator;
        private CharacterController m_CharacterController;

        /// <summary>
        /// Initializes the default value.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_CharacterController = GetComponent<CharacterController>();

            m_Animator.SetInteger("State", m_StateParameterValue);
        }

        /// <summary>
        /// Moves the character.
        /// </summary>
        private void Update()
        {
            var direction = new Vector3(Input.GetAxis(m_HorizontalAxisName), 0, Input.GetAxis(m_VerticalAxisName));
            var multiplier = (Input.GetButton(m_ShiftButtonMapping) ? m_ShiftSpeedMultiplier : 1);
            m_CharacterController.Move(direction * m_MoveSpeed * Time.deltaTime * multiplier);
            m_Transform.LookAt(m_Transform.position + direction);
            m_Animator.speed = multiplier;
            m_Animator.SetFloat("Forward", direction.magnitude);
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// An example showing how to synchronize NavMeshAgent movement with the Animator. This solution depends on the Animator having a "Forward" float Animator parameter.
    /// </summary>
    public class AnimationSynchronizer : MonoBehaviour
    {
        [Tooltip("Should root motion be used?")]
        [SerializeField] private bool m_UseRootMotion;
        [Tooltip("The NavMeshAgent stopping distance. Setting this value will prevent the agent from wobbling back and forth trying to arrive precisely on the destination.")]
        [SerializeField] private float m_StoppingDistance = 0.2f;
        [Tooltip("The multiplier to apply to the Forward Animator parameter value.")]
        [SerializeField] private float m_ForwardParameterMultiplier = 1f;
        [Tooltip("The damping time to reach the Forward Animator parameter value.")]
        [SerializeField] private float m_ForwardDampingTime = 0.01f;

        private Transform m_Transform;
        private Animator m_Animator;
        private NavMeshAgent m_NavMeshAgent;

        private int m_ForwardParameterHash = Animator.StringToHash("Forward");

        /// <summary>
        /// Initialize the default values.
        /// </summary>
        private void Awake()
        {
            m_Transform = transform;
            m_Animator = GetComponent<Animator>();
            m_NavMeshAgent = GetComponent<NavMeshAgent>();

            m_NavMeshAgent.stoppingDistance = m_StoppingDistance;
            m_NavMeshAgent.updatePosition = !m_UseRootMotion;
        }

        /// <summary>
        /// Updates the Animator value based on the NavMesh path.
        /// </summary>
        private void Update()
        {
            var localVelocity = Vector3.ProjectOnPlane(m_Transform.InverseTransformDirection(m_NavMeshAgent.velocity), Vector3.up).normalized;

            // Set the Forward parameter. This is specific to the sample Animator Controller.
            m_Animator.SetFloat(m_ForwardParameterHash, localVelocity.z * m_ForwardParameterMultiplier, m_ForwardDampingTime, Time.deltaTime);

            // If using root motion set the position within OnAnimatorMove.
            if (!m_UseRootMotion) {
                m_Transform.position = m_NavMeshAgent.nextPosition;
            }
        }

        /// <summary>
        /// Sets a new Forward Paramter Multiplier value.
        /// </summary>
        /// <param name="multiplier">The new value.</param>
        public void SetForwardMultiplier(float multiplier)
        {
            m_ForwardParameterMultiplier = multiplier;
        }

        /// <summary>
        /// Applies the root motion movement.
        /// </summary>
        private void OnAnimatorMove()
        {
            if (!m_UseRootMotion) {
                return;
            }

            m_Transform.position += m_Animator.deltaPosition;
            m_NavMeshAgent.nextPosition = m_Transform.position;
        }
    }
}
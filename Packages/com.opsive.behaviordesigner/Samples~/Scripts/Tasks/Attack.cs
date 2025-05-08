/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.BehaviorDesigner.Samples.SceneManagers;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Attacks the player.
    /// </summary>
    public class Attack: Action
    {
        [Tooltip("The agent that should attack.")]
        [RequireShared]
        [SerializeField] protected SharedVariable<GameObject> m_AttackAgent;

        private GameObject m_Player;
        private Health m_PlayerHealth;

        /// <summary>
        /// Initializes the values.
        /// </summary>
        public override void OnAwake()
        {
            var turnManager = GetComponent<TurnBasedSceneManager>();
            m_Player = turnManager.Player;
            m_PlayerHealth = m_Player.GetComponent<Health>();
        }

        /// <summary>
        /// Select an agent that should attack.
        /// </summary>
        /// <returns>Success if an agent was found.</returns>
        public override TaskStatus OnUpdate()
        {
            // The player has to be alive.
            if (m_PlayerHealth.Value <= 0) {
                return TaskStatus.Failure;
            }

            var controller = m_AttackAgent.Value.GetComponent<TurnBaseCharacterController>();
            controller.Attack(m_Player);

            return TaskStatus.Success;
        }
    }
}
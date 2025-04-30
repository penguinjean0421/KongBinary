/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples.SceneManagers
{
    using Opsive.BehaviorDesigner.Runtime;
    using UnityEngine;

    /// <summary>
    /// Manually ticks the behavior tree when it is the agent's turn.
    /// </summary>
    public class TurnBasedSceneManager : MonoBehaviour
    {
        [Tooltip("The player GameObject.")]
        [SerializeField] protected GameObject m_Player;
        [Tooltip("The agents against the player.")]
        [SerializeField] protected GameObject[] m_Agents;

        [Tooltip("Specifies the amount of time to wait in between turns.")]
        [SerializeField] protected float m_TurnDelay = 3;
        [Tooltip("The player instructions.")]
        [SerializeField] protected GameObject m_InstructionsUI;

        public GameObject Player { get => m_Player; }
        public GameObject[] Agents { get => m_Agents; }

        private bool m_PlayersTurn = false;
        private bool m_CanAttack = false;
        private BehaviorTree m_BehaviorTree;
        private TurnBaseCharacterController m_PlayerController;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public void Awake()
        {
            m_BehaviorTree = GetComponent<BehaviorTree>();
            m_PlayerController = m_Player.GetComponent<TurnBaseCharacterController>();

            enabled = false; // Enable when it is the agent's turn.
            StartTurn();
        }

        /// <summary>
        /// Alternates the turn between the player and the agent.
        /// </summary>
        public void StartTurn()
        {
            m_PlayersTurn = !m_PlayersTurn;

            if (m_PlayersTurn) {
                m_InstructionsUI.SetActive(true);
                m_CanAttack = true;
            } else {
                enabled = true;
            }
        }

        /// <summary>
        /// The specified character has been selected by the player.
        /// </summary>
        /// <param name="character">The selected character.</param>
        public void SelectedCharacter(GameObject character)
        {
            // Wait for the player's turn.
            if (!m_PlayersTurn || !m_CanAttack) {
                return;
            }

            // The player can't shoot themself.
            if (character == m_Player) {
                return;
            }

            m_PlayerController.Attack(character);
            m_CanAttack = false;
            Invoke("StartTurn", m_TurnDelay);
            m_InstructionsUI.SetActive(false);
        }

        /// <summary>
        /// Ticks the behavior tree.
        /// </summary>
        public void Update()
        {
            m_BehaviorTree.Tick();
        }

        /// <summary>
        /// The AI is done with their turn.
        /// </summary>
        public void EndAgentTurn()
        {
            enabled = false;

            Invoke("StartTurn", m_TurnDelay);
        }
    }
}
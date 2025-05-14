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
    /// Manages the melee combat scene.
    /// </summary>
    public class MeleeCombatSceneManager : MonoBehaviour
    {
        [Header("Agent")]
        [Tooltip("A reference to the behavior tree agent.")]
        [SerializeField] protected GameObject m_Agent;
        [Header("Opponent")]
        [Tooltip("A reference to the behavior tree opponent.")]
        [SerializeField] protected GameObject m_Opponent;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            var behaviorTree = m_Agent.GetComponent<BehaviorTree>();
            var sharedVariable = behaviorTree.GetVariable<GameObject>("Opponent");
            sharedVariable.Value = m_Opponent;

            behaviorTree = m_Opponent.GetComponent<BehaviorTree>();
            sharedVariable = behaviorTree.GetVariable<GameObject>("Opponent");
            sharedVariable.Value = m_Agent;
        }
    }
}
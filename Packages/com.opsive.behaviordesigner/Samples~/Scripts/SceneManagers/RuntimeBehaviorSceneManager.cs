/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Events;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Manages the runtime behavior scene.
    /// </summary>
    public class RuntimeBehaviorSceneManager : MonoBehaviour
    {
        [Header("Agent")]
        [Tooltip("A reference to the behavior tree agent.")]
        [SerializeField] protected GameObject m_Agent;
        [Tooltip("The location that the agent should train strength.")]
        [SerializeField] protected GameObject m_StrengthLocation;
        [Tooltip("The location that the agent should train speed.")]
        [SerializeField] protected GameObject m_SpeedLocation;
        [Tooltip("The location that the agent should fight.")]
        [SerializeField] protected GameObject m_FightLocation;
        [Tooltip("The left and right dumbbells.")]
        [SerializeField] protected GameObject[] m_Dumbbells;
        [Tooltip("The location the dumbbells should be equipped to.")]
        [SerializeField] protected GameObject[] m_DumbbellParents;
        [Tooltip("The location the dumbbells should be unequipped to.")]
        [SerializeField] protected GameObject[] m_DumbbellStorage;
        [Tooltip("The left and right boxing gloves.")]
        [SerializeField] protected GameObject[] m_BoxingGloves;
        [Header("Opponent")]
        [Tooltip("A reference to the behavior tree opponent.")]
        [SerializeField] protected GameObject m_Opponent;
        [Tooltip("The location that the opponent should fight.")]
        [SerializeField] protected GameObject m_OpponentFightLocation;
        [Tooltip("The left and right boxing gloves.")]
        [SerializeField] protected GameObject[] m_OpponentBoxingGloves;
        [Header("Camera")]
        [Tooltip("Specifies the camera locations.")]
        [SerializeField] protected GameObject[] m_CameraLocations;
        [Header("UI")]
        [Tooltip("A reference to the fight button.")]
        [SerializeField] protected GameObject m_FightButton;
        [Tooltip("A reference to the agent speed display.")]
        [SerializeField] protected Text m_AgentSpeed;
        [Tooltip("A reference to the agent strength display.")]
        [SerializeField] protected Text m_AgentStrength;
        [Tooltip("A reference to the agent health display.")]
        [SerializeField] protected Text m_AgentHealth;
        [Tooltip("A reference to the opponent health display.")]
        [SerializeField] protected Text m_OpponentHealth;

        private BehaviorTree m_AgentBehaviorTree;
        private BehaviorTree m_OpponentBehaviorTree;

        private SharedVariable<int> m_AgentSpeedVariable;
        private SharedVariable<int> m_AgentStrengthVariable;
        private SharedVariable<float> m_AgentHealthVariable;
        private SharedVariable<float> m_OpponentHealthVariable;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_AgentBehaviorTree = m_Agent.GetComponent<BehaviorTree>();
            SetVariableValue(m_AgentBehaviorTree, "StrengthLocation", m_StrengthLocation);
            SetVariableValue(m_AgentBehaviorTree, "SpeedLocation", m_SpeedLocation);
            SetVariableValue(m_AgentBehaviorTree, "LeftDumbbell", m_Dumbbells[0]);
            SetVariableValue(m_AgentBehaviorTree, "RightDumbbell", m_Dumbbells[1]);
            SetVariableValue(m_AgentBehaviorTree, "LeftHand", m_DumbbellParents[0]);
            SetVariableValue(m_AgentBehaviorTree, "RightHand", m_DumbbellParents[1]);
            SetVariableValue(m_AgentBehaviorTree, "LeftDumbbellStorage", m_DumbbellStorage[0]);
            SetVariableValue(m_AgentBehaviorTree, "RightDumbbellStorage", m_DumbbellStorage[1]);
            SetVariableValue(m_AgentBehaviorTree, "LeftGlove", m_BoxingGloves[0]);
            SetVariableValue(m_AgentBehaviorTree, "RightGlove", m_BoxingGloves[1]);
            SetVariableValue(m_AgentBehaviorTree, "FightLocation", m_FightLocation);

            EventHandler.RegisterEvent(m_AgentBehaviorTree, "OnChangeToStrengthLocation", () => { SetCameraLocation(CameraLocations.Dumbbells); });
            EventHandler.RegisterEvent(m_AgentBehaviorTree, "OnChangeToSpeedLocation", () => { SetCameraLocation(CameraLocations.HeavyBag); });

            m_OpponentBehaviorTree = m_Opponent.GetComponent<BehaviorTree>();
            SetVariableValue(m_OpponentBehaviorTree, "FightLocation", m_OpponentFightLocation);
            SetVariableValue(m_OpponentBehaviorTree, "LeftGlove", m_OpponentBoxingGloves[0]);
            SetVariableValue(m_OpponentBehaviorTree, "RightGlove", m_OpponentBoxingGloves[1]);

            m_AgentHealth.gameObject.SetActive(false);
            m_OpponentHealth.gameObject.SetActive(false);

            // Bind the health variable value to the health display.
#if UNITY_2022
            var sceneVariables = Object.FindAnyObjectByType<SceneSharedVariables>();
#else
            var sceneVariables = Object.FindFirstObjectByType<SceneSharedVariables>();
#endif
            m_AgentSpeedVariable = sceneVariables.GetVariable<int>("AgentSpeed");
            m_AgentSpeedVariable.OnValueChange += OnAgentSpeedChanged;
            m_AgentStrengthVariable = sceneVariables.GetVariable<int>("AgentStrength");
            m_AgentStrengthVariable.OnValueChange += OnAgentStrengthChanged;
            m_AgentHealthVariable = sceneVariables.GetVariable<float>("AgentHealth");
            m_AgentHealthVariable.OnValueChange += OnAgentHealthChanged;
            m_OpponentHealthVariable = sceneVariables.GetVariable<float>("OpponentHealth");
            m_OpponentHealthVariable.OnValueChange += OnOpponentHealthChanged;
        }

        /// <summary>
        /// Sets the variable value on the specified tree.
        /// </summary>
        /// <param name="behaviorTree">The behavior tree that should be set.</param>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="value">The value of the variable.</param>
        private void SetVariableValue<T>(BehaviorTree behaviorTree, string variableName, T value)
        {
            var sharedVariable = behaviorTree.GetVariable<T>(variableName);
            sharedVariable.Value = value;
        }

        /// <summary>
        /// The agent's speed value changed.
        /// </summary>
        private void OnAgentSpeedChanged()
        {
            m_AgentSpeed.text = "Agent Speed: " + m_AgentSpeedVariable.Value;
        }

        /// <summary>
        /// The agent's strength value changed.
        /// </summary>
        private void OnAgentStrengthChanged()
        {
            m_AgentStrength.text = "Agent Strength: " + m_AgentStrengthVariable.Value;
        }

        /// <summary>
        /// The agent's health value changed.
        /// </summary>
        private void OnAgentHealthChanged()
        {
            m_AgentHealth.text = "Agent Health: " + Mathf.Max(m_AgentHealthVariable.Value, 0);
        }

        /// <summary>
        /// The agent's health value changed.
        /// </summary>
        private void OnOpponentHealthChanged()
        {
            m_OpponentHealth.text = "Opponent Health: " + Mathf.Max(m_OpponentHealthVariable.Value, 0);
        }

        /// <summary>
        /// Training is done. Update the subtree references so the fight tree is used.
        /// </summary>
        public void Fight()
        {
            // The dumbbells should be unequipped.
            m_Dumbbells[0].SetActive(false);
            m_Dumbbells[1].SetActive(false);

            m_FightButton.SetActive(false);
            m_AgentSpeed.gameObject.SetActive(false);
            m_AgentStrength.gameObject.SetActive(false);
            m_AgentHealth.gameObject.SetActive(true);
            m_OpponentHealth.gameObject.SetActive(true);

            // The SubtreeReferenceSelector will use the SubtreeIndex in order to determine which subtree should be chosen.
            SetVariableValue(m_AgentBehaviorTree, "SubtreeIndex", 1);
            SetVariableValue(m_OpponentBehaviorTree, "SubtreeIndex", 1);
                        
            // Swap out the subtrees.
            m_AgentBehaviorTree.ReevaluateSubtreeReferences();
            m_OpponentBehaviorTree.ReevaluateSubtreeReferences();

            SetCameraLocation(CameraLocations.BoxingRing);
        }

        /// <summary>
        /// Specifies where the camera should be placed.
        /// </summary>
        public enum CameraLocations
        {
            HeavyBag,   // The camera should look at the heavy rack.
            Dumbbells,  // The camera should look at the dumbbell rack.
            BoxingRing, // The camera should look at the boxing ring.
        }

        /// <summary>
        /// Changes the camera location.
        /// </summary>
        /// <param name="cameraLocation">The location the camera should be placed.</param>
        private void SetCameraLocation(CameraLocations cameraLocation)
        {
            var index = (int)cameraLocation;
            Camera.main.transform.SetPositionAndRotation(m_CameraLocations[index].transform.position, m_CameraLocations[index].transform.rotation);
        }

        /// <summary>
        /// The agent has changed training locations.
        /// </summary>
        /// <param name="locationIndex"></param>
        private void OnChangeTrainingLocations(object locationIndex)
        {
            SetCameraLocation((CameraLocations)locationIndex);
        }
    }
}
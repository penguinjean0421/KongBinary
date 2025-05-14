/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples.SceneManagers
{
    using Opsive.BehaviorDesigner.Runtime;
    using System.IO;
    using UnityEngine;
    using UnityEngine.AI;
    using UnityEngine.UI;
    using static Opsive.BehaviorDesigner.Runtime.Utility.SaveManager;

    /// <summary>
    /// Saves and loads the behavior tree state.
    /// </summary>
    public class SaveLoadSceneManager : MonoBehaviour
    {
        [Tooltip("A reference to the behavior tree agent.")]
        [SerializeField] protected GameObject m_Agent;
        [Tooltip("The location that the behavior tree data should be saved.")]
        [SerializeField] protected string m_SaveLocation = "Assets/Opsive/BehaviorDesigner/Samples/SaveLoad.save";
        [Tooltip("A reference to the load button.")]
        [SerializeField] protected Button m_LoadButton;
        [Tooltip("A reference to the enemy agents.")]
        [SerializeField] protected GameObject[] m_EnemyAgents;
        [Tooltip("The location the enemies should move towards.")]
        [SerializeField] protected GameObject[] m_EnemyDestinations;

        private Transform m_Transform;
        private BehaviorTree m_BehaviorTree;
        private Animator m_Animator; 

        /// <summary>
        /// Cache the variables and start moving the enemies.
        /// </summary>
        private void Awake()
        {
            m_Transform = m_Agent.transform;
            m_BehaviorTree = m_Agent.GetComponent<BehaviorTree>();
            m_Animator = m_Agent.GetComponent<Animator>();
            m_BehaviorTree.SetVariableValue<GameObject[]>("Enemies", m_EnemyAgents);

            if (!File.Exists(m_SaveLocation)) {
                m_LoadButton.interactable = false;
            }

            for (int i = 0; i < m_EnemyAgents.Length; ++i) {
                m_EnemyAgents[i].GetComponent<NavMeshAgent>().SetDestination(m_EnemyDestinations[Random.Range(0, m_EnemyDestinations.Length)].transform.position);
            }
        }

        /// <summary>
        /// Stores all of the agent save data in one structure.
        /// </summary>
        private struct AgentSaveData
        {
            public SaveData BehaviorTreeSaveData;           // The behavior tree save data.
            public int TargetIndex;                         // The index of the Target SharedVariable.
            public int AnimationStateHash;                  // The current Animator state.
            public int AnimationParameter;                  // The Animator "State" parameter value.
            public Vector3[] EnemyPositions;                // The enemy's position.
            public Quaternion[] EnemyRotations;             // The enemy's rotation.
            public int[] EnemyAnimationStateHashes;         // The enemy's Animator state.
            public float[] EnemyAnimationNormalizedTimes;   // The enemy's Animator playback time.
            public float[] EnemyAnimationForwardParameters; // The enemy's Animator "Forward" parameter value.
            public int[] EnemyAnimationStateParameters;     // The enemy's Animator "State" parameter value.
            public bool[] EnemyIsStopped;                   // True if the enemy's NavmeshAgent is stopped.
            public Vector3[] EnemyDestinations;             // The destination of the enemy.
            public float CameraAnimationTime;               // The camera's current animation time.
        }

        /// <summary>
        /// Save the agent state.
        /// </summary>
        public void Save()
        {
            var saveData = m_BehaviorTree.Save(); // Don't use BehaviorTree.Save(FilePath) because it will only save the BehaviorTree data to the file path.
            if (!saveData.HasValue) {
                return;
            }

            var target = m_BehaviorTree.GetVariable<GameObject>("Target");
            var targetIndex = -1;
            for (int i = 0; i < m_EnemyAgents.Length; ++i) {
                if (target.Value == m_EnemyAgents[i]) {
                    targetIndex = i;
                    break;
                }
            }

            var enemyPositions = new Vector3[m_EnemyAgents.Length];
            var enemyRotations = new Quaternion[m_EnemyAgents.Length];
            var enemyAnimationStateHashes = new int[m_EnemyAgents.Length];
            var enemyAnimationNormalizedTimes = new float[m_EnemyAgents.Length];
            var enemyAnimationForwardParameters = new float[m_EnemyAgents.Length];
            var enemyAnimationStateParameters = new int[m_EnemyAgents.Length];
            var enemyIsStopped = new bool[m_EnemyAgents.Length];
            var enemyDestinations = new Vector3[m_EnemyAgents.Length];
            for (int i = 0; i < m_EnemyAgents.Length; ++i) {
                enemyPositions[i] = m_EnemyAgents[i].transform.position;
                enemyRotations[i] = m_EnemyAgents[i].transform.rotation;
                enemyAnimationStateHashes[i] = m_EnemyAgents[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash;
                enemyAnimationNormalizedTimes[i] = m_EnemyAgents[i].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime;
                enemyAnimationForwardParameters[i] = m_EnemyAgents[i].GetComponent<Animator>().GetFloat("Forward");
                enemyAnimationStateParameters[i] = m_EnemyAgents[i].GetComponent<Animator>().GetInteger("State");
                enemyDestinations[i] = m_EnemyAgents[i].GetComponent<NavMeshAgent>().destination;
                enemyIsStopped[i] = m_EnemyAgents[i].GetComponent<NavMeshAgent>().isStopped;
            }

            // Create the data structure which contains all of the values that should be saved.
            var agentSaveData = new AgentSaveData() { BehaviorTreeSaveData = saveData.Value, TargetIndex = targetIndex, 
                                                      AnimationStateHash = m_Animator.GetCurrentAnimatorStateInfo(0).fullPathHash, AnimationParameter = m_Animator.GetInteger("State"),
                                                      EnemyPositions = enemyPositions, EnemyRotations = enemyRotations, EnemyAnimationStateHashes = enemyAnimationStateHashes,
                                                      EnemyAnimationNormalizedTimes = enemyAnimationNormalizedTimes, EnemyAnimationForwardParameters = enemyAnimationForwardParameters, 
                                                      EnemyAnimationStateParameters = enemyAnimationStateParameters, EnemyIsStopped = enemyIsStopped, 
                                                      EnemyDestinations = enemyDestinations, CameraAnimationTime = Camera.main.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime };

            if (File.Exists(m_SaveLocation)) {
                File.Delete(m_SaveLocation);
            }
            try {
                if (!Directory.Exists(Path.GetDirectoryName(m_SaveLocation))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(m_SaveLocation));
                }
                var fileStream = File.Create(m_SaveLocation);
                using (var streamWriter = new StreamWriter(fileStream)) {
                    streamWriter.Write(JsonUtility.ToJson(agentSaveData));
                }
                fileStream.Close();
            } catch (System.Exception e) {
                Debug.LogException(e);
                return;
            }
            m_LoadButton.interactable = true;
        }

        /// <summary>
        /// Load the agent state.
        /// </summary>
        public void Load()
        {
            if (!File.Exists(m_SaveLocation)) {
                return;
            }

            AgentSaveData agentSaveData;
            var fileStream = File.Open(m_SaveLocation, FileMode.Open);
            using (var streamReader = new StreamReader(fileStream)) {
                var fileData = streamReader.ReadToEnd();
                agentSaveData = JsonUtility.FromJson<AgentSaveData>(fileData);
            }
            fileStream.Close();

            // Restore the values.
            m_BehaviorTree.Load(agentSaveData.BehaviorTreeSaveData, (BehaviorTree tree) =>
            {
                // Scene objects cannot be persisted to a file. Restore the scene values before the tasks are restored.
                m_BehaviorTree.SetVariableValue<GameObject[]>("Enemies", m_EnemyAgents);
                if (agentSaveData.TargetIndex != -1) {
                    m_BehaviorTree.SetVariableValue("Target", m_EnemyAgents[agentSaveData.TargetIndex]);
                }
            });

            m_Animator.Play(agentSaveData.AnimationStateHash, 0);
            m_Animator.SetInteger("State", agentSaveData.AnimationParameter);

            // Restore the enemy agent values.
            for (int i = 0; i < m_EnemyAgents.Length; ++i) {
                m_EnemyAgents[i].transform.SetPositionAndRotation(agentSaveData.EnemyPositions[i], agentSaveData.EnemyRotations[i]);
                m_EnemyAgents[i].GetComponent<Animator>().Play(agentSaveData.EnemyAnimationStateHashes[i], 0, agentSaveData.EnemyAnimationNormalizedTimes[i]);
                m_EnemyAgents[i].GetComponent<Animator>().SetFloat("Forward", agentSaveData.EnemyAnimationForwardParameters[i]);
                m_EnemyAgents[i].GetComponent<Animator>().SetInteger("State", agentSaveData.EnemyAnimationStateParameters[i]);
                m_EnemyAgents[i].GetComponent<NavMeshAgent>().isStopped = agentSaveData.EnemyIsStopped[i];
                if (!agentSaveData.EnemyIsStopped[i]) {
                    m_EnemyAgents[i].GetComponent<NavMeshAgent>().SetDestination(agentSaveData.EnemyDestinations[i]);
                }
                m_EnemyAgents[i].GetComponent<Health>().Value = agentSaveData.EnemyIsStopped[i] ? 0 : 100;
            }

            Camera.main.GetComponent<Animator>().Play(Camera.main.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).fullPathHash, 0, agentSaveData.CameraAnimationTime);
        }
    }
}
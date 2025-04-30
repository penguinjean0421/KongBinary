/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples.SceneManagers
{
    using Opsive.BehaviorDesigner.Runtime;
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// Manages the Hide and Seek scene.
    /// </summary>
    public class HideSeekSceneManager : MonoBehaviour
    {
        [Tooltip("A reference to the agent prefab.")]
        [SerializeField] protected GameObject m_AgentPrefab;
        [Tooltip("The locations that the agent should spawn.")]
        [SerializeField] protected GameObject[] m_SpawnLocations;
        [Tooltip("The locations the agents should patrol.")]
        [SerializeField] protected GameObject[] m_PatrolWaypoints;
        [Tooltip("A reference to the player.")]
        [SerializeField] protected GameObject m_Player;
        [Tooltip("The objects that should be reset.")]
        [SerializeField] protected GameObject[] m_ResetObjects;

        private GameObject[] m_SpawnedAgents;
        private Vector3[] m_ResetPositions;
        private Quaternion[] m_ResetRotations;

        /// <summary>
        /// Spawn the agents.
        /// </summary>
        private void Awake()
        {
            m_SpawnedAgents = new GameObject[m_SpawnLocations.Length];
            for (int i = 0; i < m_SpawnLocations.Length; ++i) {
                m_SpawnedAgents[i] = Object.Instantiate(m_AgentPrefab, m_SpawnLocations[i].transform.position, m_SpawnLocations[i].transform.rotation);

                var behaviorTree = m_SpawnedAgents[i].GetComponent<BehaviorTree>();
                var playerVariable = behaviorTree.GetVariable<GameObject>("Player");
                playerVariable.Value = m_Player;
                var waypointsVariable = behaviorTree.GetVariable<GameObject[]>("Waypoints");
                waypointsVariable.Value = m_PatrolWaypoints;
            }

            m_ResetPositions = new Vector3[m_ResetObjects.Length];
            m_ResetRotations = new Quaternion[m_ResetObjects.Length];

            for (int i = 0; i < m_ResetPositions.Length; ++i) {
                m_ResetPositions[i] = m_ResetObjects[i].transform.position;
                m_ResetRotations[i] = m_ResetObjects[i].transform.rotation;
            }
        }

        /// <summary>
        /// Restarts the scene.
        /// </summary>
        public void Restart()
        {
            for (int i = 0; i < m_ResetPositions.Length; ++i) {
                m_ResetObjects[i].transform.SetPositionAndRotation(m_ResetPositions[i], m_ResetRotations[i]);
                Animator animator;
                if ((animator = m_ResetObjects[i].GetComponent<Animator>()) != null) {
                    animator.SetFloat("Forward", 0);
                }
            }
            for (int i = 0; i < m_SpawnedAgents.Length; ++i) {
                m_SpawnedAgents[i].transform.SetPositionAndRotation(m_SpawnLocations[i].transform.position, m_SpawnLocations[i].transform.rotation);
                m_SpawnedAgents[i].GetComponent<NavMeshAgent>().Warp(m_SpawnLocations[i].transform.position);
            }
        }
    }
}
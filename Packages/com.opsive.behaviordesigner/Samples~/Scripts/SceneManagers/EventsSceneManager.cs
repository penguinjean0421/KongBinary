/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples.SceneManagers
{
    using Opsive.BehaviorDesigner.Runtime;
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Manages the Events scene.
    /// </summary>
    public class EventsSceneManager : MonoBehaviour
    {
        [Tooltip("A prefab of the behavior tree agent.")]
        [SerializeField] protected GameObject m_AgentPrefab;
        [Tooltip("The number of agents that should be spawned.")]
        [SerializeField] protected int m_SpawnCount;
        [Tooltip("The center of the spawn.")]
        [SerializeField] protected GameObject m_SpawnCenter;
        [Tooltip("The spawn area.")]
        [SerializeField] protected Vector2 m_SpawnSize;
        [Tooltip("The location the agent should move to. The agent will randomly choose one destination.")]
        [SerializeField] protected GameObject[] m_Destinations;
        [Tooltip("A reference to the turret.")]
        [SerializeField] protected Turret m_Turret;

        /// <summary>
        /// Spawns the agents.
        /// </summary>
        public void Awake()
        {
            var agentHealths = new List<Health>();
            for (int i = 0; i < m_SpawnCount; ++i) {
                var position = m_SpawnCenter.transform.position + new Vector3(Random.Range(-m_SpawnSize.x, m_SpawnSize.x), 0, Random.Range(-m_SpawnSize.y, m_SpawnSize.y));
                var agent = Object.Instantiate(m_AgentPrefab, position, m_SpawnCenter.transform.rotation);
                agent.name = m_AgentPrefab.name + (i + 1);
                
                // Set the scene variables.
                var behaviorTree = agent.GetComponent<BehaviorTree>();
                var destinationVariable = behaviorTree.GetVariable<GameObject>("Destination");
                destinationVariable.Value = m_Destinations[Random.Range(0, m_Destinations.Length)];

                agentHealths.Add(agent.GetComponent<Health>());
            }

            m_Turret.Targets = agentHealths;
        }
    }
}
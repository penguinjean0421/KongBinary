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
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Selects an agent out of the agent list.
    /// </summary>
    public class SelectAgent : Action
    {
        [Tooltip("The possible GameObjects")]
        [SerializeField] protected SharedVariable<GameObject[]> m_Agents;
        [Tooltip("The agent that should attack.")]
        [RequireShared]
        [SerializeField] protected SharedVariable<GameObject> m_Agent;
        [Tooltip("If specified, the agent that is closest to the specified GameObject will be selected.")]
        [SerializeField] protected SharedVariable<GameObject> m_ClosestToTarget;

        private List<Health> m_AgentHealth;
        private List<Health> m_ActiveAgents;

        /// <summary>
        /// Initializes the values.
        /// </summary>
        public override void OnAwake()
        {
            m_AgentHealth = new List<Health>();
            for (int i = 0; i < m_Agents.Value.Length; ++i) {
                m_AgentHealth.Add(m_Agents.Value[i].GetComponent<Health>());
            }
            if (m_ClosestToTarget.Value == null) {
                m_ActiveAgents = new List<Health>();
            }
        }

        /// <summary>
        /// Select an agent that should attack.
        /// </summary>
        /// <returns>Success if an agent was found.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_ClosestToTarget.Value != null) {
                var closestToDistance = float.MaxValue;
                float distance;
                var closestToTargetPosition = m_ClosestToTarget.Value.transform.position;
                for (int i = 0; i < m_AgentHealth.Count; ++i) {
                    if (m_AgentHealth[i].Value > 0 && (distance = (m_AgentHealth[i].transform.position - closestToTargetPosition).sqrMagnitude) < closestToDistance) {
                        closestToDistance = distance;
                        m_Agent.Value = m_AgentHealth[i].gameObject;
                    }
                }
                return closestToDistance != float.MaxValue ? TaskStatus.Success : TaskStatus.Failure;
            }

            // Randomly select an agent that is alive.
            m_ActiveAgents.Clear();
            for (int i = 0; i < m_AgentHealth.Count; ++i) {
                if (m_AgentHealth[i].Value > 0) {
                    m_ActiveAgents.Add(m_AgentHealth[i]);
                }
            }

            if (m_ActiveAgents.Count == 0) {
                return TaskStatus.Failure;
            }

            // Only those remaining are alive.
            var randomIndex = Random.Range(0, m_ActiveAgents.Count);
            m_Agent.Value = m_ActiveAgents[randomIndex].gameObject;
            return TaskStatus.Success;
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// Uses the NavMeshAgent to seek to the specified target.
    /// This task is basic intended for demo purposes. For a more complete task see the Movement Pack:
    /// https://assetstore.unity.com/packages/slug/310243
    /// </summary>
    public class Seek : Action
    {
        [Tooltip("The seek destination.")]
        [SerializeField] protected SharedVariable<GameObject> m_Destination;

        private NavMeshAgent m_Agent;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            m_Agent = GetComponent<NavMeshAgent>();
        }

        /// <summary>
        /// Sets the NavMesh destination.
        /// </summary>
        public override void OnStart()
        {
            m_Agent.isStopped = false;
            m_Agent.SetDestination(m_Destination.Value.transform.position);
        }

        /// <summary>
        /// Returns success when the agent has arrived.
        /// </summary>
        /// <returns>Success when the agent has arrived.</returns>
        public override TaskStatus OnUpdate()
        {
            if (!m_Agent.pathPending && m_Agent.remainingDistance <= m_Agent.stoppingDistance) {
                return TaskStatus.Success;
            }

            if (m_Agent.destination != m_Destination.Value.transform.position) {
                m_Agent.SetDestination(m_Destination.Value.transform.position);
            }

            return TaskStatus.Running;
        }

        /// <summary>
        /// The task has ended.
        /// </summary>
        public override void OnEnd()
        {
            if (m_Agent.isOnNavMesh) {
                m_Agent.isStopped = true;
            }
        }
    }
}
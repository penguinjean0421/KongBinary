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
    /// Uses the NavMeshAgent to patrol through a set of waypoints.
    /// This task is basic intended for demo purposes. For a more complete task see the Movement Pack:
    /// https://assetstore.unity.com/packages/slug/310243
    /// </summary>
    public class Patrol : Action
    {
        [Tooltip("The patrol waypoints.")]
        [SerializeField] protected SharedVariable<GameObject[]> m_Waypoints;
        [Tooltip("Should the starting waypoint be randomized?")]
        [SerializeField] protected SharedVariable<bool> m_RandomStart;

        private NavMeshAgent m_Agent;

        private int m_Index;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            m_Agent = GetComponent<NavMeshAgent>();

            m_Waypoints.OnValueChange += WaypointsChanged;
        }

        /// <summary>
        /// The Waypoints SharedVariable has changed.
        /// </summary>
        private void WaypointsChanged()
        {
            m_Index = (m_Index + 1) % m_Waypoints.Value.Length;
            m_Agent.SetDestination(m_Waypoints.Value[m_Index].transform.position);
        }

        /// <summary>
        /// Sets the NavMesh destination.
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();

            if (m_Waypoints.Value.Length == 0) {
                return;
            }

            m_Index = m_RandomStart.Value ? Random.Range(0, m_Waypoints.Value.Length - 1) : 0;
            m_Agent.isStopped = false;
            m_Agent.SetDestination(m_Waypoints.Value[m_Index].transform.position);
        }

        /// <summary>
        /// Patrols the waypoints. Always returns running.
        /// </summary>
        /// <returns>A status of running.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Waypoints.Value.Length == 0) {
                return TaskStatus.Failure;
            }
            if (!m_Agent.pathPending && m_Agent.remainingDistance <= m_Agent.stoppingDistance) {
                m_Index = (m_Index + 1) % m_Waypoints.Value.Length;
                m_Agent.SetDestination(m_Waypoints.Value[m_Index].transform.position);
            }
            return TaskStatus.Running;
        }

        /// <summary>
        /// The task has ended.
        /// </summary>
        public override void OnEnd()
        {
            if (!m_Agent.isOnNavMesh) {
                return;
            }
            m_Agent.isStopped = true;
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            m_Waypoints.OnValueChange -= WaypointsChanged;
        }
    }
}
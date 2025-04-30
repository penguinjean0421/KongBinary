/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.GraphDesigner.Runtime;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [NodeDescription("Uses DOTS to rotate around the center. This task will always return a status of running.")]
    public struct Swarm : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The angular speed of the agent.")]
        [SerializeField] float m_AngularSpeed;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(SwarmTag); }
        public System.Type SystemType { get => typeof(SwarmTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_AngularSpeed = 2; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<SwarmComponent> buffer;
            if (world.EntityManager.HasBuffer<SwarmComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SwarmComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<SwarmComponent>(entity);
            }

            buffer.Add(new SwarmComponent()
            {
                Index = RuntimeIndex,
                AngularSpeed = m_AngularSpeed,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<SwarmComponent> buffer;
            if (world.EntityManager.HasBuffer<SwarmComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SwarmComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Swarm struct.
    /// </summary>
    public struct SwarmComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The angular speed of the agent.")]
        public float AngularSpeed;
    }

    /// <summary>
    /// A DOTS tag indicating when a Swarm node is active.
    /// </summary>
    public struct SwarmTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Swarm logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct SwarmTaskSystem : ISystem
    {
        private EntityQuery m_SwarmQuery;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_SwarmQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TaskComponent>().WithAllRW<LocalTransform>()
                .WithAll<SwarmComponent, SwarmTag, EvaluationComponent>()
                .Build(ref state);
        }

        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = new SwarmJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(m_SwarmQuery, state.Dependency);
        }

        /// <summary>
        /// Rotates around the center.
        /// </summary>
        [BurstCompile]
        private partial struct SwarmJob : IJobEntity
        {
            [Tooltip("The current frame's DeltaTime.")]
            public float DeltaTime;

            /// <summary>
            /// Updates the logic.
            /// </summary>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="swarmComponents">An array of SwarmComponents.</param>
            /// <param name="transform">The entity's transform.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<SwarmComponent> swarmComponents, ref LocalTransform transform)
            {
                for (int i = 0; i < swarmComponents.Length; ++i) {
                    var swarmComponent = swarmComponents[i];
                    var taskComponent = taskComponents[swarmComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;

                        taskComponents[swarmComponent.Index] = taskComponent;
                    }

                    // Always swarm.
                    if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    var dist = math.length(transform.Position);
                    var radians = GetAngle(transform.Position);

                    radians += swarmComponent.AngularSpeed * DeltaTime;

                    transform.Position.x = math.cos(radians) * dist;
                    transform.Position.z = math.sin(radians) * dist;
                }
            }

            /// <summary>
            /// Returns the angle between the target point and the center.
            /// </summary>
            /// <param name="target">The target point.</param>
            /// <returns>The angle between the target point and the center. This will be in the range of 0 - 2PI (radians).</returns>
            [BurstCompile]
            private float GetAngle(float3 target)
            {
                var n = 270 - (math.atan2(-target.x, -target.z)) * 180 / math.PI;
                return (n % 360) * math.TORADIANS;
            }
        }
    }
}
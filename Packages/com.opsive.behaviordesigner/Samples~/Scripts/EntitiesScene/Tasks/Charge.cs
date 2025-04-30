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
    using System;

    [NodeDescription("Uses DOTS to move towards the center point, returns success when the agent is less than the arrive distance.")]
    public struct Charge : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The speed of the agent.")]
        [SerializeField] float m_Speed;
        [Tooltip("The distance away from the target when the agent has arrived at the target.")]
        [SerializeField] float m_ArriveDistance;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(ChargeTag); }
        public Type SystemType { get => typeof(ChargeTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_Speed = 10; m_ArriveDistance = 0.1f; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<ChargeComponent> buffer;
            if (world.EntityManager.HasBuffer<ChargeComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ChargeComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<ChargeComponent>(entity);
            }

            buffer.Add(new ChargeComponent()
            {
                Index = RuntimeIndex,
                Speed = m_Speed,
                ArriveDistance = m_ArriveDistance,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<ChargeComponent> buffer;
            if (world.EntityManager.HasBuffer<ChargeComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ChargeComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Charge struct.
    /// </summary>
    public struct ChargeComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The speed of the agent.")]
        public float Speed;
        [Tooltip("The distance away from the target when the agent has arrived at the target.")]
        public float ArriveDistance;
    }

    /// <summary>
    /// A DOTS tag indicating when a Charge node is active.
    /// </summary>
    public struct ChargeTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Charge logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct ChargeTaskSystem : ISystem
    {
        private EntityQuery m_ChargeQuery;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_ChargeQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TaskComponent>().WithAllRW<LocalTransform>()
                .WithAll<ChargeComponent, EvaluationComponent>()
                .Build(ref state);
        }

        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            state.Dependency = new ChargeJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(m_ChargeQuery, state.Dependency);
        }

        /// <summary>
        /// Charges towards the center.
        /// </summary>
        [BurstCompile]
        private partial struct ChargeJob : IJobEntity
        {
            [Tooltip("The current frame's DeltaTime.")]
            public float DeltaTime;

            /// <summary>
            /// Updates the logic.
            /// </summary>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="chargeComponents">An array of ChargeComponents.</param>
            /// <param name="transform">The entity's transform.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<ChargeComponent> chargeComponents, ref LocalTransform transform)
            {
                for (int i = 0; i < chargeComponents.Length; ++i) {
                    var chargeComponent = chargeComponents[i];
                    var taskComponent = taskComponents[chargeComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[chargeComponent.Index] = taskComponent;
                    }

                    if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The task should return success as soon as the agent has arrived.
                    var direction = -transform.Position;
                    if (math.length(direction) < chargeComponent.ArriveDistance) {
                        taskComponent.Status = TaskStatus.Success;
                        taskComponents[chargeComponent.Index] = taskComponent;

                        continue;
                    }

                    // The entity hasn't arrived yet - keep moving to the center.
                    transform.Position += (direction * chargeComponent.Speed * DeltaTime);
                }
            }
        }
    }
}
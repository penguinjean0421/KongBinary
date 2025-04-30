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
    using Unity.Transforms;
    using UnityEngine;

    [NodeDescription("Destroys the Entity.")]
    public struct Destroy : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(DestroyTag); }
        public System.Type SystemType { get => typeof(DestroyTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<DestroyTaskComponent> buffer;
            if (world.EntityManager.HasBuffer<DestroyTaskComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<DestroyTaskComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<DestroyTaskComponent>(entity);
            }

            buffer.Add(new DestroyTaskComponent()
            {
                Index = RuntimeIndex,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<DestroyTaskComponent> buffer;
            if (world.EntityManager.HasBuffer<DestroyTaskComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<DestroyTaskComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Destroy struct.
    /// </summary>
    public struct DestroyTaskComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a Destroy node is active.
    /// </summary>
    public struct DestroyTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Destroy logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct DestroyTaskSystem : ISystem
    {
        private EntityQuery m_DestroyQuery;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_DestroyQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TaskComponent>().WithAllRW<LocalTransform>()
                .WithAll<DestroyTaskComponent, EvaluationComponent>()
                .Build(ref state);
        }

        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (destroyComponents, taskComponents, entity) in
                SystemAPI.Query<DynamicBuffer<DestroyTaskComponent>, DynamicBuffer<TaskComponent>>().WithAll<DestroyTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < destroyComponents.Length; ++i) {
                    var destroyComponent = destroyComponents[i];
                    var taskComponent = taskComponents[destroyComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    // The DestroyEntityTag will destroy the entity.
                    ecb.AddComponent<DestroyEntityTag>(entity);

                    // The task always returns success.
                    taskComponent.Status = TaskStatus.Success;
                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[destroyComponent.Index] = taskComponent;
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.GraphDesigner.Runtime;
    using System;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;

    [NodeDescription("Uses DOTS to determine if the entity has a target.")]
    public struct FindTarget : ILogicNode, ITaskComponentData, IAction
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

        public ComponentType Tag { get => typeof(FindTargetTag); }
        public System.Type SystemType { get => typeof(FindTargetTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<FindTargetComponent> buffer;
            if (world.EntityManager.HasBuffer<FindTargetComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<FindTargetComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<FindTargetComponent>(entity);
            }

            buffer.Add(new FindTargetComponent()
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
            DynamicBuffer<FindTargetComponent> buffer;
            if (world.EntityManager.HasBuffer<FindTargetComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<FindTargetComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the FindTarget struct.
    /// </summary>
    public struct FindTargetComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a FindTarget node is active.
    /// </summary>
    public struct FindTargetTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the FindTarget logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct FindTargetTaskSystem : ISystem
    {
        Unity.Mathematics.Random randomGenerator;

        /// <summary>
        /// The system has been created.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            randomGenerator = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks);
        }

        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (transform, taskComponents, findTargetComponents) in
                SystemAPI.Query<RefRW<LocalTransform>, DynamicBuffer<TaskComponent>, DynamicBuffer<FindTargetComponent>>().WithAll<FindTargetTag, EvaluationComponent>()) {
                for (int i = 0; i < findTargetComponents.Length; ++i) {
                    var fndTargetComponent = findTargetComponents[i];
                    var taskComponent = taskComponents[fndTargetComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    var index = -1;
                    var count = 0;
                    var entities = state.EntityManager.GetAllEntities(Unity.Collections.Allocator.Temp);
                    var foundAgent = false;
                    if (entities.Length > 0) {
                        do {
                            index = randomGenerator.NextInt(entities.Length);
                            count++;
                        } while (count < entities.Length * 2 && !(foundAgent = state.EntityManager.HasComponent<AgentTag>(entities[index])));
                    }

                    // A new target has been found. Add the TargetEntityTag.
                    if (foundAgent) {
                        ecb.AddComponent<TargetEntityTag>(entities[index]);
                    }
                    entities.Dispose();

                    // The task is complete, return to the parent.
                    taskComponent.Status = foundAgent ? TaskStatus.Success : TaskStatus.Failure;
                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[fndTargetComponent.Index] = taskComponent;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
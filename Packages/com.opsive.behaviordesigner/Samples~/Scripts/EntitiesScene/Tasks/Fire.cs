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
    using Unity.Entities;
    using Unity.Transforms;
    using UnityEngine;
    using System;

    [Tooltip("Fires any entity that has the HealthComponent.")]
    public struct Fire : ILogicNode, ITaskComponentData, IAction
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

        public ComponentType Tag { get => typeof(FireTag); }
        public Type SystemType { get => typeof(FireTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<FireComponent> buffer;
            if (world.EntityManager.HasBuffer<FireComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<FireComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<FireComponent>(entity);
            }
            buffer.Add(new FireComponent()
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
            DynamicBuffer<FireComponent> buffer;
            if (world.EntityManager.HasBuffer<FireComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<FireComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Fire struct.
    /// </summary>
    public struct FireComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a Fire node is active.
    /// </summary>
    public struct FireTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Fire logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct FireTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (taskComponents, fireComponents, entity) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<FireComponent>>().WithAll<FireTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < fireComponents.Length; ++i) {
                    var fireComponent = fireComponents[i];
                    var taskComponent = taskComponents[fireComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    // Find the target. There will only be one entity with the TargetEntityTag.
                    foreach (var (target, localTransform, targetEntity) in SystemAPI.Query<RefRO<TargetEntityTag>, RefRO<LocalTransform>>().WithEntityAccess()) {
                        ecb.AddComponent<DestroyEntityTag>(targetEntity);
                        break;
                    }

                    // The task will always return immediately.
                    taskComponent.Status = TaskStatus.Success;
                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[fireComponent.Index] = taskComponent;

                    // The turret has fired - apply a recoil.
                    foreach (var (target, turretEntity) in SystemAPI.Query<RefRO<TurretRecoil>>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess()) {
                        ecb.SetComponentEnabled<TurretRecoil>(turretEntity, true);
                        break;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
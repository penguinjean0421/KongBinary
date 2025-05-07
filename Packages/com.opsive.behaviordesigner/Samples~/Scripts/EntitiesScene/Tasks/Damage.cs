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
    using Unity.Collections;
    using Unity.Transforms;
    using UnityEngine;
    using System;

    [Tooltip("Damages any entity that has the HealthComponent.")]
    public struct Damage : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The amount of damage to apply.")]
        [SerializeField] float m_DamageAmount;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(DamageTag); }
        public System.Type SystemType { get => typeof(DamageTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_DamageAmount = 1; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<DamageComponent> buffer;
            if (world.EntityManager.HasBuffer<DamageComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<DamageComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<DamageComponent>(entity);
            }
            buffer.Add(new DamageComponent()
            {
                Index = RuntimeIndex,
                DamageAmount = m_DamageAmount,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<DamageComponent> buffer;
            if (world.EntityManager.HasBuffer<DamageComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<DamageComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Damage struct.
    /// </summary>
    public struct DamageComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The amount of damage to apply.")]
        public float DamageAmount;
    }

    /// <summary>
    /// A DOTS tag indicating when a Fire node is active.
    /// </summary>
    public struct DamageTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Damage logic.
    /// </summary>
    [DisableAutoCreation]
    public partial class DamageTaskSystem : SystemBase
    {
        public Action<float> OnDamage;

        private EntityQuery m_DamageQuery;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        protected override void OnCreate()
        {
            m_DamageQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<TaskComponent>().WithAllRW<LocalTransform>()
                .WithAll<DamageComponent, EvaluationComponent>()
                .Build(EntityManager);
        }

        /// <summary>
        /// Updates the logic.
        /// </summary>
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(WorldUpdateAllocator);
            foreach (var (taskComponents, damageComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<DamageComponent>>().WithAll<DamageTag, EvaluationComponent>()) {
                for (int i = 0; i < damageComponents.Length; ++i) {
                    var damageComponent = damageComponents[i];
                    var taskComponent = taskComponents[damageComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    // Apply the damage.
                    foreach (var (healthComponent, entity) in SystemAPI.Query<RefRW<HealthComponent>>().WithEntityAccess()) {
                        healthComponent.ValueRW.Value -= damageComponent.DamageAmount;

                        if (healthComponent.ValueRO.Value <= 0) {
                            ecb.AddComponent<DestroyEntityTag>(entity);
                        }
                        OnDamage?.Invoke(healthComponent.ValueRO.Value);
                    }

                    // The task will always return immediately.
                    taskComponent.Status = TaskStatus.Success;
                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[damageComponent.Index] = taskComponent;
                }
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
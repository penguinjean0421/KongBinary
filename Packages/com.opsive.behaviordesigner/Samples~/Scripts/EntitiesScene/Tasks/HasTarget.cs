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
    using Unity.Transforms;
    using UnityEngine;

    [NodeDescription("Uses DOTS to determine if the entity has a target.")]
    public struct HasTarget : ILogicNode, ITaskComponentData, IConditional, IReevaluateResponder
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

        public ComponentType Tag { get => typeof(HasTargetTag); }
        public System.Type SystemType { get => typeof(HasTargetTaskSystem); }
        public ComponentType ReevaluateTag { get => typeof(HasTargetReevaluateTag); }
        public System.Type ReevaluateSystemType { get => typeof(HasTargetReevaluateTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<HasTargetComponent> buffer;
            if (world.EntityManager.HasBuffer<HasTargetComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<HasTargetComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<HasTargetComponent>(entity);
            }

            buffer.Add(new HasTargetComponent()
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
            DynamicBuffer<HasTargetComponent> buffer;
            if (world.EntityManager.HasBuffer<HasTargetComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<HasTargetComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the HasTarget struct.
    /// </summary>
    public struct HasTargetComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a HasTarget node is active.
    /// </summary>
    public struct HasTargetTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the HasTarget logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct HasTargetTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, hasTargetComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<HasTargetComponent>>().WithAll<HasTargetTag, EvaluationComponent>()) {
                for (int i = 0; i < hasTargetComponents.Length; ++i) {
                    var hasTargetComponent = hasTargetComponents[i];
                    var taskComponent = taskComponents[hasTargetComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    // Find the target. There will only be one entity with the TargetEntityTag.
                    var hasTarget = false;
                    foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TargetEntityTag>()) {
                        hasTarget = true;
                        break;
                    }

                    taskComponent.Status = hasTarget ? TaskStatus.Success : TaskStatus.Failure;

                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[hasTargetComponent.Index] = taskComponent;
                }
            }
        }
    }


    /// <summary>
    /// A DOTS tag indicating when an HasTarget node needs to be reevaluated.
    /// </summary>
    public struct HasTargetReevaluateTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Runs the HasTarget reevaluation logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct HasTargetReevaluateTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the reevaluation logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, hasTargetComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<HasTargetComponent>>().WithAll<HasTargetReevaluateTag, EvaluationComponent>()) {
                for (int i = 0; i < hasTargetComponents.Length; ++i) {
                    var hasTargetComponent = hasTargetComponents[i];
                    var taskComponent = taskComponents[hasTargetComponent.Index];
                    if (!taskComponent.Reevaluate) {
                        continue;
                    }

                    // Find the target. There will only be one entity with the TargetEntityTag.
                    var hasTarget = false;
                    foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TargetEntityTag>()) {
                        hasTarget = true;
                        break;
                    }

                    var status = hasTarget ? TaskStatus.Success : TaskStatus.Failure;
                    if (status != taskComponent.Status) {
                        taskComponent.Status = status;
                        var buffer = taskComponents;
                        buffer[taskComponent.Index] = taskComponent;
                    }
                }
            }
        }
    }
}
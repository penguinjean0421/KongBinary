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

    [NodeDescription("Uses DOTS to determine if the turret is still alive.")]
    public struct IsTurretAlive : ILogicNode, ITaskComponentData, IConditional, IReevaluateResponder
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

        public ComponentType Tag { get => typeof(IsTurretAliveTag); }
        public System.Type SystemType { get => typeof(IsTurretAliveTaskSystem); }
        public ComponentType ReevaluateTag { get => typeof(IsTurretAliveReevaluateTag); }
        public System.Type ReevaluateSystemType { get => typeof(IsTurretAliveReevaluateTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<IsTurretAliveComponent> buffer;
            if (world.EntityManager.HasBuffer<IsTurretAliveComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<IsTurretAliveComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<IsTurretAliveComponent>(entity);
            }

            buffer.Add(new IsTurretAliveComponent()
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
            DynamicBuffer<IsTurretAliveComponent> buffer;
            if (world.EntityManager.HasBuffer<IsTurretAliveComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<IsTurretAliveComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the IsTurretAlive struct.
    /// </summary>
    public struct IsTurretAliveComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a IsTurretAlive node is active.
    /// </summary>
    public struct IsTurretAliveTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the IsTurretAlive logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct IsTurretAliveTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            // Find the turret. An entity will be found if the turret hasn't been destroyed.
            var isTurretAlive = false;
            foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TurretTag>()) {
                isTurretAlive = true;
                break;
            }

            foreach (var (taskComponents, isTurretAliveComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<IsTurretAliveComponent>>().WithAll<IsTurretAliveTag, EvaluationComponent>()) {
                for (int i = 0; i < isTurretAliveComponents.Length; ++i) {
                    var isTurretAliveComponent = isTurretAliveComponents[i];
                    var taskComponent = taskComponents[isTurretAliveComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }

                    taskComponent.Status = isTurretAlive ? TaskStatus.Success : TaskStatus.Failure;

                    var taskComponentBuffer = taskComponents;
                    taskComponentBuffer[isTurretAliveComponent.Index] = taskComponent;
                }
            }
        }
    }


    /// <summary>
    /// A DOTS tag indicating when an IsTurretAlive node needs to be reevaluated.
    /// </summary>
    public struct IsTurretAliveReevaluateTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Runs the IsTurretAlive reevaluation logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct IsTurretAliveReevaluateTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the reevaluation logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            // Find the turret. An entity will be found if the turret hasn't been destroyed.
            var isTurretAlive = false;
            foreach (var localTransform in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TurretTag>()) {
                isTurretAlive = true;
                break;
            }

            foreach (var (taskComponents, isTurretAliveComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<IsTurretAliveComponent>>().WithAll<IsTurretAliveReevaluateTag, EvaluationComponent>()) {
                for (int i = 0; i < isTurretAliveComponents.Length; ++i) {
                    var isTurretAliveComponent = isTurretAliveComponents[i];
                    var taskComponent = taskComponents[isTurretAliveComponent.Index];
                    if (!taskComponent.Reevaluate) {
                        continue;
                    }

                    var status = isTurretAlive ? TaskStatus.Success : TaskStatus.Failure;
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
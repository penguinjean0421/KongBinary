#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.Shared.Utility;
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;
    using System;

    /// <summary>
    /// A node representation of the return status task.
    /// </summary>
    [NodeDescription("The return status task will immediately return sucess or failure.")]
    public struct ReturnStatus : ILogicNode, ITaskComponentData, IAction, ICloneable
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("Should a success status be returned? If false then failure will be returned.")]
        [SerializeField] bool m_Success;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public bool Success { get => m_Success; set => m_Success = value; }

        public ComponentType Tag { get => typeof(ReturnStatusTag); }
        public System.Type SystemType { get => typeof(ReturnStatusTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<ReturnStatusComponent> buffer;
            if (world.EntityManager.HasBuffer<ReturnStatusComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ReturnStatusComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<ReturnStatusComponent>(entity);
            }
            buffer.Add(new ReturnStatusComponent() {
                Index = RuntimeIndex,
                Success = m_Success
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<ReturnStatusComponent> buffer;
            if (world.EntityManager.HasBuffer<ReturnStatusComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ReturnStatusComponent>(entity);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<ReturnStatus>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            clone.Success = Success;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the ReturnStatus class.
    /// </summary>
    public struct ReturnStatusComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("Should a success status be returned? If false then failure will be returned.")]
        [SerializeField] bool m_Success;
        public ushort Index { get => m_Index; set => m_Index = value; }
        public bool Success { get => m_Success; set => m_Success = value; }
    }

    /// <summary>
    /// A DOTS tag indicating when a ReturnStatus node is active.
    /// </summary>
    public struct ReturnStatusTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the ReturnStatus logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct ReturnStatusTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<TaskComponent>().WithAllRW<ReturnStatusComponent>().WithAll<ReturnStatusTag, EvaluationComponent>().Build();
            state.Dependency = new ReturnStatusJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct ReturnStatusJob : IJobEntity
        {
            /// <summary>
            /// Executes the return status logic.
            /// </summary>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="returnStatusComponents">An array of ReturnStatusComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<ReturnStatusComponent> returnStatusComponents)
            {
                for (int i = 0; i < returnStatusComponents.Length; ++i) {
                    var returnStatusComponent = returnStatusComponents[i];
                    var taskComponent = taskComponents[returnStatusComponent.Index];
                    if (taskComponent.Status != TaskStatus.Queued) {
                        continue;
                    }
                    taskComponent.Status = returnStatusComponent.Success ? TaskStatus.Success : TaskStatus.Failure;
                    taskComponents[returnStatusComponent.Index] = taskComponent;
                }
            }
        }
    }
}
#endif
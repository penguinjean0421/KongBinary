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
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;

    /// <summary>
    /// A node representation of the idle task.
    /// </summary>
    [NodeIcon("fc4d1b83384913b4abfbd8455db6df5b", "79a6985a753bb244fb5b32dc0f26addb")]
    [NodeDescription("Returns a TaskStatus of running. The task will only stop when interrupted or a conditional abort is triggered.")]
    public struct Idle : ILogicNode, ITaskComponentData, IAction
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

        public ComponentType Tag { get => typeof(IdleTag); }
        public System.Type SystemType { get => typeof(IdleTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<IdleComponent> buffer;
            if (world.EntityManager.HasBuffer<IdleComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<IdleComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<IdleComponent>(entity);
            }
            buffer.Add(new IdleComponent() {
                Index = RuntimeIndex
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<IdleComponent> buffer;
            if (world.EntityManager.HasBuffer<IdleComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<IdleComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the Idle class.
    /// </summary>
    public struct IdleComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when a Idle node is active.
    /// </summary>
    public struct IdleTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Idle logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct IdleTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<TaskComponent>().WithAll<IdleComponent, IdleTag, EvaluationComponent>().Build();
            state.Dependency = new IdleJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct IdleJob : IJobEntity
        {
            /// <summary>
            /// Executes the idle logic.
            /// </summary
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="idleComponents">An array of IdleComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<IdleComponent> idleComponents)
            {
                for (int i = 0; i < idleComponents.Length; ++i) {
                    var idleComponent = idleComponents[i];
                    var taskComponent = taskComponents[idleComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[idleComponent.Index] = taskComponent;
                    }
                }
            }
        }
    }
}
#endif
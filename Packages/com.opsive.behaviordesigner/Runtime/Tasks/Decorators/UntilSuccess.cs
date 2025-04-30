#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A node representation of the until success task.
    /// </summary>
    [NodeIcon("f2e750025a5812640919385b75319d6f", "4e9ac4f2dd8bfe741a5f889efb1ade67")]
    [NodeDescription("The until success task will keep executing its child task until the child task returns success.")]
    public struct UntilSuccess : ILogicNode, IParentNode, ITaskComponentData, IDecorator
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

        public int MaxChildCount { get { return 1; } }

        public ComponentType Tag { get => typeof(UntilSuccessTag); }
        public System.Type SystemType { get => typeof(UntilSuccessTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<UntilSuccessComponent> buffer;
            if (world.EntityManager.HasBuffer<UntilSuccessComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UntilSuccessComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<UntilSuccessComponent>(entity);
            }
            buffer.Add(new UntilSuccessComponent() {
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
            DynamicBuffer<UntilSuccessComponent> buffer;
            if (world.EntityManager.HasBuffer<UntilSuccessComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UntilSuccessComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the UntilSuccess class.
    /// </summary>
    public struct UntilSuccessComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when an UntilSuccess node is active.
    /// </summary>
    public struct UntilSuccessTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the UntilSuccess logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct UntilSuccessTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<UntilSuccessComponent>().WithAll<UntilSuccessTag, EvaluationComponent>().Build();
            state.Dependency = new UntilSuccessJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct UntilSuccessJob : IJobEntity
        {
            /// <summary>
            /// Executes the until success logic.
            /// </summary>
            /// <param name="branchComponents">An array of BranchComponents.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="untilSuccessComponents">An array of UntilSuccessComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<UntilSuccessComponent> untilSuccessComponents)
            {
                for (int i = 0; i < untilSuccessComponents.Length; ++i) {
                    var untilSuccessComponent = untilSuccessComponents[i];
                    var taskComponent = taskComponents[untilSuccessComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];
                    TaskComponent childTaskComponent;

                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[taskComponent.Index] = taskComponent;

                        childTaskComponent = taskComponents[taskComponent.Index + 1];
                        childTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[taskComponent.Index + 1] = childTaskComponent;

                        branchComponent.NextIndex = taskComponent.Index + 1;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                        continue;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The until success task is currently active. Check the first child.
                    childTaskComponent = taskComponents[taskComponent.Index + 1];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running ) {
                        // The child should keep running.
                        continue;
                    }

                    // If the child returns failure then it should be queued again.
                    if (childTaskComponent.Status == TaskStatus.Failure) {
                        childTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[taskComponent.Index + 1] = childTaskComponent;

                        branchComponent.NextIndex = taskComponent.Index + 1;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                        continue;
                    }

                    // The child has returned success. The task can end.
                    taskComponent.Status = TaskStatus.Success;
                    taskComponents[taskComponent.Index] = taskComponent;

                    branchComponent.NextIndex = taskComponent.ParentIndex;
                    branchComponents[taskComponent.BranchIndex] = branchComponent;
                }
            }
        }
    }
}
#endif
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
    /// A node representation of the return failure task.
    /// </summary>
    [NodeIcon("667a475ceee05824188a36b24ec8d392", "7d32c9b05505df24a94069606f3b823d")]
    [NodeDescription("The return failure task will always return failure except when the child task is running.")]
    public struct ReturnFailure : ILogicNode, IParentNode, ITaskComponentData, IDecorator
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

        public ComponentType Tag { get => typeof(ReturnFailureTag); }
        public System.Type SystemType { get => typeof(ReturnFailureTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<ReturnFailureComponent> buffer;
            if (world.EntityManager.HasBuffer<ReturnFailureComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ReturnFailureComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<ReturnFailureComponent>(entity);
            }
            buffer.Add(new ReturnFailureComponent() {
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
            DynamicBuffer<ReturnFailureComponent> buffer;
            if (world.EntityManager.HasBuffer<ReturnFailureComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<ReturnFailureComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the ReturnFailure class.
    /// </summary>
    public struct ReturnFailureComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when an ReturnFailure node is active.
    /// </summary>
    public struct ReturnFailureTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the ReturnFailure logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct ReturnFailureTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<ReturnFailureComponent>().WithAll<ReturnFailureTag, EvaluationComponent>().Build();
            state.Dependency = new ReturnFailureJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct ReturnFailureJob : IJobEntity
        {
            /// <summary>
            /// Executes the return failure logic.
            /// </summary>
            /// <param name="branchComponents">An array of BranchComponents.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="returnFailureComponents">An array of ReturnFailureComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<ReturnFailureComponent> returnFailureComponents)
            {
                for (int i = 0; i < returnFailureComponents.Length; ++i) {
                    var returnFailureComponent = returnFailureComponents[i];
                    var taskComponent = taskComponents[returnFailureComponent.Index];
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

                    // The return failure task is currently active. Check the first child.
                    childTaskComponent = taskComponents[taskComponent.Index + 1];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    // The child has completed. Return failure.
                    taskComponent.Status = TaskStatus.Failure;
                    taskComponents[taskComponent.Index] = taskComponent;

                    branchComponent.NextIndex = taskComponent.ParentIndex;
                    branchComponents[taskComponent.BranchIndex] = branchComponent;
                }
            }
        }
    }
}
#endif
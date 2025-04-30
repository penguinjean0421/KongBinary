#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Systems
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// System which checks for any tasks that should be reevaluated with conditional aborts. This system only marks the tasks, it does not do
    /// the actual reevaluation or interruption.
    /// </summary>
    [UpdateInGroup(typeof(BeforeTraversalSystemGroup), OrderFirst = true)]
    public partial struct ReevaluateSystem : ISystem
    {
        private EntityQuery m_Query;

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <param name="state">THe current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder().WithAllRW<TaskComponent>().WithAllRW<ReevaluateTaskComponent>().WithAll<BranchComponent>().Build();
        }

        /// <summary>
        /// Creates the ReevaluateJob.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            state.Dependency = new ReevaluateJob()
            {
                EntityCommandBuffer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);

            // The job must run immediately for the next systems.
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Job which checks for any tasks that should be reevaluated with conditional aborts. This job only flags the tasks, it does not do
        /// the actual reevaluation or interruption.
        /// </summary>
        [BurstCompile]
        private partial struct ReevaluateJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="reevaluateTaskComponents">An array of reevaluate task components.</param>
            [BurstCompile]
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, in DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<ReevaluateTaskComponent> reevaluateTaskComponents)
            {
                for (int i = 0; i < reevaluateTaskComponents.Length; ++i) {
                    var reevaluateTaskComponent = reevaluateTaskComponents[i];
                    // The task may not be able to reevaluate.
                    var taskComponent = taskComponents[reevaluateTaskComponent.Index];
                    if (!taskComponent.CanReevaluate || taskComponent.Disabled) {
                        continue;
                    }

                    // The branch may not be active.
                    var branchComponent = branchComponents[taskComponent.BranchIndex];
                    if (branchComponent.ActiveIndex == ushort.MaxValue) {
                        if (taskComponent.Reevaluate) {
                            taskComponent.Reevaluate = false;
                            taskComponents[reevaluateTaskComponent.Index] = taskComponent;

                            reevaluateTaskComponent.ReevaluateStatus = ReevaluateStatus.Inactive;
                            reevaluateTaskComponents[i] = reevaluateTaskComponent;
                        }
                        continue;
                    }

                    var reevaluate = false;
                    if (reevaluateTaskComponent.AbortType == ConditionalAbortType.Self || reevaluateTaskComponent.AbortType == ConditionalAbortType.Both) {
                        if (branchComponent.ActiveIndex > taskComponent.Index && branchComponent.ActiveIndex <= reevaluateTaskComponent.SelfPriorityUpperIndex) {
                            // Reevaluate.
                            reevaluate = true;
                            if (reevaluateTaskComponent.ReevaluateStatus == ReevaluateStatus.Inactive) {
                                reevaluateTaskComponent.ReevaluateStatus = ReevaluateStatus.Active;
                                EntityCommandBuffer.SetComponentEnabled(entityIndex, entity, reevaluateTaskComponent.ReevaluateTagComponentType, true);
                            }
                        }
                    } 
                    if (!reevaluate && (reevaluateTaskComponent.AbortType == ConditionalAbortType.LowerPriority || reevaluateTaskComponent.AbortType == ConditionalAbortType.Both)) {
                        if (branchComponent.ActiveIndex > reevaluateTaskComponent.LowerPriorityLowerIndex && branchComponent.ActiveIndex <= reevaluateTaskComponent.LowerPriorityUpperIndex) {
                            // Reevaluate.
                            reevaluate = true;
                            if (reevaluateTaskComponent.ReevaluateStatus == ReevaluateStatus.Inactive) {
                                reevaluateTaskComponent.ReevaluateStatus = ReevaluateStatus.Active;
                                EntityCommandBuffer.SetComponentEnabled(entityIndex, entity, reevaluateTaskComponent.ReevaluateTagComponentType, true);
                            }
                        }
                    }

                    // The task should no longer reevaluate.
                    if (!reevaluate && (taskComponent.Reevaluate || reevaluateTaskComponent.ReevaluateStatus == ReevaluateStatus.Dirty)) {
                        // The system needs to be kept active if there are other tasks with the same reevaluate tag.
                        var keepSystemActive = false;
                        for (int j = 0; j < reevaluateTaskComponents.Length; ++j) {
                            if (i == j) {
                                continue;
                            }

                            if (reevaluateTaskComponents[j].ReevaluateStatus == ReevaluateStatus.Active &&
                                reevaluateTaskComponent.ReevaluateTagComponentType == reevaluateTaskComponents[j].ReevaluateTagComponentType) {
                                keepSystemActive = true;
                                break;
                            }
                        }

                        if (!keepSystemActive) {
                            EntityCommandBuffer.SetComponentEnabled(entityIndex, entity, reevaluateTaskComponent.ReevaluateTagComponentType, false);
                        }
                        // The task should always disable itself.
                        taskComponent.Reevaluate = false;
                        reevaluateTaskComponent.ReevaluateStatus = ReevaluateStatus.Inactive;
                    } else {
                        // Store the current status of the task. This status will be compared after the task is reevaluated within DetermineInterruptSystem.
                        reevaluateTaskComponent.OriginalStatus = taskComponent.Status;
                    }

                    reevaluateTaskComponents[i] = reevaluateTaskComponent;

                    taskComponent.Reevaluate = reevaluate;
                    taskComponents[reevaluateTaskComponent.Index] = taskComponent;
                }
            }
        }
    }

    /// <summary>
    /// The tasks have been reevaluated. Compare the status to determine if an interrupt should occur.
    /// </summary>
    [UpdateInGroup(typeof(InterruptSystemGroup))]
    [UpdateBefore(typeof(InterruptSystem))]
    public partial struct ConditionalAbortsInvokerSystem : ISystem
    {
        private EntityQuery m_Query;

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <param name="state">THe current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<ReevaluateTaskComponent>().Build();
        }

        /// <summary>
        /// Creates the jobs necessary for conditional aborts.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            state.Dependency = new ConditionalAbortsJob()
            {
                EntityCommandBuffer = ecb.AsParallelWriter()
            }.ScheduleParallel(m_Query, state.Dependency);

            // The jobs must be run immediately for the next systems.
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Job which checks for any tasks that should be reevaluated with conditional aborts. This job only flags the tasks, it does not do
        /// the actual reevaluation or interruption.
        /// </summary>
        [BurstCompile]
        private partial struct ConditionalAbortsJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="reevaluateTaskComponents">An array of reevaluate task components.</param>
            [BurstCompile]
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<ReevaluateTaskComponent> reevaluateTaskComponents)
            {
                for (int i = 0; i < reevaluateTaskComponents.Length; ++i) {
                    var reevaluateTaskComponent = reevaluateTaskComponents[i];
                    var taskComponent = taskComponents[reevaluateTaskComponent.Index];

                    if (taskComponent.Reevaluate) {
                        if (reevaluateTaskComponent.OriginalStatus != taskComponent.Status) {
                            // The status is different. This will cause an interrupt.
                            var branchComponent = branchComponents[taskComponent.BranchIndex];
                            // The task with the highest priority should cause the abort.
                            if (branchComponent.InterruptType == InterruptType.None || taskComponent.Index < branchComponent.InterruptIndex) {
                                branchComponent.InterruptIndex = taskComponent.Index;
                                branchComponent.InterruptType = InterruptType.Branch;
                                branchComponents[taskComponent.BranchIndex] = branchComponent;
                            } else {
                                taskComponent.Status = TaskStatus.Inactive;
                            }

                            taskComponent.Reevaluate = false;
                            taskComponents[reevaluateTaskComponent.Index] = taskComponent;
                            EntityCommandBuffer.SetComponentEnabled<InterruptTag>(entityIndex, entity, true);

                            reevaluateTaskComponent.ReevaluateStatus = ReevaluateStatus.Dirty;
                            var reevaluateTaskComponentsBuffer = reevaluateTaskComponents;
                            reevaluateTaskComponentsBuffer[i] = reevaluateTaskComponent;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Processes any interrupts.
    /// </summary>
    [UpdateInGroup(typeof(InterruptSystemGroup))]
    [UpdateAfter(typeof(ConditionalAbortsInvokerSystem))]
    public partial struct InterruptSystem : ISystem
    {
        private EntityQuery m_Query;

        /// <summary>
        /// Builds the query.
        /// </summary>
        /// <param name="state">THe current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            m_Query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAll<InterruptTag>().Build();
        }

        /// <summary>
        /// Creates the InterruptJob.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            state.Dependency = new InterruptJob()
            {
                EntityCommandBuffer = ecb.AsParallelWriter(),
            }.ScheduleParallel(m_Query, state.Dependency);
            
            // The job must run immediately for the next systems.
            state.Dependency.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Triggers the interrupts.
        /// </summary>
        [BurstCompile]
        private partial struct InterruptJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            [BurstCompile]
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, DynamicBuffer<BranchComponent> branchComponents, DynamicBuffer<TaskComponent> taskComponents)
            {
                for (ushort i = 0; i < branchComponents.Length; ++i) {
                    var branchComponent = branchComponents[i];
                    if (branchComponent.InterruptType != InterruptType.None) {
                        var targetTaskComponent = taskComponents[branchComponent.InterruptIndex];
                        var parentIndex = targetTaskComponent.ParentIndex;
                        TaskStatus prevActiveNewStatus;
                        if (branchComponent.InterruptType == InterruptType.Branch) {
                            branchComponent.NextIndex = branchComponent.InterruptIndex;
                            branchComponents[i] = branchComponent;

                            // Set the target branch tasks to running.
                            targetTaskComponent.Status = TaskStatus.Running;
                            while (parentIndex != ushort.MaxValue && taskComponents[parentIndex].Status != TaskStatus.Running) {
                                var parentTaskComponent = taskComponents[parentIndex];
                                parentTaskComponent.Status = TaskStatus.Running;
                                taskComponents[parentIndex] = parentTaskComponent;
                                parentIndex = parentTaskComponent.ParentIndex;
                            }

                            prevActiveNewStatus = TaskStatus.Failure;
                        } else { // InterruptType.ImmediateSuccess/Failure.
                            targetTaskComponent.Status = branchComponent.InterruptType == InterruptType.ImmediateSuccess ? TaskStatus.Success : TaskStatus.Failure;
                            var targetBranchComponent = branchComponents[targetTaskComponent.BranchIndex];
                            targetBranchComponent.NextIndex = targetTaskComponent.ParentIndex;
                            branchComponents[targetTaskComponent.BranchIndex] = targetBranchComponent;

                            prevActiveNewStatus = targetTaskComponent.Status;
                        }

                        // Determine if any other branches need to be interrupted.
                        for (ushort j = i; j < branchComponents.Length; ++j) {
                            if (i == j || TraversalUtility.IsParent((ushort)branchComponents[j].ActiveIndex, parentIndex, ref taskComponents)) {
                                AbortChildren((ushort)branchComponents[j].ActiveIndex, parentIndex, ref taskComponents, prevActiveNewStatus);
                                // If the branch is a parallel branch then reset the NextIndex. The current branch (i) will be interrupted normally above.
                                var localBranchComponent = branchComponents[j];
                                if (localBranchComponent.InterruptType == InterruptType.None) {
                                    localBranchComponent.NextIndex = ushort.MaxValue;
                                    branchComponents[j] = localBranchComponent;
                                }
                                EntityCommandBuffer.SetComponentEnabled<InterruptedTag>(entityIndex, entity, true);
                            } else {
                                break;
                            }
                        }

                        taskComponents[targetTaskComponent.Index] = targetTaskComponent;
                    }
                }
            }

            /// <summary>
            /// Aborts all of the children within the specified branch.
            /// </summary>
            /// <param name="activeIndex">The index of the active task within the branch.</param>
            /// <param name="parentIndex">Aborts the tasks up to the specified parent index.</param>
            /// <param name="taskComponents">All of the tasks.</param>
            /// <param name="status">The abort status.</param>
            [BurstCompile]
            private void AbortChildren(ushort activeIndex, ushort parentIndex, ref DynamicBuffer<TaskComponent> taskComponents, TaskStatus status)
            {
                while (activeIndex != ushort.MaxValue && activeIndex != parentIndex) {
                    var activeTask = taskComponents[activeIndex];
                    activeTask.Status = status;
                    taskComponents[activeIndex] = activeTask;
                    activeIndex = activeTask.ParentIndex;
                }
            }
        }
    }

    /// <summary>
    /// Cleanup the interrupts after they have run.
    /// </summary>
    [UpdateInGroup(typeof(InterruptSystemGroup), OrderLast = true)]
    public partial struct InterruptCleanupSystem : ISystem
    {
        /// <summary>
        /// Executes the system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (branchComponents, entity) in
                SystemAPI.Query<DynamicBuffer<BranchComponent>>().WithAll<InterruptTag>().WithEntityAccess()) {
                for (int i = 0; i < branchComponents.Length; ++i) {
                    var branchComponent = branchComponents[i];
                    if (branchComponent.InterruptType != InterruptType.None) {
                        // Reset the interruption.
                        branchComponent.InterruptType = InterruptType.None;
                        branchComponent.InterruptIndex = 0;
                        var branchComponentBuffer = branchComponents;
                        branchComponentBuffer[i] = branchComponent;

                        state.EntityManager.SetComponentEnabled<InterruptTag>(entity, false);
                    }
                }
            }
        }
    }
}
#endif
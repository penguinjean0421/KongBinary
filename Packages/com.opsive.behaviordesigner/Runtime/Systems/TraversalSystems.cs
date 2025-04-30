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
    using Unity.Jobs;
    using UnityEngine;

    /// <summary>
    /// Loops through the active tasks to determine if the system should stay active for the current tick.
    /// </summary>
    [UpdateInGroup(typeof(TraversalSystemGroup))]
    [UpdateBefore(typeof(TraversalTaskSystemGroup))]
    public partial struct DetermineEvaluationSystem : ISystem
    {
        private bool m_JobScheduled;
        private JobHandle m_Dependency;
        private EntityCommandBuffer m_EntityCommandBuffer;
        private NativeArray<bool> m_Results;

        [Tooltip("Should the group stay active? An inactive tree does not run.")]
        public bool Active { get; private set; }
        [Tooltip("Should the group be evaluate? This bool indicates if the entire tree should be evaluated instead of the reevaluation" +
                 "concept for conditional aborts. The tree will be reevaluated if any of the leaf tasks have a status of running.")]
        public bool Evaluate { get; private set; }

        /// <summary>
        /// The system has been created.
        /// </summary>
        /// <param name="state">The state of the system.</param>
        private void OnCreate(ref SystemState state)
        {
            Active = Evaluate = true;

            m_JobScheduled = false;
        }

        /// <summary>
        /// Executes the job to determine if the system should stay active and evaluating.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            Active = Evaluate = true;
            m_JobScheduled = true;

            m_EntityCommandBuffer = new EntityCommandBuffer(Allocator.TempJob);
            m_Results = new NativeArray<bool>(3, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < m_Results.Length; ++i) {
                m_Results[i] = false;
            }

            state.Dependency = new DetermineEvaluationJob()
            {
                EntityCommandBuffer = m_EntityCommandBuffer.AsParallelWriter(),
                Results = m_Results
            }.ScheduleParallel(state.Dependency);

            m_Dependency = state.Dependency;
        }

        /// <summary>
        /// Completes the job and releases any memory.
        /// </summary>
        /// <param name="entityManager">The running EntityManager.</param>
        [BurstCompile]
        public void Complete(EntityManager entityManager)
        {
            if (!m_JobScheduled) {
                return;
            }

            m_Dependency.Complete();
            m_EntityCommandBuffer.Playback(entityManager);
            m_EntityCommandBuffer.Dispose();

            if (m_Results.IsCreated) {
                if (m_Results[0]) {
                    Active = m_Results[1];
                    Evaluate = m_Results[2];
                } else {
                    // If the first element is false then no trees executed.
                    Active = Evaluate = false;
                }
                m_Results.Dispose();
            }
            m_JobScheduled = false;
        }

        /// <summary>
        /// The system has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            if (m_Dependency.IsCompleted) {
                return;
            }

            m_Results.Dispose();
        }

        /// <summary>
        /// Job which determine if the system should stay active. If any behavior tree should stay active then the entire system must remain active.
        /// </summary>
        [BurstCompile]
        public partial struct DetermineEvaluationJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;
            [Tooltip("The computed results.")]
            [NativeDisableParallelForRestriction] public NativeArray<bool> Results;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            /// <param name="evaluationComponent">The EvaluationComponent that belongs to the entity.</param>
            [BurstCompile]
            private void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref EvaluationComponent evaluationComponent)
            {
                Results[0] = true; // The first element indicates that the job has been executed.

                // No branches may be active.
                var active = false;
                for (int i = 0; i < branchComponents.Length; ++i) {
                    var branchComponent = branchComponents[i];
                    if (branchComponent.ActiveIndex == ushort.MaxValue) {
                        continue;
                    }
                    active = true;
                    break;
                }

                // If a branch is active then at least one task within that branch is active.
                var evaluate = false;
                if (active) {
                    var continueEvaluation = false;
                    for (int i = 0; i < branchComponents.Length; ++i) {
                        var branchComponent = branchComponents[i];
                        if (branchComponent.ActiveIndex == ushort.MaxValue) {
                            continue;
                        }

                        var taskComponent = taskComponents[branchComponent.ActiveIndex];
                        var taskComponentBuffer = taskComponents;
                        var isParentTask = EvaluationUtility.IsParentTask(ref taskComponentBuffer, branchComponent.ActiveIndex);
                        // Continue evaluation if the active task is an outer node (action or conditional) and is not running OR
                        // the task is an inner node (composite or decorator), is running, and is not a parallel task. Parent tasks cannot run without an active child.
                        if ((!isParentTask && taskComponent.Status != TaskStatus.Running && taskComponent.ParentIndex != ushort.MaxValue) ||
                            (isParentTask && (taskComponent.Status == TaskStatus.Queued || taskComponent.Status == TaskStatus.Running) &&
                                !EvaluationUtility.IsParallelTask(ref taskComponentBuffer, branchComponent.ActiveIndex))) {
                            continueEvaluation = true;
                        }
                    }

                    if (continueEvaluation) {
                        evaluationComponent.EvaluationCount++;

                        // Disable the evaluation if too many tasks have run.
                        if ((evaluationComponent.EvaluationType == EvaluationType.EntireTree && evaluationComponent.EvaluationCount >= taskComponents.Length * 2) ||
                            (evaluationComponent.EvaluationType == EvaluationType.Count && evaluationComponent.EvaluationCount >= evaluationComponent.MaxEvaluationCount)) {
                            // Reset the count when the component has been disabled.
                            evaluationComponent.EvaluationCount = 0;
                            EntityCommandBuffer.SetComponentEnabled<EvaluationComponent>(entityIndex, entity, false);
                        } else {
                            evaluate = true;
                        }
                    } else {
                        evaluationComponent.EvaluationCount = 0;
                        EntityCommandBuffer.SetComponentEnabled<EvaluationComponent>(entityIndex, entity, false);
                    }
                }

                // Sets the results. These results are not thread safe but that's ok because the value will only be read after the job is complete.
                if (active) {
                    Results[1] = true;
                }
                if (evaluate) {
                    Results[2] = true;
                }
            }
        }
    }

    /// <summary>
    /// Traverses and ensures the correct tasks are active.
    /// </summary>
    [UpdateInGroup(typeof(TraversalSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(TraversalTaskSystemGroup))]
    public partial struct EvaluationSystem : ISystem
    {
        private bool m_JobScheduled;
        private JobHandle m_Dependency;
        private EntityCommandBuffer m_EntityCommandBuffer;

        /// <summary>
        /// The system has been created.
        /// </summary>
        /// <param name="state">The state of the system.</param>
        private void OnCreate(ref SystemState state)
        {
            m_JobScheduled = false;
        }

        /// <summary>
        /// Starts the job which traverses the tree.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            m_JobScheduled = true;

            m_EntityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
            state.Dependency = new EvaluationJob()
            {
                EntityCommandBuffer = m_EntityCommandBuffer.AsParallelWriter(),
            }.ScheduleParallel(state.Dependency);
        }

        /// <summary>
        /// Completes the job and releases any memory.
        /// </summary>
        /// <param name="entityManager">The running EntityManager.</param>
        /// <param name="stopRunning">Has the system been stopped?</param>
        [BurstCompile]
        public void Complete(EntityManager entityManager, bool stopRunning = false)
        {
            if (!m_JobScheduled) {
                return;
            }

            if (!stopRunning) {
                m_Dependency.Complete();
                m_EntityCommandBuffer.Playback(entityManager);
                m_EntityCommandBuffer.Dispose();
            }

            m_JobScheduled = false;
        }

        /// <summary>
        /// Job which traverses the tree.
        /// </summary>
        [BurstCompile]
        public partial struct EvaluationJob : IJobEntity
        {
            [Tooltip("CommandBuffer which sets the component data.")]
            public EntityCommandBuffer.ParallelWriter EntityCommandBuffer;

            /// <summary>
            /// Executes the job.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param
            /// <param name="branchComponents">An array of branch components.</param>
            /// <param name="taskComponents">An array of task components.</param>
            [BurstCompile]
            public void Execute(Entity entity, [EntityIndexInQuery] int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents)
            {
                EvaluationUtility.Traverse(ref entity, ref EntityCommandBuffer, entityIndex, ref branchComponents, ref taskComponents);
            }
        }
    }

    /// <summary>
    /// Utility functions for the task evaluation.
    /// </summary>
    [BurstCompile]
    public struct EvaluationUtility
    {
        /// <summary>
        /// Is the task at the specified index a parent task.
        /// </summary>
        /// <param name="taskComponents">An array of task components.</param>
        /// <param name="index">The index to check if it is a parent.</param>
        /// <returns>True if the task at the specified index is a parent task.</returns>
        [BurstCompile]
        public static bool IsParentTask(ref DynamicBuffer<TaskComponent> taskComponents, int index)
        {
            // The last task cannot be a parent.
            if (index == taskComponents.Length - 1) {
                return false;
            }

            // The next child will have a parent of the current task.
            if (taskComponents[index + 1].ParentIndex == index) {
                return true;
            }

            // The parent index is different - the current task is not a parent.
            return false;
        }

        /// <summary>
        /// Is the task at the specified index a parallel task.
        /// </summary>
        /// <param name="taskComponents">An array of task components.</param>
        /// <param name="index">The index to check if it is parallel.</param>
        /// <returns>True if the task at the specified index is a parallel task.</returns>
        [BurstCompile]
        public static bool IsParallelTask(ref DynamicBuffer<TaskComponent> taskComponents, int index)
        {
            // The task is a parent task, but is it parallel?
            return taskComponents[index].BranchIndex != taskComponents[index + 1].BranchIndex;
        }

        /// <summary>
        /// Traverses through the TaskComponents to determine the active index. This method will update the corresponding tag.
        /// </summary>
        /// <param name="entity">The entity being traversed.</param>
        /// <param name="entityCommandBuffer">The EntityCommandBuffer used to update the tag.</param>
        /// <param name="entityIndex">The index of the entity.</param>
        /// <param name="branchComponents">An array of branch components.</param>
        /// <param name="taskComponents">An array of task components.</param>
        [BurstCompile]
        public static void Traverse(ref Entity entity, ref EntityCommandBuffer.ParallelWriter entityCommandBuffer, int entityIndex, ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents)
        {
            for (int i = 0; i < branchComponents.Length; ++i) {
                var branchComponent = branchComponents[i];
                if (branchComponent.ActiveIndex != ushort.MaxValue && branchComponent.ActiveIndex == branchComponent.NextIndex) {
                    var activeTask = taskComponents[branchComponent.ActiveIndex];
                    if (activeTask.Status == TaskStatus.Success || activeTask.Status == TaskStatus.Failure) {
                        branchComponent.NextIndex = activeTask.ParentIndex;
                    }
                }
                if (branchComponent.ActiveIndex != branchComponent.NextIndex) {
                    // Do not switch into a disabled task.
                    if (branchComponent.NextIndex != ushort.MaxValue && taskComponents[branchComponent.NextIndex].Disabled) {
                        var taskComponent = taskComponents[branchComponent.NextIndex];
                        taskComponent.Status = TaskStatus.Inactive;
                        var taskComponentBuffer = taskComponents;
                        taskComponentBuffer[branchComponent.NextIndex] = taskComponent;

                        branchComponent.NextIndex = branchComponent.ActiveIndex;
                    } else {
                        // The status for all children should be reset back to their inactive state if the next task is within a new branch. This will prevent
                        // the return status from being reset when the task ends normally.
                        var taskComponentBuffer = taskComponents;
                        if (branchComponent.NextIndex != ushort.MaxValue &&
                            !TraversalUtility.IsParent((ushort)branchComponent.ActiveIndex, (ushort)branchComponent.NextIndex, ref taskComponentBuffer)) {
                            var nextTaskComponent = taskComponents[branchComponent.NextIndex];
                            nextTaskComponent.Status = nextTaskComponent.Status == TaskStatus.Running ? TaskStatus.Running : TaskStatus.Queued;
                            taskComponentBuffer[branchComponent.NextIndex] = nextTaskComponent;
                            var childCount = TraversalUtility.GetChildCount(branchComponent.NextIndex, ref taskComponentBuffer);
                            for (int j = 0; j < childCount; ++j) {
                                var childTaskComponent = taskComponents[branchComponent.NextIndex + j + 1];
                                childTaskComponent.Status = TaskStatus.Inactive;
                                taskComponentBuffer[branchComponent.NextIndex + j + 1] = childTaskComponent;
                            }
                        }

                        branchComponent.ActiveIndex = branchComponent.NextIndex;

                        // Change the component tag if the task type is different.
                        var componentType = branchComponent.ActiveIndex != ushort.MaxValue ? taskComponents[branchComponent.ActiveIndex].TagComponentType : new ComponentType();
                        if (componentType != branchComponent.ActiveTagComponentType) {
                            if (branchComponent.ActiveTagComponentType.TypeIndex != TypeIndex.Null) {
                                var deactivateTag = true;
                                for (int j = 0; j < branchComponents.Length; ++j) {
                                    // The tag should be deactivated if no other tasks have the same tag type.
                                    if (i != j && branchComponents[j].ActiveIndex != ushort.MaxValue &&
                                        branchComponent.ActiveTagComponentType == branchComponents[j].ActiveTagComponentType) {
                                        deactivateTag = false;
                                        break;
                                    }
                                }

                                // The task of that type is no longer active - disable the system to prevent it from running.
                                if (deactivateTag) {
                                    entityCommandBuffer.SetComponentEnabled(entityIndex, entity, branchComponent.ActiveTagComponentType, false);
                                }
                            }
                            // A new system type should start.
                            if (branchComponent.ActiveIndex != ushort.MaxValue) {
                                var taskComponent = taskComponents[branchComponent.ActiveIndex];
                                entityCommandBuffer.SetComponentEnabled(entityIndex, entity, taskComponent.TagComponentType, true);
                            }
                            branchComponent.ActiveTagComponentType = componentType;
                        }
                    }
                    var branchComponentBuffer = branchComponents;
                    branchComponentBuffer[i] = branchComponent;
                }
            }
        }
    }
}
#endif
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
    using Opsive.GraphDesigner.Runtime;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Specifies that the node is an object task which can specify the next child that should run.
    /// </summary>
    public interface ITaskObjectParentNode
    {
        /// <summary>
        /// Returns the index of the next child that should run. Set to ushort.MaxValue to ignore.
        /// </summary>
        ushort NextChildIndex { get; }
    }

    /// <summary>
    /// The DOTS data structure for the TaskObject class.
    /// </summary>
    public struct TaskObjectComponent : IBufferElementData
    {
        [Tooltip("The index of the task.")]
        public ushort Index;
    }

    /// <summary>
    /// A DOTS tag indicating when an TaskObject node is active.
    /// </summary>
    public struct TaskObjectTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the TaskObject logic.
    /// </summary>
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TraversalTaskSystemGroup), OrderLast = true)]
    public partial struct TaskObjectSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            // When the task is interrupted there is no callback which prevents Task.OnEnd from being called. Track the status within the referenced task object and if the status is different then
            // the task was aborted and OnEnd needs to be called.
            foreach (var (taskObjectComponents, taskComponents, entity) in
                SystemAPI.Query<DynamicBuffer<TaskObjectComponent>, DynamicBuffer<TaskComponent>>().WithAll<InterruptedTag>().WithEntityAccess()) {
                var behaviorTree = BehaviorTree.GetBehaviorTree(entity);
                if (behaviorTree == null) {
                    continue;
                }

                for (int i = 0; i < taskObjectComponents.Length; ++i) {
                    var taskComponent = taskComponents[taskObjectComponents[i].Index];
                    if (taskComponent.Status == TaskStatus.Success || taskComponent.Status == TaskStatus.Failure) {
                        var task = behaviorTree.GetTask(taskObjectComponents[i].Index) as Task;
                        if (task.Status != taskComponent.Status) {
                            task.OnEnd();
                            task.Status = taskComponent.Status;
                        }
                    }
                }
            }

            // Update the task objects.
            foreach (var (taskObjectComponents, taskComponents, branchComponents, entity) in
                SystemAPI.Query<DynamicBuffer<TaskObjectComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<BranchComponent>>().WithAll<TaskObjectTag, EvaluationComponent>().WithEntityAccess()) {

                var behaviorTree = BehaviorTree.GetBehaviorTree(entity);
                if (behaviorTree == null) {
                    continue;
                }

                for (int i = 0; i < taskObjectComponents.Length; ++i) {
                    var taskComponent = taskComponents[taskObjectComponents[i].Index];
                    var task = behaviorTree.GetTask(taskObjectComponents[i].Index) as Task;
                    if (taskComponent.Status == TaskStatus.Queued) {
                        task.Status = taskComponent.Status = TaskStatus.Running;
                        var buffer = taskComponents;
                        buffer[taskComponent.Index] = taskComponent;

                        task.OnStart();
                    }
                    if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }
                    
                    var status = task.OnUpdate();
                    // Update the status if has changed.
                    if (status != taskComponent.Status) {
                        task.Status = taskComponent.Status = status;
                        var buffer = taskComponents;
                        buffer[taskComponent.Index] = taskComponent;

                        // End the task if it is done running.
                        if (status != TaskStatus.Running) {
                            task.OnEnd();

                            var branchComponent = branchComponents[taskComponent.BranchIndex];
                            branchComponent.NextIndex = taskComponent.ParentIndex;
                            var branchComponentBuffer = branchComponents;
                            branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                            continue;
                        }
                    }

                    // Parent object tasks do not have a direct way to set the next child. Use the ITaskObjectParentNode to switch the child task.
                    if (task is IParentNode && (task is ITaskObjectParentNode taskObjectParentNode) && status == TaskStatus.Running) {
                        if (taskObjectParentNode.NextChildIndex != ushort.MaxValue && taskComponents[taskObjectParentNode.NextChildIndex].Status != TaskStatus.Running) {
                            var branchComponent = branchComponents[taskComponent.BranchIndex];
                            branchComponent.NextIndex = taskObjectParentNode.NextChildIndex;
                            var branchComponentBuffer = branchComponents;
                            branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;

                            var nextTaskComponent = taskComponents[taskObjectParentNode.NextChildIndex];
                            nextTaskComponent.Status = TaskStatus.Queued;
                            var taskComponentBuffer = taskComponents;
                            taskComponentBuffer[taskObjectParentNode.NextChildIndex] = nextTaskComponent;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A DOTS tag indicating when an TaskObject node needs to be reevaluated.
    /// </summary>
    public struct TaskObjectReevaluateTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Runs the TaskObject reevaluation logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct TaskObjectReevaluateSystem : ISystem
    {
        /// <summary>
        /// Updates the reevaluation logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, taskObjectComponents, entity) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<TaskObjectComponent>>().WithAll<TaskObjectReevaluateTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < taskObjectComponents.Length; ++i) {
                    var taskObjectComponent = taskObjectComponents[i];
                    var taskComponent = taskComponents[taskObjectComponent.Index];
                    if (!taskComponent.Reevaluate) {
                        continue;
                    }
                    var behaviorTree = BehaviorTree.GetBehaviorTree(entity);
                    if (behaviorTree == null) {
                        continue;
                    }

                    var task = behaviorTree.GetTask(taskObjectComponent.Index) as IConditionalReevaluation;
                    var status = task.OnReevaluateUpdate();
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
#endif
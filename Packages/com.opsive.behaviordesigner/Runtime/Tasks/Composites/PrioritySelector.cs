#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Composites
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.Shared.Utility;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;
    using System;

    /// <summary>
    /// A node representation of the priority selector task.
    /// </summary>
    [NodeIcon("cea0f2b6cee06a742bb35dcc40202e8e", "744afc2640950e045961296f1d5800d7")]
    [NodeDescription("Similar to the selector task, the priority selector task will return success as soon as a child task returns success. " +
                     "Instead of running the tasks sequentially from left to right within the tree, the priority selector will ask the task what its priority is to determine the order. " +
                     "The higher priority tasks have a higher chance at being run first.")]
    public struct PrioritySelector : ILogicNode, IParentNode, ITaskComponentData, IComposite, ISavableTask, ICloneable
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;

        private ushort m_ComponentIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public int MaxChildCount { get { return int.MaxValue; } }

        public ComponentType Tag { get => typeof(PrioritySelectorTag); }
        public System.Type SystemType { get => typeof(PrioritySelectorTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<PrioritySelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<PrioritySelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PrioritySelectorComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<PrioritySelectorComponent>(entity);
            }

            buffer.Add(new PrioritySelectorComponent() {
                Index = RuntimeIndex,
            });
            m_ComponentIndex = (ushort)(buffer.Length - 1);
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<PrioritySelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<PrioritySelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PrioritySelectorComponent>(entity);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public MemberVisibility GetSaveReflectionType(int index) { return MemberVisibility.None; }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public object Save(World world, Entity entity)
        {
            var prioritySelectorComponents = world.EntityManager.GetBuffer<PrioritySelectorComponent>(entity);
            var prioritySelectorComponent = prioritySelectorComponents[m_ComponentIndex];

            // Save the active child and array order.
            var saveData = new object[2];
            saveData[0] = prioritySelectorComponent.ActiveRelativeChildIndex;
            if (prioritySelectorComponent.Priorities.IsCreated) {
                saveData[1] = prioritySelectorComponent.Priorities.ToArray();
            }
            return saveData;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var prioritySelectorComponents = world.EntityManager.GetBuffer<PrioritySelectorComponent>(entity);
            var prioritySelectorComponent = prioritySelectorComponents[m_ComponentIndex];

            // saveData is the active child and array order.
            var taskSaveData = (object[])saveData;
            prioritySelectorComponent.ActiveRelativeChildIndex = (ushort)taskSaveData[0];
            if (taskSaveData[1] != null) {
                prioritySelectorComponent.Priorities = new NativeArray<PrioritySelectorComponent.PriorityItem>((PrioritySelectorComponent.PriorityItem[])taskSaveData[1], Allocator.Persistent);
            }
            prioritySelectorComponents[m_ComponentIndex] = prioritySelectorComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<PrioritySelector>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the PrioritySelector class.
    /// </summary>
    public struct PrioritySelectorComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The relative index of the child that is currently active.")]
        public ushort ActiveRelativeChildIndex;
        [Tooltip("The latest priority values for each child task.")]
        public NativeArray<PriorityItem> Priorities;

        /// <summary>
        /// Joins the task index with the priority value.
        /// </summary>
        public struct PriorityItem : IComparable<PriorityItem>
        {
            [Tooltip("The index of the task.")]
            public ushort TaskIndex;
            [Tooltip("The index of the PriorityValueComponent. A value of ushort.MaxValue indicates that there is not a corresponding PriorityValueComponent to this element.")]
            public ushort PriorityValueIndex;
            [Tooltip("The priority value.")]
            public float Value;

            /// <summary>
            /// Compares the current PriorityItem to the other PriorityItem.
            /// </summary>
            /// <param name="other">The other PriorityItem.</param>
            /// <returns>The comparison between the current PriorityItem and the other PriorityItem.</returns>
            public int CompareTo(PriorityItem other)
            {
                // The higher the value the lower the item is in the array.
                return other.Value.CompareTo(Value);
            }
        }
    }

    /// <summary>
    /// DOTS structure that contains the most recently priority of the task.
    /// </summary>
    public struct PriorityValueComponent : IBufferElementData
    {
        [Tooltip("The index of the task.")]
        public ushort Index;
        [Tooltip("The current priority value. The higher the value the more likely it will be selected.")]
        public float Value;
    }

    /// <summary>
    /// A DOTS tag indicating when a PrioritySelector node is active.
    /// </summary>
    public struct PrioritySelectorTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the PrioritySelector logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct PrioritySelectorTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var hasPriorityValueComponent = false;
            foreach (var (branchComponents, taskComponents, prioritySelectorComponents, priorityValueComponents) in
                SystemAPI.Query<DynamicBuffer<BranchComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<PrioritySelectorComponent>, DynamicBuffer<PriorityValueComponent>>().WithAll<PrioritySelectorTag, EvaluationComponent>()) {

                hasPriorityValueComponent = true;
                for (int i = 0; i < prioritySelectorComponents.Length; ++i) {
                    var prioritySelectorComponent = prioritySelectorComponents[i];
                    var taskComponent = taskComponents[prioritySelectorComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    var prioritySelectorComponentsBuffer = prioritySelectorComponents;
                    var taskComponentsBuffer = taskComponents;
                    var branchComponentBuffer = branchComponents;
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponentsBuffer[taskComponent.Index] = taskComponent;

                        // Initialize the priority value array.
                        NativeArray<PrioritySelectorComponent.PriorityItem> priorities;
                        if (prioritySelectorComponent.Priorities.Length == 0) {
                            var childCount = TraversalUtility.GetImmediateChildCount(ref taskComponent, ref taskComponentsBuffer);
                            priorities = new NativeArray<PrioritySelectorComponent.PriorityItem>(childCount, Allocator.Persistent);
                            // Match the PriorityValueComponent with the child index.
                            var childIndex = (ushort)(taskComponent.Index + 1);
                            for (ushort j = 0; j < childCount; ++j) {
                                priorities[j] = new PrioritySelectorComponent.PriorityItem() { TaskIndex = childIndex, PriorityValueIndex = ushort.MaxValue, Value = float.MinValue };
                                for (ushort k = 0; k < priorityValueComponents.Length; ++k) {
                                    var priorityValueComponent = priorityValueComponents[k];

                                    if (priorityValueComponent.Index == childIndex) {
                                        var priorityItem = priorities[j];
                                        priorityItem.PriorityValueIndex = k;
                                        priorities[j] = priorityItem;
                                        break;
                                    }
                                }
                                childIndex = taskComponents[childIndex].SiblingIndex;
                            }

                            prioritySelectorComponent.Priorities = priorities;
                        }

                        // Determine the child order when the task starts.
                        priorities = prioritySelectorComponent.Priorities;

                        for (ushort j = 0; j < priorities.Length; ++j) {
                            var valueIndex = priorities[j].PriorityValueIndex;
                            // The task may not have a matching PriorityValueComponent.
                            if (valueIndex == ushort.MaxValue) {
                                continue;
                            }

                            var priorityItem = priorities[j];
                            priorityItem.Value = priorityValueComponents[valueIndex].Value;
                            priorities[j] = priorityItem;
                        }
                        priorities.Sort();
                        prioritySelectorComponent.Priorities = priorities;
                        prioritySelectorComponentsBuffer[i] = prioritySelectorComponent;

                        prioritySelectorComponent.ActiveRelativeChildIndex = 0;
                        branchComponent.NextIndex = prioritySelectorComponent.Priorities[prioritySelectorComponent.ActiveRelativeChildIndex].TaskIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;

                        // Start the child.
                        var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                        nextChildTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[branchComponent.NextIndex] = nextChildTaskComponent;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The prioritySelector task is currently active. Check the first child.
                    var childTaskComponent = taskComponents[prioritySelectorComponent.Priorities[prioritySelectorComponent.ActiveRelativeChildIndex].TaskIndex];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    // Switch to the next highest priority. If no more priority values exist the task should act as a normal selector.
                    if (prioritySelectorComponent.ActiveRelativeChildIndex == prioritySelectorComponent.Priorities.Length - 1 || 
                        childTaskComponent.Status == TaskStatus.Success) {
                        // There are no more children or the child succeeded. The selector task should end.
                        taskComponent.Status = childTaskComponent.Status;
                        prioritySelectorComponent.ActiveRelativeChildIndex = 0;
                        taskComponentsBuffer[prioritySelectorComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    } else {
                        // The child task returned failure. Move onto the next task. 
                        prioritySelectorComponent.ActiveRelativeChildIndex++;
                        var nextIndex = prioritySelectorComponent.Priorities[prioritySelectorComponent.ActiveRelativeChildIndex].TaskIndex;
                        var nextTaskComponent = taskComponents[nextIndex];
                        nextTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[nextIndex] = nextTaskComponent;

                        branchComponent.NextIndex = nextIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    }
                    prioritySelectorComponentsBuffer[i] = prioritySelectorComponent;
                }
            }

            // Special case where the PrioritySelectorComponent has no PriorityValueComponent children.
            if (!hasPriorityValueComponent) {
                foreach (var (prioritySelectorComponents, taskComponents, branchComponents) in
                    SystemAPI.Query<DynamicBuffer<PrioritySelectorComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<BranchComponent>>().WithAll<PrioritySelectorTag, EvaluationComponent>()) {

                    for (int i = 0; i < prioritySelectorComponents.Length; ++i) {
                        var prioritySelectorComponent = prioritySelectorComponents[i];
                        var taskComponent = taskComponents[prioritySelectorComponent.Index];

                        // If there are no values then the selector should return failure.
                        if (taskComponent.Status == TaskStatus.Queued && prioritySelectorComponent.Priorities.Length == 0) {
                            taskComponent.Status = TaskStatus.Failure;
                            var taskComponentsBuffer = taskComponents;
                            taskComponentsBuffer[prioritySelectorComponent.Index] = taskComponent;

                            var branchComponent = branchComponents[taskComponent.BranchIndex];
                            branchComponent.NextIndex = taskComponent.ParentIndex;
                            var branchComponentBuffer = branchComponents;
                            branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The task has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            foreach (var prioritySelectorComponents in SystemAPI.Query<DynamicBuffer<PrioritySelectorComponent>>()) {
                for (int i = 0; i < prioritySelectorComponents.Length; ++i) {
                    prioritySelectorComponents[i].Priorities.Dispose();
                }
            }
        }
    }
}
#endif
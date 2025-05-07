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
    /// A node representation of the utility selector task.
    /// </summary>
    [NodeIcon("9d36cd363c3e08246a6e9eaf5ad99d69", "db3d0b77c7f9e0b4f9157aa03178836a")]
    [NodeDescription("The utility selector task evaluates the child tasks using Utility Theory AI. The child task can return the utility value " +
                     "at that particular time. The task with the highest utility value will be selected and the existing running task will be aborted. The utility selector " +
                     "task reevaluates its children every tick.")]
    public struct UtilitySelector : ILogicNode, IParentNode, ITaskComponentData, IComposite, ISavableTask, ICloneable
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

        public ComponentType Tag { get => typeof(UtilitySelectorTag); }
        public System.Type SystemType { get => typeof(UtilitySelectorTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<UtilitySelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<UtilitySelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UtilitySelectorComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<UtilitySelectorComponent>(entity);
            }

            buffer.Add(new UtilitySelectorComponent() {
                Index = RuntimeIndex,
            });
            m_ComponentIndex = (ushort)(buffer.Length - 1);

            var entityManager = world.EntityManager;
            ComponentUtility.AddInterruptComponents(ref entityManager, ref entity);
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<UtilitySelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<UtilitySelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UtilitySelectorComponent>(entity);
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
            var utilitySelectorComponents = world.EntityManager.GetBuffer<UtilitySelectorComponent>(entity);
            var utilitySelectorComponent = utilitySelectorComponents[m_ComponentIndex];

            // Save the active child.
            return utilitySelectorComponent.ActiveChildIndex;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var utilitySelectorComponents = world.EntityManager.GetBuffer<UtilitySelectorComponent>(entity);
            var utilitySelectorComponent = utilitySelectorComponents[m_ComponentIndex];

            // saveData is the active child index.
            utilitySelectorComponent.ActiveChildIndex = (ushort)saveData;
            utilitySelectorComponents[m_ComponentIndex] = utilitySelectorComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<UtilitySelector>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the UtilitySelector class.
    /// </summary>
    public struct UtilitySelectorComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The index of the child that is currently active.")]
        public ushort ActiveChildIndex;
        [Tooltip("The index of the UtilityValueComponent that corresponds to each child.")]
        public NativeArray<UtilityItem> UtilityItems;

        /// <summary>
        /// A mapping between the UnityValueComponent and the task.
        /// </summary>
        public struct UtilityItem
        {
            [Tooltip("The index of the task.")]
            public ushort TaskIndex;
            [Tooltip("The index of the UtilityValueComponent.")]
            public ushort UtilityValueIndex;
            [Tooltip("Can the task continue to exectute?")]
            public bool CanExecute;
        }
    }

    /// <summary>
    /// DOTS structure that contains the most recently utility of the task.
    /// </summary>
    public struct UtilityValueComponent : IBufferElementData
    {
        [Tooltip("The index of the task.")]
        public ushort Index;
        [Tooltip("The current utility value. The higher the value the more likely it will be selected.")]
        public float Value;
    }

    /// <summary>
    /// A DOTS tag indicating when a UtilitySelector node is active.
    /// </summary>
    public struct UtilitySelectorTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the UtilitySelector logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct UtilitySelectorTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var hasUtilityValueComponent = false;
            foreach (var (utilitySelectorComponents, utilityValueComponents, taskComponents, branchComponents, entity) in
                SystemAPI.Query<DynamicBuffer<UtilitySelectorComponent>, DynamicBuffer<UtilityValueComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<BranchComponent>>().WithAll<EvaluationComponent>().WithEntityAccess()) {

                hasUtilityValueComponent = true;
                for (int i = 0; i < utilitySelectorComponents.Length; ++i) {
                    var utilitySelectorComponent = utilitySelectorComponents[i];
                    var taskComponent = taskComponents[utilitySelectorComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    var utilitySelectorComponentsBuffer = utilitySelectorComponents;
                    var utilityValueComponentBuffer = utilityValueComponents;
                    var taskComponentsBuffer = taskComponents;
                    var branchComponentBuffer = branchComponents;
                    if (taskComponent.Status == TaskStatus.Queued) {
                        // Initialize the UtilityItem array.
                        if (utilitySelectorComponent.UtilityItems.Length == 0) {
                            var childCount = TraversalUtility.GetImmediateChildCount(ref taskComponent, ref taskComponentsBuffer);
                            var utilityItems = new NativeArray<UtilitySelectorComponent.UtilityItem>(childCount, Allocator.Persistent);
                            // Match the UtilitySelectorComponent with the child index.
                            var childIndex = (ushort)(taskComponent.Index + 1);
                            for (ushort j = 0; j < childCount; ++j) {
                                utilityItems[j] = new UtilitySelectorComponent.UtilityItem() { TaskIndex = childIndex, UtilityValueIndex = ushort.MaxValue, CanExecute = !taskComponents[childIndex].Disabled };

                                for (ushort k = 0; k < utilityValueComponents.Length; ++k) {
                                    var utilityValueComponent = utilityValueComponents[k];
                                    if (utilityValueComponent.Index == childIndex) {
                                        var utilityItem = utilityItems[j];
                                        utilityItem.UtilityValueIndex = k;
                                        utilityItems[j] = utilityItem;
                                        break;
                                    }
                                }
                                childIndex = taskComponents[childIndex].SiblingIndex;
                            }

                            utilitySelectorComponent.UtilityItems = utilityItems;
                        } else {
                            // Reset the execution status.
                            var childIndex = (ushort)(taskComponent.Index + 1);
                            for (int j = 0; j < utilitySelectorComponent.UtilityItems.Length; ++j) {
                                var utilityItem = utilitySelectorComponent.UtilityItems[j];
                                utilityItem.CanExecute = !taskComponents[childIndex].Disabled;
                                utilitySelectorComponent.UtilityItems[j] = utilityItem;

                                childIndex = taskComponents[childIndex].SiblingIndex;
                            }
                        }

                        utilitySelectorComponent.ActiveChildIndex = GetHighestUtility(utilitySelectorComponent.UtilityItems, ref utilityValueComponentBuffer);
                        utilitySelectorComponentsBuffer[i] = utilitySelectorComponent;

                        if (utilitySelectorComponent.ActiveChildIndex == ushort.MaxValue) {
                            taskComponent.Status = TaskStatus.Failure;
                            branchComponent.NextIndex = taskComponent.ParentIndex;
                        } else {
                            taskComponent.Status = TaskStatus.Running;
                            branchComponent.NextIndex = utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex].TaskIndex;

                            // Start the child.
                            var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                            nextChildTaskComponent.Status = TaskStatus.Queued;
                            taskComponentsBuffer[branchComponent.NextIndex] = nextChildTaskComponent;
                        }
                        taskComponentsBuffer[taskComponent.Index] = taskComponent;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;

                        continue;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    var childTaskComponent = taskComponents[utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex].TaskIndex];
                    if (childTaskComponent.Status == TaskStatus.Success) {
                        // The child has returned success. Stop the selector.
                        taskComponent.Status = childTaskComponent.Status;
                        taskComponentsBuffer[utilitySelectorComponent.Index] = taskComponent;
                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                        continue;
                    } else if (childTaskComponent.Status == TaskStatus.Failure) {
                        var utilityItem = utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex];
                        utilityItem.CanExecute = false;
                        utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex] = utilityItem;

                        utilitySelectorComponentsBuffer[i] = utilitySelectorComponent;
                    }

                    // The active task returned failure or is currently running. Determine if the active task needs to change.
                    var highestIndex = GetHighestUtility(utilitySelectorComponent.UtilityItems, ref utilityValueComponentBuffer);
                    if (highestIndex == utilitySelectorComponent.ActiveChildIndex) {
                        // No changes are necessary.
                        continue;
                    }

                    var activeTaskIndex = utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex].TaskIndex;
                    utilitySelectorComponent.ActiveChildIndex = highestIndex;
                    utilitySelectorComponentsBuffer[i] = utilitySelectorComponent;

                    // A new branch has been selected. Trigger an interrupt on the currently active branch.
                    if (taskComponents[activeTaskIndex].Status == TaskStatus.Running){
                        branchComponent.InterruptType = InterruptType.Branch;
                        branchComponent.InterruptIndex = utilitySelectorComponent.UtilityItems[highestIndex].TaskIndex;
                        state.EntityManager.SetComponentEnabled<InterruptTag>(entity, true);
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                        continue;
                    }

                    // No more tasks may need to run.
                    if (highestIndex == ushort.MaxValue) {
                        taskComponent.Status = TaskStatus.Failure;
                        taskComponentsBuffer[utilitySelectorComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                    } else {
                        // Run the task with the next highest utility value.
                        var nextTaskIndex = utilitySelectorComponent.UtilityItems[utilitySelectorComponent.ActiveChildIndex].TaskIndex;
                        var nextTaskComponent = taskComponents[nextTaskIndex];
                        nextTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[nextTaskIndex] = nextTaskComponent;

                        branchComponent.NextIndex = nextTaskIndex;
                    }
                    branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                }
            }

            // Special case where the UtilitySelectorComponent has no UtilityValueComponent children.
            if (!hasUtilityValueComponent) {
                foreach (var (utilitySelectorComponents, taskComponents, branchComponents) in
                    SystemAPI.Query<DynamicBuffer<UtilitySelectorComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<BranchComponent>>().WithAll<UtilitySelectorTag, EvaluationComponent>()) {

                    for (int i = 0; i < utilitySelectorComponents.Length; ++i) {
                        var utilitySelectorComponent = utilitySelectorComponents[i];
                        var taskComponent = taskComponents[utilitySelectorComponent.Index];

                        // If there are no values then the selector should return failure.
                        if (taskComponent.Status == TaskStatus.Queued && utilitySelectorComponent.UtilityItems.Length == 0) {
                            taskComponent.Status = TaskStatus.Failure;
                            var taskComponentsBuffer = taskComponents;
                            taskComponentsBuffer[utilitySelectorComponent.Index] = taskComponent;

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
        /// Retruns the task index with the highest utility value.
        /// </summary>
        /// <param name="utilityItems">The components with utility values.</param>
        /// <param name="utilityValueComponents">The utility values of the tasks.</param>
        /// <returns>The task with the highest utility value.</returns>
        [BurstCompile]
        private ushort GetHighestUtility(NativeArray<UtilitySelectorComponent.UtilityItem> utilityItems, ref DynamicBuffer<UtilityValueComponent> utilityValueComponents)
        {
            var utilityIndex = ushort.MaxValue;
            var highestUtility = float.MinValue;
            for (ushort i = 0; i < utilityItems.Length; ++i) {
                if (utilityItems[i].UtilityValueIndex == ushort.MaxValue || !utilityItems[i].CanExecute) {
                    continue;
                }

                var utilityValue = utilityValueComponents[utilityItems[i].UtilityValueIndex].Value;
                if (utilityValue > highestUtility) {
                    highestUtility = utilityValue;
                    utilityIndex = i;
                }
            }

            return utilityIndex;
        }

        /// <summary>
        /// The task has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            foreach (var utilitySelectorComponents in SystemAPI.Query<DynamicBuffer<UtilitySelectorComponent>>()) {
                for (int i = 0; i < utilitySelectorComponents.Length; ++i) {
                    utilitySelectorComponents[i].UtilityItems.Dispose();
                }
            }
        }
    }
}
#endif
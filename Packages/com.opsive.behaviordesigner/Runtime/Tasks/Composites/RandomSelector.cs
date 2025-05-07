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
    /// A node representation of the random selector task.
    /// </summary>
    [NodeIcon("d7c1e0f5830316e449df8a35561df859", "7638e4bc5a1f4cd488801902387ec5ea")]
    [NodeDescription("Similar to the selector task, the random selector task will return success as soon as a child task returns success.  " +
                     "The difference is that the random selector class will run its children in a random order. The selector task is deterministic " +
                     "in that it will always run the tasks from left to right within the tree. The random selector task shuffles the child tasks up and then begins " +
                     "execution in a random order. Other than that the random selector class is the same as the selector class. It will continue running tasks " +
                     "until a task completes successfully. If no child tasks return success then it will return failure.")]
    public struct RandomSelector : ILogicNode, IParentNode, ITaskComponentData, IComposite, IConditionalAbortParent, IInterruptResponder, ISavableTask, ICloneable
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("Specifies how the child conditional tasks should be reevaluated.")]
        [SerializeField] ConditionalAbortType m_AbortType;
        [Tooltip("The seed of the random number generator. Set to 0 to use the entity index as the seed.")]
        [SerializeField] uint m_Seed;

        private ushort m_ComponentIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public ConditionalAbortType AbortType { get => m_AbortType; set => m_AbortType = value; }
        public uint Seed { get => m_Seed; set => m_Seed = value; }

        public int MaxChildCount { get { return int.MaxValue; } }

        public ComponentType Tag { get => typeof(RandomSelectorTag); }
        public Type SystemType { get => typeof(RandomSelectorTaskSystem); }
        public Type InterruptSystemType { get => typeof(RandomSelectorInterruptSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>Re
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RandomSelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomSelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomSelectorComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<RandomSelectorComponent>(entity);
            }
            buffer.Add(new RandomSelectorComponent() {
                Index = RuntimeIndex,
                Seed = m_Seed,
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
            DynamicBuffer<RandomSelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomSelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomSelectorComponent>(entity);
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
            var randomSelectorComponents = world.EntityManager.GetBuffer<RandomSelectorComponent>(entity);
            var randomSelectorComponent = randomSelectorComponents[m_ComponentIndex];

            // Save the active child and array order.
            var saveData = new object[2];
            saveData[0] = randomSelectorComponent.ActiveRelativeChildIndex;
            if (randomSelectorComponent.TaskOrder.IsCreated) {
                saveData[1] = randomSelectorComponent.TaskOrder.ToArray();
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
            var randomSelectorComponents = world.EntityManager.GetBuffer<RandomSelectorComponent>(entity);
            var randomSelectorComponent = randomSelectorComponents[m_ComponentIndex];

            // saveData is the active child and array order.
            var taskSaveData = (object[])saveData;
            randomSelectorComponent.ActiveRelativeChildIndex = (ushort)taskSaveData[0];
            if (taskSaveData[1] != null) {
                randomSelectorComponent.TaskOrder = new NativeArray<ushort>((ushort[])taskSaveData[1], Allocator.Persistent);
            }
            randomSelectorComponents[m_ComponentIndex] = randomSelectorComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<RandomSelector>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the RandomSelector class.
    /// </summary>
    public struct RandomSelectorComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The relative index of the child that is currently active.")]
        public ushort ActiveRelativeChildIndex;
        [Tooltip("The seed of the random number generator.")]
        public uint Seed;
        [Tooltip("The random number generator for the task.")]
        public Unity.Mathematics.Random RandomNumberGenerator;
        [Tooltip("The indicies of the child task execution order.")]
        public NativeArray<ushort> TaskOrder;
    }

    /// <summary>
    /// A DOTS tag indicating when a RandomSelector node is active.
    /// </summary>
    public struct RandomSelectorTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the RandomSelector logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RandomSelectorTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (branchComponents, taskComponents, randomSelectorComponents, entity) in
                SystemAPI.Query<DynamicBuffer<BranchComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<RandomSelectorComponent>> ().WithAll<RandomSelectorTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < randomSelectorComponents.Length; ++i) {
                    var randomSelectorComponent = randomSelectorComponents[i];
                    var taskComponent = taskComponents[randomSelectorComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    var randomSelectorComponentsBuffer = randomSelectorComponents;
                    var taskComponentsBuffer = taskComponents;
                    var branchComponentBuffer = branchComponents;
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponentsBuffer[taskComponent.Index] = taskComponent;

                        // Initialize the task order array.
                        if (randomSelectorComponent.TaskOrder.Length == 0) {
                            var childCount = TraversalUtility.GetImmediateChildCount(ref taskComponent, ref taskComponentsBuffer);
                            var taskOrder = new NativeArray<ushort>(childCount, Allocator.Persistent);
                            var childIndex = taskComponent.Index + 1;
                            for (int j = 0; j < childCount; ++j) {
                                taskOrder[j] = (ushort)childIndex;
                                childIndex = taskComponents[childIndex].SiblingIndex;
                            }
                            randomSelectorComponent.TaskOrder = taskOrder;
                        }

                        // Generate a new random number seed for each entity.
                        if (randomSelectorComponent.RandomNumberGenerator.state == 0) {
                            randomSelectorComponent.RandomNumberGenerator = Unity.Mathematics.Random.CreateFromIndex(randomSelectorComponent.Seed != 0 ? randomSelectorComponent.Seed : (uint)entity.Index);
                        }

                        // Use fisher-yates to shuffle the array in place.
                        var index = randomSelectorComponent.TaskOrder.Length;
                        while (index != 0) {
                            var randomUnitFloat = randomSelectorComponent.RandomNumberGenerator.NextFloat();
                            var randomIndex = (int)Unity.Mathematics.math.floor(randomUnitFloat * index);
                            index--;

                            var element = randomSelectorComponent.TaskOrder[randomIndex];
                            randomSelectorComponent.TaskOrder[randomIndex] = randomSelectorComponent.TaskOrder[index];
                            randomSelectorComponent.TaskOrder[index] = element;
                        }

                        randomSelectorComponent.ActiveRelativeChildIndex = 0;
                        randomSelectorComponentsBuffer[i] = randomSelectorComponent;

                        branchComponent.NextIndex = randomSelectorComponent.TaskOrder[randomSelectorComponent.ActiveRelativeChildIndex];
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;

                        // The child may have already ran and have a non-inactive status.
                        var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                        nextChildTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[branchComponent.NextIndex] = nextChildTaskComponent;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The randomSelector task is currently active. Check the first child.
                    var childTaskComponent = taskComponents[randomSelectorComponent.TaskOrder[randomSelectorComponent.ActiveRelativeChildIndex]];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    if (randomSelectorComponent.ActiveRelativeChildIndex == randomSelectorComponent.TaskOrder.Length - 1 || childTaskComponent.Status == TaskStatus.Success) {
                        // There are no more children or the child succeeded. The random selector task should end. A task status of inactive indicates the last task was disabled. Return failure.
                        taskComponent.Status = childTaskComponent.Status != TaskStatus.Inactive ? childTaskComponent.Status : TaskStatus.Failure;
                        randomSelectorComponent.ActiveRelativeChildIndex = 0;
                        taskComponentsBuffer[randomSelectorComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    } else {
                        // The child task returned failure. Move onto the next task. 
                        randomSelectorComponent.ActiveRelativeChildIndex++;
                        var nextIndex = randomSelectorComponent.TaskOrder[randomSelectorComponent.ActiveRelativeChildIndex];
                        var nextTaskComponent = taskComponents[nextIndex];
                        nextTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[nextIndex] = nextTaskComponent;

                        branchComponent.NextIndex = nextIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    }
                    randomSelectorComponentsBuffer[i] = randomSelectorComponent;
                }
            }
        }

        /// <summary>
        /// The task has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            foreach (var randomSelectorComponents in SystemAPI.Query<DynamicBuffer<RandomSelectorComponent>>()) {
                for (int i = 0; i < randomSelectorComponents.Length; ++i) {
                    var randomSelectorComponent = randomSelectorComponents[i];
                    randomSelectorComponent.TaskOrder.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// An interrupt has occurred. Ensure the task state is correct after the interruption.
    /// </summary>
    [DisableAutoCreation]

    public partial struct RandomSelectorInterruptSystem : ISystem
    {
        /// <summary>
        /// Runs the logic after an interruption.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, randomSelectorComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<RandomSelectorComponent>>().WithAll<InterruptTag>()) {
                for (int i = 0; i < randomSelectorComponents.Length; ++i) {
                    var randomSelectorComponent = randomSelectorComponents[i];
                    // The active child will have a non-running status if it has been interrupted.
                    var taskComponent = taskComponents[randomSelectorComponent.Index];
                    if (taskComponent.Status == TaskStatus.Running && taskComponents[randomSelectorComponent.TaskOrder[randomSelectorComponent.ActiveRelativeChildIndex]].Status != TaskStatus.Running) {
                        ushort relativeChildIndex = 0;
                        // Find the currently active task.
                        while (taskComponents[randomSelectorComponent.TaskOrder[relativeChildIndex]].Status != TaskStatus.Running) {
                            relativeChildIndex++;
                        }
                        randomSelectorComponent.ActiveRelativeChildIndex = relativeChildIndex;
                        var randomSelectorBuffer = randomSelectorComponents;
                        randomSelectorBuffer[i] = randomSelectorComponent;
                    }
                }
            }
        }
    }
}
#endif
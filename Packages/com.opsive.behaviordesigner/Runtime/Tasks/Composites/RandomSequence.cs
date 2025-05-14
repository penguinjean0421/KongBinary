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
    /// A node representation of the random sequence task.
    /// </summary>
    [NodeIcon("edb30349221143a408c76da55a6aa809", "cfb9039832ed52748b617bde070898dc")]
    [NodeDescription("Similar to the sequence task, the random sequence task will return success as soon as every child task returns success.  " +
                     "The difference is that the random sequence class will run its children in a random order. The sequence task is deterministic " +
                     "in that it will always run the tasks from left to right within the tree. The random sequence task shuffles the child tasks up and then begins " +
                     "execution in a random order. Other than that the random sequence class is the same as the sequence class. It will stop running tasks " +
                     "as soon as a single task ends in failure. On a task failure it will stop executing all of the child tasks and return failure. " +
                     "If no child returns failure then it will return success.")]
    public struct RandomSequence : ILogicNode, IParentNode, ITaskComponentData, IComposite, IConditionalAbortParent, IInterruptResponder, ISavableTask, ICloneable
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

        public ComponentType Tag { get => typeof(RandomSequenceTag); }
        public Type SystemType { get => typeof(RandomSequenceTaskSystem); }
        public Type InterruptSystemType { get => typeof(RandomSequenceInterruptSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RandomSequenceComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomSequenceComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomSequenceComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<RandomSequenceComponent>(entity);
            }
            buffer.Add(new RandomSequenceComponent() {
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
            DynamicBuffer<RandomSequenceComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomSequenceComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomSequenceComponent>(entity);
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
            var randomSequenceComponents = world.EntityManager.GetBuffer<RandomSequenceComponent>(entity);
            var randomSequenceComponent = randomSequenceComponents[m_ComponentIndex];

            // Save the active child and array order.
            var saveData = new object[2];
            saveData[0] = randomSequenceComponent.ActiveRelativeChildIndex;
            if (randomSequenceComponent.TaskOrder.IsCreated) {
                saveData[1] = randomSequenceComponent.TaskOrder.ToArray();
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
            var randomSequenceComponents = world.EntityManager.GetBuffer<RandomSequenceComponent>(entity);
            var randomSequenceComponent = randomSequenceComponents[m_ComponentIndex];

            // saveData is the active child and array order.
            var taskSaveData = (object[])saveData;
            randomSequenceComponent.ActiveRelativeChildIndex = (ushort)taskSaveData[0];
            if (taskSaveData[1] != null) {
                randomSequenceComponent.TaskOrder = new NativeArray<ushort>((ushort[])taskSaveData[1], Allocator.Persistent);
            }
            randomSequenceComponents[m_ComponentIndex] = randomSequenceComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<RandomSequence>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the RandomSequence class.
    /// </summary>
    public struct RandomSequenceComponent : IBufferElementData
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
    /// A DOTS tag indicating when a RandomSequence node is active.
    /// </summary>
    public struct RandomSequenceTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the RandomSequence logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RandomSequenceTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (branchComponents, taskComponents, randomSequenceComponents, entity) in
                SystemAPI.Query<DynamicBuffer<BranchComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<RandomSequenceComponent>>().WithAll<RandomSequenceTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < randomSequenceComponents.Length; ++i) {
                    var randomSequenceComponent = randomSequenceComponents[i];
                    var taskComponent = taskComponents[randomSequenceComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    var randomSequenceComponentsBuffer = randomSequenceComponents;
                    var taskComponentsBuffer = taskComponents;
                    var branchComponentBuffer = branchComponents;
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponentsBuffer[taskComponent.Index] = taskComponent;

                        // Initialize the task order array.
                        if (randomSequenceComponent.TaskOrder.Length == 0) {
                            var childCount = TraversalUtility.GetImmediateChildCount(ref taskComponent, ref taskComponentsBuffer);
                            var taskOrder = new NativeArray<ushort>(childCount, Allocator.Persistent);
                            var childIndex = taskComponent.Index + 1;
                            for (int j = 0; j < childCount; ++j) {
                                taskOrder[j] = (ushort)childIndex;
                                childIndex = taskComponents[childIndex].SiblingIndex;
                            }
                            randomSequenceComponent.TaskOrder = taskOrder;
                        }

                        // Generate a new random number seed for each entity.
                        if (randomSequenceComponent.RandomNumberGenerator.state == 0) {
                            randomSequenceComponent.RandomNumberGenerator = Unity.Mathematics.Random.CreateFromIndex(randomSequenceComponent.Seed != 0 ? randomSequenceComponent.Seed : (uint)entity.Index);
                        }

                        // Use fisher-yates to shuffle the array in place.
                        var index = randomSequenceComponent.TaskOrder.Length;
                        while (index != 0) {
                            var randomUnitFloat = randomSequenceComponent.RandomNumberGenerator.NextFloat();
                            var randomIndex = (int)Unity.Mathematics.math.floor(randomUnitFloat * index);
                            index--;

                            var element = randomSequenceComponent.TaskOrder[randomIndex];
                            randomSequenceComponent.TaskOrder[randomIndex] = randomSequenceComponent.TaskOrder[index];
                            randomSequenceComponent.TaskOrder[index] = element;
                        }

                        randomSequenceComponent.ActiveRelativeChildIndex = 0;
                        randomSequenceComponentsBuffer[i] = randomSequenceComponent;

                        branchComponent.NextIndex = randomSequenceComponent.TaskOrder[randomSequenceComponent.ActiveRelativeChildIndex];
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;

                        // Start the child.
                        var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                        nextChildTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[branchComponent.NextIndex] = nextChildTaskComponent;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The randomSequence task is currently active. Check the first child.
                    var childTaskComponent = taskComponents[randomSequenceComponent.TaskOrder[randomSequenceComponent.ActiveRelativeChildIndex]];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    if (randomSequenceComponent.ActiveRelativeChildIndex == randomSequenceComponent.TaskOrder.Length - 1 || childTaskComponent.Status == TaskStatus.Failure) {
                        // There are no more children or the child failed. The random sequence task should end. A task status of inactive indicates the last task was disabled. Return success.
                        taskComponent.Status = childTaskComponent.Status != TaskStatus.Inactive ? childTaskComponent.Status : TaskStatus.Success;
                        randomSequenceComponent.ActiveRelativeChildIndex = 0;
                        taskComponentsBuffer[randomSequenceComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    } else {
                        // The child task returned success. Move onto the next task. 
                        randomSequenceComponent.ActiveRelativeChildIndex++;
                        var nextIndex = randomSequenceComponent.TaskOrder[randomSequenceComponent.ActiveRelativeChildIndex];
                        var nextTaskComponent = taskComponents[nextIndex];
                        nextTaskComponent.Status = TaskStatus.Queued;
                        taskComponentsBuffer[nextIndex] = nextTaskComponent;

                        branchComponent.NextIndex = nextIndex;
                        branchComponentBuffer[taskComponent.BranchIndex] = branchComponent;
                    }
                    randomSequenceComponentsBuffer[i] = randomSequenceComponent;
                }
            }
        }

        /// <summary>
        /// The task has been destroyed.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnDestroy(ref SystemState state)
        {
            foreach (var randomSequenceComponents in SystemAPI.Query<DynamicBuffer<RandomSequenceComponent>>()) {
                for (int i = 0; i < randomSequenceComponents.Length; ++i) {
                    var randomSequenceComponent = randomSequenceComponents[i];
                    randomSequenceComponent.TaskOrder.Dispose();
                }
            }
        }
    }

    /// <summary>
    /// An interrupt has occurred. Ensure the task state is correct after the interruption.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RandomSequenceInterruptSystem : ISystem
    {
        /// <summary>
        /// Runs the logic after an interruption.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, randomSequenceComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<RandomSequenceComponent>>().WithAll<InterruptTag>()) {
                for (int i = 0; i < randomSequenceComponents.Length; ++i) {
                    var randomSequenceComponent = randomSequenceComponents[i];
                    // The active child will have a non-running status if it has been interrupted.
                    var taskComponent = taskComponents[randomSequenceComponent.Index];
                    if (taskComponent.Status == TaskStatus.Running && taskComponents[randomSequenceComponent.TaskOrder[randomSequenceComponent.ActiveRelativeChildIndex]].Status != TaskStatus.Running) {
                        ushort relativeChildIndex = 0;
                        // Find the currently active task.
                        while (taskComponents[randomSequenceComponent.TaskOrder[relativeChildIndex]].Status != TaskStatus.Running) {
                            relativeChildIndex++;
                        }
                        randomSequenceComponent.ActiveRelativeChildIndex = relativeChildIndex;
                        var randomSequenceBuffer = randomSequenceComponents;
                        randomSequenceBuffer[i] = randomSequenceComponent;
                    }
                }
            }
        }
    }
}
#endif
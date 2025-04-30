#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Composites
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.Shared.Utility;
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;
    using System;

    /// <summary>
    /// A node representation of the selector task.
    /// </summary>
    [NodeIcon("4c3d0559a9ebc604e88b16e9a3fdfa05", "de3acf0e386a26246b8bc999b1ef8e32")]
    [NodeDescription("The selector task is similar to an \"or\" operation. It will return success as soon as one of its child tasks return success. " +
                     "If a child task returns failure then it will sequentially run the next task. If no child task returns success then it will return failure.")]
    public struct Selector : ILogicNode, IParentNode, ITaskComponentData, IComposite, IConditionalAbortParent, IInterruptResponder, ISavableTask, ICloneable
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("Specifies how the child conditional tasks should be reevaluated.")]
        [SerializeField] ConditionalAbortType m_AbortType;

        private ushort m_ComponentIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public ConditionalAbortType AbortType { get => m_AbortType; set => m_AbortType = value; }

        public int MaxChildCount { get { return int.MaxValue; } }

        public ComponentType Tag { get => typeof(SelectorTag); }
        public Type SystemType { get => typeof(SelectorTaskSystem); }
        public Type InterruptSystemType { get => typeof(SelectorInterruptSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<SelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<SelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SelectorComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<SelectorComponent>(entity);
            }
            buffer.Add(new SelectorComponent() {
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
            DynamicBuffer<SelectorComponent> buffer;
            if (world.EntityManager.HasBuffer<SelectorComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SelectorComponent>(entity);
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
            var selectorComponents = world.EntityManager.GetBuffer<SelectorComponent>(entity);
            var selectorComponent = selectorComponents[m_ComponentIndex];

            // Save the active child.
            return selectorComponent.ActiveChildIndex;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var selectorComponents = world.EntityManager.GetBuffer<SelectorComponent>(entity);
            var selectorComponent = selectorComponents[m_ComponentIndex];

            // saveData is the active child.
            selectorComponent.ActiveChildIndex = (ushort)saveData;
            selectorComponents[m_ComponentIndex] = selectorComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<Selector>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the Selector class.
    /// </summary>
    public struct SelectorComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The index of the child that is currently active.")]
        public ushort ActiveChildIndex;
    }

    /// <summary>
    /// A DOTS tag indicating when a Selector node is active.
    /// </summary>
    public struct SelectorTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Selector logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct SelectorTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<SelectorComponent>().WithAll<SelectorTag, EvaluationComponent>().Build();
            state.Dependency = new SelectorJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct SelectorJob : IJobEntity
        {
            /// <summary>
            /// Executes the selector logic.
            /// </summary>
            /// <param name="branchComponents">An array of BranchComponents.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="selectorComponents">An array of SelectorComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<SelectorComponent> selectorComponents)
            {
                for (int i = 0; i < selectorComponents.Length; ++i) {
                    var selectorComponent = selectorComponents[i];
                    var taskComponent = taskComponents[selectorComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[taskComponent.Index] = taskComponent;

                        selectorComponent.ActiveChildIndex = (ushort)(taskComponent.Index + 1);
                        selectorComponents[i] = selectorComponent;

                        branchComponent.NextIndex = selectorComponent.ActiveChildIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;

                        // Start the child.
                        var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                        nextChildTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[branchComponent.NextIndex] = nextChildTaskComponent;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The selector task is currently active. Check the first child.
                    var childTaskComponent = taskComponents[selectorComponent.ActiveChildIndex];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    if (childTaskComponent.SiblingIndex == ushort.MaxValue || childTaskComponent.Status == TaskStatus.Success) {
                        // There are no more children or the child succeeded. The selector task should end. A task status of inactive indicates the last task was disabled. Return failure.
                        taskComponent.Status = childTaskComponent.Status != TaskStatus.Inactive ? childTaskComponent.Status : TaskStatus.Failure;
                        selectorComponent.ActiveChildIndex = (ushort)(selectorComponent.Index + 1);
                        taskComponents[selectorComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                    } else {
                        // The previous task is no longer running. 
                        var siblingTaskComponent = taskComponents[childTaskComponent.SiblingIndex];

                        siblingTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[childTaskComponent.SiblingIndex] = siblingTaskComponent;
                        // The current index is now the sibling index.
                        selectorComponent.ActiveChildIndex = childTaskComponent.SiblingIndex;

                        branchComponent.NextIndex = selectorComponent.ActiveChildIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                    }
                    selectorComponents[i] = selectorComponent;
                }
            }
        }
    }

    /// <summary>
    /// An interrupt has occurred. Ensure the task state is correct after the interruption.
    /// </summary>
    [DisableAutoCreation]
    public partial struct SelectorInterruptSystem : ISystem
    {
        /// <summary>
        /// Runs the logic after an interruption.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, selectorComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<SelectorComponent>>().WithAll<InterruptTag>()) {
                for (int i = 0; i < selectorComponents.Length; ++i) {
                    var selectorComponent = selectorComponents[i];
                    // The active child will have a non-running status if it has been interrupted.
                    if (taskComponents[selectorComponent.ActiveChildIndex].Status != TaskStatus.Running) {
                        var childIndex = (ushort)(selectorComponent.Index + 1);
                        // Find the currently active task.
                        while (childIndex != ushort.MaxValue && taskComponents[childIndex].Status != TaskStatus.Running) {
                            childIndex = taskComponents[childIndex].SiblingIndex;
                        }
                        if (childIndex != ushort.MaxValue) {
                            selectorComponent.ActiveChildIndex = childIndex;
                        }
                        var selectorBuffer = selectorComponents;
                        selectorBuffer[i] = selectorComponent;
                    }
                }
            }
        }
    }
}
#endif
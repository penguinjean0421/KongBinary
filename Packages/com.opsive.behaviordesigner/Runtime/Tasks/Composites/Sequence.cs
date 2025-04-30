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
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;
    using System;

    /// <summary>
    /// A node representation of the sequence task.
    /// </summary>
    [NodeIcon("8981cc246f900b24da46ae10eb49b68b", "4a7b39d8e0d056a4a9d8eb390b4bc9b8")]
    [NodeDescription("The sequence task is similar to an \"and\" operation. It will return failure as soon as one of its child tasks return failure. " +
                     "If a child task returns success then it will sequentially run the next task. If all child tasks return success then it will return success.")]
    public struct Sequence : ILogicNode, IParentNode, ITaskComponentData, IComposite, IConditionalAbortParent, IInterruptResponder, ISavableTask, ICloneable
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
        public ushort ParentIndex { get => m_ParentIndex; set { m_ParentIndex = value; } }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public ConditionalAbortType AbortType { get => m_AbortType; set => m_AbortType = value; }

        public int MaxChildCount { get { return int.MaxValue; } }

        public ComponentType Tag { get => typeof(SequenceTag); }
        public Type SystemType { get => typeof(SequenceTaskSystem); }
        public Type InterruptSystemType { get => typeof(SequenceInterruptSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<SequenceComponent> buffer;
            if (world.EntityManager.HasBuffer<SequenceComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SequenceComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<SequenceComponent>(entity);
            }
            buffer.Add(new SequenceComponent() {
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
            DynamicBuffer<SequenceComponent> buffer;
            if (world.EntityManager.HasBuffer<SequenceComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<SequenceComponent>(entity);
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
            var sequenceComponents = world.EntityManager.GetBuffer<SequenceComponent>(entity);
            var sequenceComponent = sequenceComponents[m_ComponentIndex];

            // Save the active child.
            return sequenceComponent.ActiveChildIndex;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var sequenceComponents = world.EntityManager.GetBuffer<SequenceComponent>(entity);
            var sequenceComponent = sequenceComponents[m_ComponentIndex];

            // saveData is the active child.
            sequenceComponent.ActiveChildIndex = (ushort)saveData;
            sequenceComponents[m_ComponentIndex] = sequenceComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<Sequence>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            clone.AbortType = AbortType;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the Sequence class.
    /// </summary>
    public struct SequenceComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The index of the child that is currently active.")]
        public ushort ActiveChildIndex;
    }

    /// <summary>
    /// A DOTS tag indicating when a Sequence node is active.
    /// </summary>
    public struct SequenceTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Sequence logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct SequenceTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<SequenceComponent>().WithAll<SequenceTag, EvaluationComponent>().Build();
            state.Dependency = new SequenceJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct SequenceJob : IJobEntity
        {
            /// <summary>
            /// Executes the sequence logic.
            /// </summary>
            /// <param name="branchComponents">An array of BranchComponents.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="sequenceComponents">An array of SequenceComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<SequenceComponent> sequenceComponents)
            {
                for (int i = 0; i < sequenceComponents.Length; ++i) {
                    var sequenceComponent = sequenceComponents[i];
                    var taskComponent = taskComponents[sequenceComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];

                    // Do not continue if there will be an interrupt.
                    if (branchComponent.InterruptType != InterruptType.None) {
                        continue;
                    }

                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[taskComponent.Index] = taskComponent;

                        sequenceComponent.ActiveChildIndex = (ushort)(taskComponent.Index + 1);
                        sequenceComponents[i] = sequenceComponent;

                        branchComponent.NextIndex = sequenceComponent.ActiveChildIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;

                        // Start the child.
                        var nextChildTaskComponent = taskComponents[branchComponent.NextIndex];
                        nextChildTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[branchComponent.NextIndex] = nextChildTaskComponent;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The sequence task is currently active. Check the first child.
                    var childTaskComponent = taskComponents[sequenceComponent.ActiveChildIndex];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    if (childTaskComponent.SiblingIndex == ushort.MaxValue || childTaskComponent.Status == TaskStatus.Failure) {
                        // There are no more children or the child failed. The sequence task should end. A task status of inactive indicates the last task was disabled. Return success.
                        taskComponent.Status = childTaskComponent.Status != TaskStatus.Inactive ? childTaskComponent.Status : TaskStatus.Success;
                        sequenceComponent.ActiveChildIndex = (ushort)(sequenceComponent.Index + 1);
                        taskComponents[sequenceComponent.Index] = taskComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                    } else {
                        // The previous task is no longer running. 
                        var siblingTaskComponent = taskComponents[childTaskComponent.SiblingIndex];

                        siblingTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[childTaskComponent.SiblingIndex] = siblingTaskComponent;
                        // The current index is now the sibling index.
                        sequenceComponent.ActiveChildIndex = childTaskComponent.SiblingIndex;

                        branchComponent.NextIndex = sequenceComponent.ActiveChildIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                    }
                    sequenceComponents[i] = sequenceComponent;
                }
            }
        }
    }

    /// <summary>
    /// An interrupt has occurred. Ensure the task state is correct after the interruption.
    /// </summary>
    [DisableAutoCreation]
    public partial struct SequenceInterruptSystem : ISystem
    {
        /// <summary>
        /// Runs the logic after an interruption.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, sequenceComponents) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<SequenceComponent>>().WithAll<InterruptTag>()) {
                for (int i = 0; i < sequenceComponents.Length; ++i) {
                    var sequenceComponent = sequenceComponents[i];
                    if (taskComponents[sequenceComponent.ActiveChildIndex].Status != TaskStatus.Running) {
                        var childIndex = (ushort)(sequenceComponent.Index + 1);
                        // Find the currently active task.
                        while (childIndex != ushort.MaxValue && taskComponents[childIndex].Status != TaskStatus.Running) {
                            childIndex = taskComponents[childIndex].SiblingIndex;
                        }
                        if (childIndex != ushort.MaxValue) {
                            sequenceComponent.ActiveChildIndex = childIndex;
                        }
                        var sequenceBuffer = sequenceComponents;
                        sequenceBuffer[i] = sequenceComponent;
                    }
                }
            }
        }
    }
}
#endif
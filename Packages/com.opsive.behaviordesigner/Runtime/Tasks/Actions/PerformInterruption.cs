#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Utility;
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;
    using Unity.Collections;

    /// <summary>
    /// A node representation of the perform interruption task.
    /// </summary>
    [NodeIcon("7c0aba0d8377aac48966d8e3f817a2a8", "90105f40f82a30e45b08d150c1928950")]
    [NodeDescription("Performs the actual interruption. This will immediately stop the specified tasks from running and will return success or failure depending on the value of interrupt success.")]
    public struct PerformInterruption : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The task that should be interrupted.")]
        [SelectableNode]
        [SerializeField] ILogicNode[] m_InterruptTasks;
        [Tooltip("Should the interrupted task return success?")]
        [SerializeField] bool m_InterruptSuccess;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(PerformInterruptionTag); }
        public System.Type SystemType { get => typeof(PerformInterruptionTaskSystem); }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<PerformInterruptionComponent> buffer;
            if (world.EntityManager.HasBuffer<PerformInterruptionComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PerformInterruptionComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<PerformInterruptionComponent>(entity);
            }
            
            var indicies = new ushort[m_InterruptTasks.Length];
            var nullTaskCount = 0;
            for (int i = 0; i < m_InterruptTasks.Length; ++i) {
                if (m_InterruptTasks[i] == null) {
                    nullTaskCount++;
                    continue;
                }
                indicies[i - nullTaskCount] = m_InterruptTasks[i].Index;
            }
            if (nullTaskCount > 0) {
                System.Array.Resize(ref indicies, indicies.Length - nullTaskCount);
            }
            buffer.Add(new PerformInterruptionComponent() {
                Index = RuntimeIndex,
                InterruptIndicies = new NativeArray<ushort>(indicies, Allocator.Persistent),
                InterruptSuccess = m_InterruptSuccess
            });

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
            DynamicBuffer<PerformInterruptionComponent> buffer;
            if (world.EntityManager.HasBuffer<PerformInterruptionComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PerformInterruptionComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the PerformInterruption class.
    /// </summary>
    public struct PerformInterruptionComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        [SerializeField] public ushort Index;
        [Tooltip("The indicies of the tasks that should be interrupted.")]
        [SerializeField] public NativeArray<ushort> InterruptIndicies;
        [Tooltip("Should the interrupted tasks return success?")]
        [SerializeField] public bool InterruptSuccess;
    }

    /// <summary>
    /// A DOTS tag indicating when a PerformInterruption node is active.
    /// </summary>
    public struct PerformInterruptionTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the PerformInterruption logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct PerformInterruptionTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (branchComponents, taskComponents, performInterruptionComponents, entity) in
                SystemAPI.Query<DynamicBuffer<BranchComponent>, DynamicBuffer<TaskComponent>, DynamicBuffer<PerformInterruptionComponent>>().WithAll<PerformInterruptionTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < performInterruptionComponents.Length; ++i) {
                    var performInterruptionComponent = performInterruptionComponents[i];
                    var taskComponent = taskComponents[performInterruptionComponent.Index];

                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Success;
                        var taskComponentsBuffer = taskComponents;
                        taskComponentsBuffer[taskComponent.Index] = taskComponent;

                        var branchComponentsBuffer = branchComponents;
                        for (int j = 0; j < performInterruptionComponent.InterruptIndicies.Length; ++j) {
                            var interruptTaskComponent = taskComponents[performInterruptionComponent.InterruptIndicies[j]];
                            var interruptBranchComponent = branchComponents[interruptTaskComponent.BranchIndex];
                            interruptBranchComponent.InterruptType = performInterruptionComponent.InterruptSuccess ? InterruptType.ImmediateSuccess : InterruptType.ImmediateFailure;
                            interruptBranchComponent.InterruptIndex = interruptTaskComponent.Index;
                            branchComponentsBuffer[interruptTaskComponent.BranchIndex] = interruptBranchComponent;
                        }

                        state.EntityManager.SetComponentEnabled<InterruptTag>(entity, true);
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
            foreach (var performInterruptionComponents in SystemAPI.Query<DynamicBuffer<PerformInterruptionComponent>>()) {
                for (int i = 0; i < performInterruptionComponents.Length; ++i) {
                    var performInterruptionComponent = performInterruptionComponents[i];
                    performInterruptionComponent.InterruptIndicies.Dispose();
                }
            }
        }
    }
}
#endif
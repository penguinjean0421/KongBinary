#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.Shared.Utility;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// A node representation of the cooldown task.
    /// </summary>
    [NodeIcon("b5459f67bc5033e49ad7a763cdb885bb", "480c79a18119d2a488b5d984211463f1")]
    [NodeDescription("Waits the specified duration after the child has completed before returning the child's status of success or failure.")]
    public struct Cooldown : ILogicNode, IParentNode, ITaskComponentData, IDecorator
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The duration of the cooldown.")]
        [SerializeField] float m_Duration;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public float Duration { get => m_Duration; set => m_Duration = value; }

        private ushort m_ComponentIndex;

        public int MaxChildCount { get { return 1; } }

        public ComponentType Tag { get => typeof(CooldownTag); }
        public System.Type SystemType { get => typeof(CooldownTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_Duration = 1; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<CooldownComponent> buffer;
            if (world.EntityManager.HasBuffer<CooldownComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<CooldownComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<CooldownComponent>(entity);
            }
            buffer.Add(new CooldownComponent() {
                Index = RuntimeIndex,
                Duration = m_Duration,
                StartTime = -1,
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
            DynamicBuffer<CooldownComponent> buffer;
            if (world.EntityManager.HasBuffer<CooldownComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<CooldownComponent>(entity);
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
            var cooldownComponents = world.EntityManager.GetBuffer<CooldownComponent>(entity);
            var cooldownComponent = cooldownComponents[m_ComponentIndex];

            // Save the elapsed time.
            return Time.time - cooldownComponent.StartTime;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var cooldownComponents = world.EntityManager.GetBuffer<CooldownComponent>(entity);
            var cooldownComponent = cooldownComponents[m_ComponentIndex];

            // saveData is the elapsed amount of time.
            var data = (object[])saveData;
            cooldownComponent.StartTime = Time.time - (double)saveData;
            cooldownComponents[m_ComponentIndex] = cooldownComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<Cooldown>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            clone.Duration = Duration;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the Cooldown class.
    /// </summary>
    public struct CooldownComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The duration of the cooldown.")]
        public float Duration;
        [Tooltip("The time the cooldown started.")]
        public double StartTime;
    }

    /// <summary>
    /// A DOTS tag indicating when an Cooldown node is active.
    /// </summary>
    public struct CooldownTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Cooldown logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct CooldownTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<BranchComponent>().WithAllRW<TaskComponent>().WithAllRW<CooldownComponent>().WithAll<CooldownTag, EvaluationComponent>().Build();
            state.Dependency = new CooldownJob()
            {
                Time = SystemAPI.Time.ElapsedTime
            }.ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct CooldownJob : IJobEntity
        {
            [Tooltip("The elapsed time.")]
            public double Time;

            /// <summary>
            /// Executes the cooldown logic.
            /// </summary>
            /// <param name="branchComponents">An array of BranchComponents.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="cooldownComponents">An array of CooldownComponents.</param>
            [BurstCompile]
            public void Execute(ref DynamicBuffer<BranchComponent> branchComponents, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<CooldownComponent> cooldownComponents)
            {
                for (int i = 0; i < cooldownComponents.Length; ++i) {
                    var cooldownComponent = cooldownComponents[i];
                    var taskComponent = taskComponents[cooldownComponent.Index];
                    var branchComponent = branchComponents[taskComponent.BranchIndex];
                    TaskComponent childTaskComponent;

                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        taskComponents[taskComponent.Index] = taskComponent;

                        childTaskComponent = taskComponents[taskComponent.Index + 1];
                        childTaskComponent.Status = TaskStatus.Queued;
                        taskComponents[taskComponent.Index + 1] = childTaskComponent;

                        branchComponent.NextIndex = taskComponent.Index + 1;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;

                        continue;
                    } else if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // The cooldown task is currently active. Check the first child.
                    childTaskComponent = taskComponents[taskComponent.Index + 1];
                    if (childTaskComponent.Status == TaskStatus.Queued || childTaskComponent.Status == TaskStatus.Running) {
                        // The child should keep running.
                        continue;
                    }

                    // The child has completed. Start the timer if it hasn't already started. If it has started then complete when the duration has elapsed.
                    if (cooldownComponent.StartTime == -1) {
                        cooldownComponent.StartTime = Time;
                        cooldownComponents[i] = cooldownComponent;
                    } else if (cooldownComponent.StartTime + cooldownComponent.Duration <= Time) {
                        taskComponent.Status = childTaskComponent.Status;
                        taskComponents[taskComponent.Index] = taskComponent;

                        cooldownComponent.StartTime = -1;
                        var cooldownComponentsBuffer = cooldownComponents;
                        cooldownComponentsBuffer[i] = cooldownComponent;

                        branchComponent.NextIndex = taskComponent.ParentIndex;
                        branchComponents[taskComponent.BranchIndex] = branchComponent;
                    }
                }
            }
        }
    }
}
#endif
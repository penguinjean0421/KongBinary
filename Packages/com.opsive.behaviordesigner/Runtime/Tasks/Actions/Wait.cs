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
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using Unity.Entities;
    using Unity.Burst;
    using UnityEngine;
    using System;

    [NodeIcon("b4b59e888607422409f1efa599af34ae", "e1cb9cb566a90fb4489bf31465b99747")]
    [NodeDescription("Wait a specified amount of time. The task will return running until the task is done waiting. It will return success after the wait time has elapsed.")]
    public struct Wait : ILogicNode, ITaskComponentData, IAction, ICloneable, ISavableTask
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The amount of time to wait (in seconds).")]
        [SerializeField] float m_Duration;
        [Tooltip("Should the wait duration be randomized?")]
        [SerializeField] bool m_RandomDuration;
        [Tooltip("The seed of the random number generator. Set to 0 to use the entity index as the seed.")]
        [SerializeField] uint m_Seed;
        [Tooltip("The wait duration range if random wait is enabled.")]
        [SerializeField] RangeFloat m_RandomDurationRange;

        private ushort m_ComponentIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public float Duration { get => m_Duration; set => m_Duration = value; }
        public bool RandomDuration { get => m_RandomDuration; set => m_RandomDuration = value; }
        public uint Seed { get => m_Seed; set => m_Seed = value; }
        public RangeFloat RandomDurationRange { get => m_RandomDurationRange; set => m_RandomDurationRange = value; }

        public ComponentType Tag { get => typeof(WaitTag); }
        public Type SystemType { get => typeof(WaitTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_Duration = m_RandomDurationRange.Min = m_RandomDurationRange.Max = 1; m_RandomDuration = false; m_Seed = 0; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<WaitComponent> buffer;
            if (world.EntityManager.HasBuffer<WaitComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<WaitComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<WaitComponent>(entity);
            }
            buffer.Add(new WaitComponent() {
                Index = RuntimeIndex,
                Duration = m_Duration,
                RandomDuration = m_RandomDuration,
                RandomDurationRange = m_RandomDurationRange,
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
            DynamicBuffer<WaitComponent> buffer;
            if (world.EntityManager.HasBuffer<WaitComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<WaitComponent>(entity);
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
            var waitComponents = world.EntityManager.GetBuffer<WaitComponent>(entity);
            var waitComponent = waitComponents[m_ComponentIndex];

            // Save the unique data.
            return new object[] { waitComponent.WaitDuration, Time.time - waitComponent.StartTime };
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity)
        {
            var waitComponents = world.EntityManager.GetBuffer<WaitComponent>(entity);
            var waitComponent = waitComponents[m_ComponentIndex];

            // saveData is the wait duration and the elapsed amount of time.
            var data = (object[])saveData;
            waitComponent.WaitDuration = (double)data[0];
            waitComponent.StartTime = Time.time - (double)data[1];
            waitComponents[m_ComponentIndex] = waitComponent;
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<Wait>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            clone.Duration = Duration;
            clone.RandomDuration = RandomDuration;
            clone.Seed = Seed;
            clone.RandomDurationRange = RandomDurationRange;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the Wait struct.
    /// </summary>
    public struct WaitComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The amount of time the task should wait.")]
        public float Duration;
        [Tooltip("Should the wait duration be randomized?")]
        public bool RandomDuration;
        [Tooltip("The wait duration range if random wait is enabled.")]
        public RangeFloat RandomDurationRange;
        [Tooltip("The amount of time the task should wait.")]
        public double WaitDuration;
        [Tooltip("The real time the task started to wait.")]
        public double StartTime;
        [Tooltip("The seed of the random number generator.")]
        public uint Seed;
        [Tooltip("The random number generator for the task.")]
        public Unity.Mathematics.Random RandomNumberGenerator;
    }

    /// <summary>
    /// A DOTS tag indicating when a Wait node is active.
    /// </summary>
    public struct WaitTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the Wait logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct WaitTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<TaskComponent>().WithAllRW<WaitComponent>().WithAll<WaitTag, EvaluationComponent>().Build();
            state.Dependency = new WaitJob() { ElapsedTime = SystemAPI.Time.ElapsedTime }.ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct WaitJob : IJobEntity
        {
            [Tooltip("The current ElapsedTime.")]
            public double ElapsedTime;

            /// <summary>
            /// Executes the wait logic.
            /// </summary>
            /// <param name="entity">The entity that is running the logic.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="waitComponents">An array of WaitComponents.</param>
            [BurstCompile]
            public void Execute(Entity entity, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<WaitComponent> waitComponents)
            {
                for (int i = 0; i < waitComponents.Length; ++i) {
                    var waitComponent = waitComponents[i];
                    var taskComponent = taskComponents[waitComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;
                        waitComponent.StartTime = ElapsedTime;

                        if (waitComponent.RandomDuration) {
                            // Generate a new random number seed for each entity.
                            if (waitComponent.RandomNumberGenerator.state == 0) {
                                waitComponent.RandomNumberGenerator = Unity.Mathematics.Random.CreateFromIndex(waitComponent.Seed != 0 ? waitComponent.Seed : (uint)entity.Index);
                            }

                            waitComponent.WaitDuration = waitComponent.RandomNumberGenerator.NextDouble(waitComponent.RandomDurationRange.Min, waitComponent.RandomDurationRange.Max);
                        } else {
                            waitComponent.WaitDuration = waitComponent.Duration;
                        }

                        waitComponents[i] = waitComponent;
                    }
                    if (taskComponent.Status == TaskStatus.Running) {
                        if (waitComponent.StartTime + waitComponent.WaitDuration <= ElapsedTime) {
                            taskComponent.Status = TaskStatus.Success;
                        }
                    }
                    taskComponents[waitComponent.Index] = taskComponent;
                }
            }
        }
    }

    [NodeIcon("b4b59e888607422409f1efa599af34ae", "e1cb9cb566a90fb4489bf31465b99747")]
    [NodeDescription("Wait a specified amount of time. The task will return running until the task is done waiting. It will return success after the wait time has elapsed. Uses the GameObject workflow.")]
    public class SharedWait : Action
    {
        [Tooltip("The amount of time to wait (in seconds).")]
        [SerializeField] SharedVariable<float> m_Duration = 1;
        [Tooltip("Should the wait duration be randomized?")]
        [SerializeField] SharedVariable<bool> m_RandomDuration;
        [Tooltip("The seed of the random number generator. Set to 0 to disable.")]
        [SerializeField] int m_Seed;
        [Tooltip("The minimum wait duration if random wait is enabled.")]
        [SerializeField] SharedVariable<RangeFloat> m_RandomDurationRange = new RangeFloat(1, 1);
        [Tooltip("The maximum wait duration if random wait is enabled.")]
        [SerializeField] SharedVariable<float> m_RandomDurationMax = 1;

        public SharedVariable<float> Duration { get => m_Duration; set => m_Duration = value; }
        public SharedVariable<bool> RandomDuration { get => m_RandomDuration; set => m_RandomDuration = value; }
        public int Seed { get => m_Seed; set => m_Seed = value; }
        public SharedVariable<RangeFloat> RandomDurationRange { get => m_RandomDurationRange; set => m_RandomDurationRange = value; }

        private float m_WaitDuration;
        private float m_StartTime;

        /// <summary>
        /// Callback when the task is initialized.
        /// </summary>
        public override void OnAwake()
        {
            if (m_Seed != 0) {
                UnityEngine.Random.InitState(m_Seed);
            }
        }

        /// <summary>
        /// Callback when the task is started.
        /// </summary>
        public override void OnStart()
        {
            if (m_RandomDuration.Value) {
                m_WaitDuration = UnityEngine.Random.Range(m_RandomDurationRange.Value.Min, m_RandomDurationRange.Value.Max);
            } else {
                m_WaitDuration = m_Duration.Value;
            }
            m_StartTime = Time.time;
        }

        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_StartTime + m_WaitDuration <= Time.time ? TaskStatus.Success : TaskStatus.Running;
        }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public override object Save(World world, Entity entity)
        {
            // Save the unique data.
            return new object[] { m_WaitDuration, Time.time - m_StartTime };
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public override void Load(object saveData, World world, Entity entity)
        {
            // saveData is the wait duration and the elapsed amount of time.
            var data = (object[])saveData;
            m_WaitDuration = (float)data[0];
            m_StartTime = Time.time - (float)data[1];
        }
    }
}
#endif
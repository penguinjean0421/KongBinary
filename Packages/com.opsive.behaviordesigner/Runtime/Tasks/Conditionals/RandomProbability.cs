#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Utility;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using Unity.Burst;
    using Unity.Entities;
    using UnityEngine;
    using System;

    /// <summary>
    /// A node representation of the random probability task.
    /// </summary>
    [NodeIcon("69bf50f8923f54c4c8bb8e258883a411", "6c5770241610a4c4aae4ac3af0ac8bf8")]
    [NodeDescription("The random probability task will return success when the random probability is below the succeed probability. It will otherwise return failure.")]
    public class RandomProbability : ILogicNode, ITaskComponentData, IConditional, IReevaluateResponder, ICloneable
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The probability of the task returning success.")]
        [SerializeField] [Range(0, 1)] float m_SuccessProbability;
        [Tooltip("The seed of the random number generator. Set to 0 to use the entity index as the seed.")]
        [SerializeField] uint m_Seed;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }
        public float SuccessProbability { get => m_SuccessProbability; set => m_SuccessProbability = value; }
        public uint Seed { get => m_Seed; set => m_Seed = value; }

        public ComponentType Tag { get => typeof(RandomProbabilityTag); }
        public System.Type SystemType { get => typeof(RandomProbabilityTaskSystem); }
        public ComponentType ReevaluateTag { get => typeof(RandomProbabilityReevaluateTag); }
        public System.Type ReevaluateSystemType { get => typeof(RandomProbabilityReevaluateTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset()
        {
            m_SuccessProbability = 1;
        }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RandomProbabilityComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomProbabilityComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomProbabilityComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<RandomProbabilityComponent>(entity);
            }

            buffer.Add(new RandomProbabilityComponent()
            {
                Index = RuntimeIndex,
                SuccessProbability = m_SuccessProbability,
                Seed = m_Seed,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RandomProbabilityComponent> buffer;
            if (world.EntityManager.HasBuffer<RandomProbabilityComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RandomProbabilityComponent>(entity);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Creates a deep clone of the component.
        /// </summary>
        /// <returns>A deep clone of the component.</returns>
        public object Clone()
        {
            var clone = GenericObjectPool.Get<RandomProbability>();
            clone.Index = Index;
            clone.ParentIndex = ParentIndex;
            clone.SiblingIndex = SiblingIndex;
            clone.SuccessProbability = SuccessProbability;
            return clone;
        }
    }

    /// <summary>
    /// The DOTS data structure for the RandomProbability class.
    /// </summary>
    public struct RandomProbabilityComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The probability of the task returning success.")]
        public float SuccessProbability;
        [Tooltip("The seed of the random number generator.")]
        public uint Seed;
        [Tooltip("The random number generator for the task.")]
        public Unity.Mathematics.Random RandomNumberGenerator;
    }

    /// <summary>
    /// A DOTS tag indicating when a RandomProbability node is active.
    /// </summary>
    public struct RandomProbabilityTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the RandomProbability logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RandomProbabilityTaskSystem : ISystem
    {
        /// <summary>
        /// Creates the job.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAllRW<TaskComponent>().WithAllRW<RandomProbabilityComponent>().WithAll<RandomProbabilityTag, EvaluationComponent>().Build();
            state.Dependency = new RandomProbabilityJob().ScheduleParallel(query, state.Dependency);
        }

        /// <summary>
        /// Job which executes the task logic.
        /// </summary>
        [BurstCompile]
        private partial struct RandomProbabilityJob : IJobEntity
        {
            /// <summary>
            /// Executes the random probability logic.
            /// </summary>
            /// <param name="entity">The entity that is running the logic.</param>
            /// <param name="taskComponents">An array of TaskComponents.</param>
            /// <param name="randomProbabilityComponents">An array of RandomProbabilityComponents.</param>
            [BurstCompile]
            public void Execute(Entity entity, ref DynamicBuffer<TaskComponent> taskComponents, ref DynamicBuffer<RandomProbabilityComponent> randomProbabilityComponents)
            {
                for (int i = 0; i < randomProbabilityComponents.Length; ++i) {
                    var randomProbabilityComponent = randomProbabilityComponents[i];
                    var taskComponent = taskComponents[randomProbabilityComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        // Generate a new random number seed for each entity.
                        if (randomProbabilityComponent.RandomNumberGenerator.state == 0) {
                            randomProbabilityComponent.RandomNumberGenerator = Unity.Mathematics.Random.CreateFromIndex(randomProbabilityComponent.Seed != 0 ? randomProbabilityComponent.Seed : (uint)entity.Index);
                        }
                        // NextFloat updates the RandomNumberGenerator so the component must be replaced.
                        var probability = randomProbabilityComponent.RandomNumberGenerator.NextFloat();
                        var randomProbabilityBuffer = randomProbabilityComponents;
                        randomProbabilityBuffer[i] = randomProbabilityComponent;

                        // The task will always change status.
                        taskComponent.Status = probability < randomProbabilityComponent.SuccessProbability ? TaskStatus.Success : TaskStatus.Failure;
                        taskComponents[randomProbabilityComponent.Index] = taskComponent;
                    } else if (taskComponent.Status == TaskStatus.Running) {
                        // A status of running means the task is being resumed from a conditional abort. Return success. 
                        taskComponent.Status = TaskStatus.Success;
                        taskComponents[randomProbabilityComponent.Index] = taskComponent;
                    }
                } 
            }
        }
    }


    /// <summary>
    /// A DOTS tag indicating when an RandomProbability node needs to be reevaluated.
    /// </summary>
    public struct RandomProbabilityReevaluateTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Runs the RandomProbability reevaluation logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RandomProbabilityReevaluateTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the reevaluation logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (taskComponents, randomProbabilityComponents, entity) in
                SystemAPI.Query<DynamicBuffer<TaskComponent>, DynamicBuffer<RandomProbabilityComponent>>().WithAll<RandomProbabilityReevaluateTag, EvaluationComponent>().WithEntityAccess()) {
                for (int i = 0; i < randomProbabilityComponents.Length; ++i) {
                    var randomProbabilityComponent = randomProbabilityComponents[i];
                    var taskComponent = taskComponents[randomProbabilityComponent.Index];
                    if (!taskComponent.Reevaluate) {
                        continue;
                    }

                    // NextFloat updates the RandomNumberGenerator so the component must be replaced.
                    var probability = randomProbabilityComponent.RandomNumberGenerator.NextFloat();
                    var randomProbabilityBuffer = randomProbabilityComponents;
                    randomProbabilityBuffer[i] = randomProbabilityComponent;

                    var status = probability < randomProbabilityComponent.SuccessProbability ? TaskStatus.Success : TaskStatus.Failure;
                    if (status != taskComponent.Status) {
                        taskComponent.Status = status;
                        var buffer = taskComponents;
                        buffer[taskComponent.Index] = taskComponent;
                    }
                }
            }
        }
    }

    [NodeIcon("69bf50f8923f54c4c8bb8e258883a411", "6c5770241610a4c4aae4ac3af0ac8bf8")]
    [NodeDescription("The random probability task will return success when the random probability is below the succeed probability. It will otherwise return failure.")]
    public class SharedRandomProbability : Conditional
    {
        [Tooltip("The probability of the task returning success.")]
        [SerializeField] SharedVariable<float> m_SuccessProbability;
        [Tooltip("The seed of the random number generator. Set to 0 to disable.")]
        [SerializeField] int m_Seed;

        public SharedVariable<float> SuccessProbability { get => m_SuccessProbability; set => m_SuccessProbability = value; }
        public int Seed { get => m_Seed; set => m_Seed = value; }

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
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            return UnityEngine.Random.value < m_SuccessProbability.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
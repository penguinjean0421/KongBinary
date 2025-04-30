#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Systems
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// Resets the evaluation status.
    /// </summary>
    [UpdateInGroup(typeof(BehaviorTreeSystemGroup), OrderLast = true)]
    public partial struct EvaluationCleanupSystem : ISystem
    {
        private EntityQuery m_EvaluateCleanupQuery;
        private ComponentTypeHandle<EnabledTag> m_EnabledComponentHandle;
        private ComponentTypeHandle<EvaluationComponent> m_EvaluationComponentHandle;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_EvaluateCleanupQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<EvaluationComponent>()
                .WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)
                .Build(ref state);
            m_EnabledComponentHandle = state.GetComponentTypeHandle<EnabledTag>();
            m_EvaluationComponentHandle = state.GetComponentTypeHandle<EvaluationComponent>();
        }

        /// <summary>
        /// Updates the data object values for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            // Reset the evaluation status.
            m_EnabledComponentHandle.Update(ref state);
            m_EvaluationComponentHandle.Update(ref state);
            var evaluationCleanupJob = new EvaluationCleanupJob()
            {
                EnabledComponentHandle = m_EnabledComponentHandle,
                EvaluationComponentHandle = m_EvaluationComponentHandle,
            };
            state.Dependency = evaluationCleanupJob.ScheduleParallel(m_EvaluateCleanupQuery, state.Dependency);
        }

        /// <summary>
        /// Job that resets the EvaluationComponent component value.
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public struct EvaluationCleanupJob : IJobChunk
        {
            [UnityEngine.Tooltip("A reference to the Enabled Component Handle.")]
            public ComponentTypeHandle<EnabledTag> EnabledComponentHandle;
            [UnityEngine.Tooltip("A reference to the Evaluate Component Handle.")]
            public ComponentTypeHandle<EvaluationComponent> EvaluationComponentHandle;

            /// <summary>
            /// Resets the EvaluationComponent component value.
            /// </summary>
            /// <param name="chunk">Block of memory that contains the entity and components.</param>
            /// <param name="unfilteredChunkIndex">The index of the chunk.</param>
            /// <param name="useEnabledMask">Should the enabled mask be used?</param>
            /// <param name="chunkEnabledMask">The bitwise enabled mask.</param>
            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                for (int i = 0; i < chunk.Count; i++) {
                    // If the chunk is enabled then it should be evaluated.
                    if (chunk.IsComponentEnabled<EnabledTag>(ref EnabledComponentHandle, i)) {
                        chunk.SetComponentEnabled<EvaluationComponent>(ref EvaluationComponentHandle, i, true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Resets the InterruptedTag enabled value.
    /// </summary>
    [UpdateInGroup(typeof(BehaviorTreeSystemGroup), OrderLast = true)]
    public partial struct InterruptedCleanupSystem : ISystem
    {
        private EntityQuery m_InterruptedCleanupQuery;
        private ComponentTypeHandle<InterruptedTag> m_InterruptedComponentHandle;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_InterruptedCleanupQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<InterruptedTag>()
                .Build(ref state);
            m_InterruptedComponentHandle = state.GetComponentTypeHandle<InterruptedTag>();
        }

        /// <summary>
        /// Updates the data object values for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            // Clean up the interrupted tag.
            m_InterruptedComponentHandle.Update(ref state);
            var interruptedJob = new InterruptedCleanupJob()
            {
                InterruptedComponentHandle = m_InterruptedComponentHandle,
            };
            state.Dependency = interruptedJob.ScheduleParallel(m_InterruptedCleanupQuery, state.Dependency);
        }

        /// <summary>
        /// Job that resets the InterruptedTag value.
        /// </summary>
        [BurstCompile(CompileSynchronously = true)]
        public partial struct InterruptedCleanupJob : IJobChunk
        {
            [UnityEngine.Tooltip("A reference to the Interrupted Component Handle.")]
            public ComponentTypeHandle<InterruptedTag> InterruptedComponentHandle;

            /// <summary>
            /// Resets the InterruptedTag value.
            /// </summary>
            /// <param name="entity">The entity that is being acted upon.</param>
            /// <param name="entityIndex">The index of the entity.</param>
            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                for (int i = 0; i < chunk.Count; i++) {
                    // Only chunks with the tag enabled will be returned so there's no need to check if the tag is enabled.
                    chunk.SetComponentEnabled<InterruptedTag>(ref InterruptedComponentHandle, i, false);
                }
            }
        }
    }
}
#endif
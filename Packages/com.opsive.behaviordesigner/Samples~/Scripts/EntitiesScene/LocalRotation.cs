/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    /// <summary>
    /// The amount to rotate the entity by.
    /// </summary>
    public struct LocalRotation : IComponentData
    {
        [Tooltip("The rotation speed.")]
        public float3 RotationSpeed;
    }

    /// <summary>
    /// Applies the rotation specified by the LocalRotation component.
    /// </summary>
    [DisableAutoCreation]
    public partial struct LocalRotationSystem : ISystem
    {
        private EntityQuery m_Query;

        /// <summary>
        /// Creates the required objects for use within the job system.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        [BurstCompile]
        private void OnCreate(ref SystemState state)
        {
            m_Query = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<LocalTransform>().WithAll<LocalRotation>()
                .Build(ref state);
        }

        /// <summary>
        /// Starts the RotateJob.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            state.Dependency = new RotateJob()
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(m_Query, state.Dependency);
        }

        /// <summary>
        /// Sets the rotation to the transform
        /// </summary>
        [BurstCompile]
        private partial struct RotateJob : IJobEntity
        {
            [Tooltip("The current frame's DeltaTime.")]
            public float DeltaTime;

            /// <summary>
            /// Updates the logic.
            /// </summary>
            /// <param name="transform">The entity's transform.</param>
            /// <param name="localRotation">The entity's local rotation component.</param>
            [BurstCompile]
            public void Execute(ref LocalTransform transform, LocalRotation localRotation)
            {
                transform.Rotation = math.mul(transform.Rotation, quaternion.EulerZYX(localRotation.RotationSpeed * DeltaTime));
            }
        }
    }
}
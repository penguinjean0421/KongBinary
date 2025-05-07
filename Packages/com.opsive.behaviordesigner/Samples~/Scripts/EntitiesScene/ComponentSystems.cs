/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using System;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;

    /// <summary>
    /// Specifies the entity agents.
    /// </summary>
    public struct AgentTag : IComponentData { }

    /// <summary>
    /// Specifies the entity that should be targeted.
    /// </summary>
    public struct TargetEntityTag : IComponentData { }

    /// <summary>
    /// Marks the object for destruction.
    /// </summary>
    public struct DestroyEntityTag : IComponentData { }

    /// <summary>
    /// Destroys the tagged entities after the behavior tree simulation has run.
    /// </summary>
    [DisableAutoCreation]
    public partial class DestroyEntitySystem : SystemBase
    {
        public Action<float3> OnDestroyEntity; 

        /// <summary>
        /// Destroys the entities tagged with the DestroyTag.
        /// </summary>
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (destroyTag, localTransform, entity) in SystemAPI.Query<DestroyEntityTag, LocalTransform>().WithEntityAccess()) {
                ecb.DestroyEntity(entity);

                OnDestroyEntity?.Invoke(localTransform.Position);
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Specifies the entity that is the rotating part of the turret.
    /// </summary>
    public struct TurretTag : IComponentData { }

    /// <summary>
    /// Specifies the entity that is the base of the turret. This entity should be destroyed at the same time as the turret's gun.
    /// </summary>
    public struct TurretBaseTag : IComponentData { }

    /// <summary>
    /// Component for pulling back the turret when it fires a bullet.
    /// </summary>
    public struct TurretRecoil : IComponentData, IEnableableComponent
    {
        [UnityEngine.Tooltip("Is the recoil pulling back the turret?")]
        public bool Pullback;
        [UnityEngine.Tooltip("The speed of the recoil pullback.")]
        public float PullbackPosition;
        [UnityEngine.Tooltip("The target z position of the recoil pullback.")]
        public float PullbackSpeed;
    }

    /// <summary>
    /// Destroys the tagged entities after the behavior tree simulation has run.
    /// </summary>
    [DisableAutoCreation]
    public partial struct TurretRecoilSystem : ISystem
    {
        /// <summary>
        /// Destroys the entities tagged with the DestroyTag.
        /// </summary>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (turretRecoil, localTransform, entity) in SystemAPI.Query<RefRW<TurretRecoil>, RefRW<LocalTransform>>().WithEntityAccess()) {
                var position = localTransform.ValueRO.Position;
                position.z = MoveTowards(localTransform.ValueRO.Position.z, turretRecoil.ValueRO.Pullback ? turretRecoil.ValueRO.PullbackPosition : 0, turretRecoil.ValueRO.PullbackSpeed * deltaTime);
                localTransform.ValueRW.Position = position;
                if (turretRecoil.ValueRO.Pullback && math.abs(position.z - turretRecoil.ValueRO.PullbackPosition) <= 0.01f) {
                    turretRecoil.ValueRW.Pullback = false;
                    ecb.SetComponentEnabled<TurretRecoil>(entity, false);
                } else if (!turretRecoil.ValueRO.Pullback && math.abs(position.z) <= 0.01f) {
                    turretRecoil.ValueRW.Pullback = true;
                    ecb.SetComponentEnabled<TurretRecoil>(entity, false);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Moves towards the target limited by the maximum distance.
        /// </summary>
        /// <param name="current">The current value.</param>
        /// <param name="target">The target value.</param>
        /// <param name="maxDistanceDelta">The maximum distance that the delta value can move.</param>
        /// <returns>The move towards value.</returns>
        [BurstCompile]
        private static float MoveTowards(float current, float target, float maxDistanceDelta)
        {
            var delta = target - current;
            if (math.abs(delta) <= maxDistanceDelta) {
                return target;
            }
            return current + maxDistanceDelta * math.sign(delta);
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime;
    using System;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    /// <summary>
    /// Spawns the entities.
    /// </summary>
    public class EntitySpawner : MonoBehaviour
    {
        [Tooltip("A reference to the spawn data.")]
        [SerializeField] protected EntitySpawnData m_SpawnData;

        /// <summary>
        /// Bakes the spawn data.
        /// </summary>
        private class Baker : Baker<EntitySpawner>
        {
            /// <summary>
            /// Bakes the data.
            /// </summary>
            /// <param name="authoring">The parent authoring component.</param>
            public override void Bake(EntitySpawner authoring)
            {
                if (authoring.m_SpawnData == null || authoring.m_SpawnData.Prefab == null) {
                    return;
                }
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var entityPrefab = GetEntity(authoring.m_SpawnData.Prefab, TransformUsageFlags.Dynamic);
                AddComponent<EntitySpawnerPrefab>(entity, new EntitySpawnerPrefab()
                {
                    Prefab = entityPrefab,
                });
                AddComponentObject(entity, new SpawnData
                {
                    Prefab = entityPrefab,
                    SpawnCount = authoring.m_SpawnData.InitialSpawnCount,
                    MinRadius = authoring.m_SpawnData.MinRadius,
                    MaxRadius = authoring.m_SpawnData.MaxRadius,
                    MinRotationSpeed = authoring.m_SpawnData.MinRotationSpeed,
                    MaxRotationSpeed = authoring.m_SpawnData.MaxRotationSpeed,
                });
            }
        }
    }

    /// <summary>
    /// Stores a reference to the spawn prefab.
    /// </summary>
    public struct EntitySpawnerPrefab : IComponentData
    {
        public Entity Prefab;
    }

    /// <summary>
    /// Contains the information needed to spawn the entities.
    /// </summary>
    public class SpawnData : IComponentData
    {
        [Tooltip("The entity prefab that should be spawned.")]
        public Entity Prefab;
        [Tooltip("The number of entities that should be initially spawned.")]
        public int SpawnCount;
        [Tooltip("The minimum radius that the entities should be spawned.")]
        public int MinRadius;
        [Tooltip("The maximum radius that the entities should be spawned.")]
        public int MaxRadius;
        [Tooltip("The minimum speed that the entities should rotate.")]
        public float3 MinRotationSpeed;
        [Tooltip("The maximum speed that the entities should rotate.")]
        public float3 MaxRotationSpeed;
    }

    /// <summary>
    /// System that does the spawning.
    /// </summary>
    public partial struct EntitySpawnerSystem : ISystem
    {
        /// <summary>
        /// Sets the system requirements.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpawnData>();
        }

        /// <summary>
        /// Spawns the entities.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            var spawner = SystemAPI.ManagedAPI.GetSingleton<SpawnData>();
            var entities = state.EntityManager.Instantiate(spawner.Prefab, spawner.SpawnCount, Allocator.Temp);
            var random = Unity.Mathematics.Random.CreateFromIndex((uint)DateTime.Now.Ticks);

            // Setup each entity.
            for (int i = 0; i < entities.Length; i += 1) {
                var position = new float3();
                var radians = random.NextFloat(math.PI2);
                var dist = random.NextFloat(spawner.MinRadius, spawner.MaxRadius);
                position.x = math.cos(radians) * dist;
                position.y = 1;
                position.z = math.sin(radians) * dist;

                state.EntityManager.SetComponentData(entities[i], new LocalTransform { Position = position, Scale = 1, Rotation = quaternion.identity });
                state.EntityManager.AddComponentData(entities[i], new LocalRotation { RotationSpeed = 
                                                                    new float3(
                                                                        random.NextFloat(spawner.MinRotationSpeed.x, spawner.MaxRotationSpeed.x),
                                                                        random.NextFloat(spawner.MinRotationSpeed.y, spawner.MaxRotationSpeed.y),
                                                                        random.NextFloat(spawner.MinRotationSpeed.z, spawner.MaxRotationSpeed.z)) });
                state.EntityManager.AddComponent(entities[i], ComponentType.ReadOnly<AgentTag>());
            }

            // LocalRotationSystem needs to be added manually.
            var systemGroup = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SimulationSystemGroup>();
            systemGroup.AddSystemToUpdateList(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<LocalRotationSystem>());

            // The behavior trees have been baked. Enable the system which starts the behavior trees.
            BehaviorTree.EnableBakedBehaviorTreeSystem(World.DefaultGameObjectInjectionWorld);
        }
    }
}
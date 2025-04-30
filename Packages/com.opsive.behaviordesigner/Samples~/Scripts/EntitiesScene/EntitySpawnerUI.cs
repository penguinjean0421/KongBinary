/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using System;
    using Unity.Collections;
    using Unity.Entities;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Displays entity spawn button and information.
    /// </summary>
    public class EntitySpawnerUI : MonoBehaviour
    {
        [Tooltip("A reference to the spawn data.")]
        [SerializeField] protected EntitySpawnData m_SpawnData;
        [Tooltip("A reference to the entity count text.")]
        [SerializeField] protected Text m_Text;

        /// <summary>
        /// Creates the EntityCountSystem.
        /// </summary>
        private void Start()
        {
            var entityCountSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCountSystem>();
            entityCountSystem.OnUpdateEntityCount += (int count) => { m_Text.text = $"Entity Count: {count}"; };

            var systemGroup = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SimulationSystemGroup>();
            systemGroup.AddSystemToUpdateList(entityCountSystem);
        }

        /// <summary>
        /// Spawns a new set of entity agents.
        /// </summary>
        public void Spawn()
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = new EntityQueryBuilder(Allocator.Temp).WithAll<EntitySpawnerPrefab>().Build(entityManager);
            var spawnerEntity = query.ToEntityArray(AllocatorManager.Temp); // Only one entity will be returned. This entity was created within EntitySpawner.Baker.
            var entitySpawnerPrefab = entityManager.GetComponentData<EntitySpawnerPrefab>(spawnerEntity[0]);
            entityManager.SetComponentData(spawnerEntity[0], new SpawnData()
            {
                Prefab = entitySpawnerPrefab.Prefab,
                SpawnCount = m_SpawnData.AdditiveSpawnCount,
                MinRadius = m_SpawnData.MinRadius,
                MaxRadius = m_SpawnData.MaxRadius,
                MinRotationSpeed = m_SpawnData.MinRotationSpeed,
                MaxRotationSpeed = m_SpawnData.MaxRotationSpeed,
            });
            spawnerEntity.Dispose();
            query.Dispose();

            // EntitySpawnerSystem disables itself. Enable it to allow for the spawn.
            World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<EntitySpawnerSystem>().Enabled = true;
        }

        /// <summary>
        /// Shows the agent entity count.
        /// </summary>
        [DisableAutoCreation]
        private partial class EntityCountSystem : SystemBase
        {
            [Tooltip("Event sent when the entity count is updated.")]
            public Action<int> OnUpdateEntityCount;

            private EntityQuery m_EntityCountQuery;

            /// <summary>
            /// Caches the entity count query.
            /// </summary>
            protected override void OnCreate()
            {
                m_EntityCountQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<AgentTag>().Build(EntityManager);
            }

            /// <summary>
            /// Updates the entity count.
            /// </summary>
            protected override void OnUpdate()
            {
                var agentEntities = m_EntityCountQuery.ToEntityArray(WorldUpdateAllocator);
                OnUpdateEntityCount?.Invoke(agentEntities.Length);
                agentEntities.Dispose();
            }

            /// <summary>
            /// Destroys the cached the entity count query.
            /// </summary>
            protected override void OnDestroy()
            {
                m_EntityCountQuery.Dispose();
            }
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using System.Collections;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.Pool;
    using UnityEngine.UI;

    /// <summary>
    /// Adds effects to the entities sample scene.
    /// </summary>
    public class EffectsManager : MonoBehaviour
    {
        [Tooltip("The explosion prefab.")]
        [SerializeField] protected GameObject m_Explosion;
        [Tooltip("A reference to the turret's healthbar.")]
        [SerializeField] protected Slider m_TurretHealthbar;

        private ObjectPool<GameObject> m_ExplosionPool;
        private EntityCommandBuffer m_EntityCommandBuffer;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_ExplosionPool = new ObjectPool<GameObject>(() => // Create. 
            {
                var explosion = GameObject.Instantiate(m_Explosion);
                explosion.GetComponent<ReturnToParticlePool>().Pool = m_ExplosionPool;
                return explosion;
            }, (GameObject explosion) => // Take.
            {
                explosion.SetActive(true);
            }, (GameObject explosion) => // Return.
            {
                explosion.SetActive(false);
            }, (GameObject explosion) => // Destroy.
            {
                Destroy(explosion);
            });
        }

        /// <summary>
        /// The entity subscene has loaded.
        /// </summary>
        private void Start()
        {
            StartCoroutine(RegisterEvents());
        }

        /// <summary>
        /// Subscribes to the interested events.
        /// </summary>
        private IEnumerator RegisterEvents()
        {
            DamageTaskSystem damageTaskSystem;
            do {
                damageTaskSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DamageTaskSystem>();
                yield return new WaitForEndOfFrame(); // The systems aren't available immediately.
            } while (damageTaskSystem == null);
            damageTaskSystem.OnDamage += OnDamageTurret;

            var destroyTaskSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<DestroyEntitySystem>();
            destroyTaskSystem.OnDestroyEntity += OnDestroyEntity;

            // Add the necessary systems that are not created by Behavior Designer.
            var systemGroup = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SimulationSystemGroup>();
            systemGroup.AddSystemToUpdateList(destroyTaskSystem);
            systemGroup.AddSystemToUpdateList(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<TurretRecoilSystem>());
        }

        /// <summary>
        /// The entity has been destroyed.
        /// </summary>
        /// <param name="position">The position of the entity.</param>
        private void OnDestroyEntity(float3 position)
        {
            var explosion = m_ExplosionPool.Get();
            explosion.transform.position = position;
        }

        /// <summary>
        /// The turret has been damaged.
        /// </summary>
        /// <param name="healthAmount">The remaining health amount.</param>
        private void OnDamageTurret(float healthAmount)
        {
            healthAmount = Mathf.Clamp(healthAmount, 0, Mathf.Abs(healthAmount));
            m_TurretHealthbar.value = healthAmount;

            // Destroy the turret when it doesn't have any more health.
            if (healthAmount == 0) {
                var explosion = m_ExplosionPool.Get();
                explosion.transform.position = Vector3.zero;
                explosion.transform.localScale = new Vector3(4, 4, 4);

                // The turret base should also be destroyed.
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var query = new EntityQueryBuilder(Allocator.Temp).WithAll<TurretBaseTag>().Build(entityManager);
                var entities = query.ToEntityArray(AllocatorManager.Temp); // Only one entity will be returned.
                m_EntityCommandBuffer = new EntityCommandBuffer(Allocator.Persistent);
                if (entities.Length > 0) {
                    m_EntityCommandBuffer.DestroyEntity(entities[0]);
                }
                query.Dispose();
                entities.Dispose();

                // .. As well as the turret weapon.
                query = new EntityQueryBuilder(Allocator.Temp).WithAll<TurretRecoil>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).Build(entityManager);
                entities = query.ToEntityArray(AllocatorManager.Temp); // Only one entity will be returned.
                if (entities.Length > 0) {
                    m_EntityCommandBuffer.DestroyEntity(entities[0]);
                }
                query.Dispose();
                entities.Dispose();

                // All of the currently charging agents should be destroyed.
                query = new EntityQueryBuilder(Allocator.Temp).WithAll<ChargeTag>().Build(entityManager);
                entities = query.ToEntityArray(AllocatorManager.Temp);
                for (int i = 0; i < entities.Length; ++i) {
                    m_EntityCommandBuffer.DestroyEntity(entities[i]);
                }
                query.Dispose();
                entities.Dispose();

                enabled = true;
            }
        }

        /// <summary>
        /// Runs the EntityCommandBuffer if it exists.
        /// </summary>
        private void Update()
        {
            if (m_EntityCommandBuffer.IsEmpty) {
                enabled = false;
                return;
            }

            m_EntityCommandBuffer.Playback(World.DefaultGameObjectInjectionWorld.EntityManager);
            m_EntityCommandBuffer.Dispose();
        }

        /// <summary>
        /// The component has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (World.DefaultGameObjectInjectionWorld == null) {
                return;
            }

            var destroySystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DestroyEntitySystem>();
            if (destroySystem != null) {
                destroySystem.OnDestroyEntity -= OnDestroyEntity;
            }

            var damageSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<DamageTaskSystem>();
            if (damageSystem != null) {
                damageSystem.OnDamage -= OnDamageTurret;
            }
        }
    }
}
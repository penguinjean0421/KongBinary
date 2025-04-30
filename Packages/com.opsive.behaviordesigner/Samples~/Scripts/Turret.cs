/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;
    using System.Collections.Generic;

    /// <summary>
    /// Fires a projectile at the specified target.
    /// </summary>
    public class Turret : MonoBehaviour
    {
        [Tooltip("The target that is being shot.")]
        [SerializeField] protected List<Health> m_Targets;
        [Tooltip("Can the turret hit target the same object multiple times?")]
        [SerializeField] protected bool m_AllowMultipleHits = true;
        [Tooltip("The maximum distance that the turret can fire.")]
        [SerializeField] protected float m_MaxDistance = float.MaxValue;
        [Tooltip("The GameObject that should rotate to look at the target.")]
        [SerializeField] protected GameObject m_Rotator;
        [Tooltip("The speed that the rotator should look at the target.")]
        [SerializeField] protected float m_RotationSpeed;
        [Tooltip("The interval at which to fire the projectile.")]
        [SerializeField] protected int m_FireInterval = 2;
        [Tooltip("The projectile that should be fired.")]
        [SerializeField] protected GameObject m_Projectile;
        [Tooltip("The location that the projectile should be spawned.")]
        [SerializeField] protected Transform m_ProjectileSpawnLocation;
        [Tooltip("The transform that should pullback when the turret is fired.")]
        [SerializeField] protected Transform m_PullbackTransform;
        [Tooltip("The amount that the transform should pullback.")]
        [SerializeField] protected float m_PullbackPosition = -2;
        [Tooltip("The speed at which the transform should pullback.")]
        [SerializeField] protected float m_PullbackSpeed = 30;
        [Tooltip("A reference to the muzzle flash.")]
        [SerializeField] protected GameObject m_MuzzleFlashPrefab;
        [Tooltip("The offset to apply to the muzzle flash spawn.")]
        [SerializeField] protected Vector3 m_MuzzleFlashSpawnRotationOffset;

        public List<Health> Targets { get => m_Targets; set => m_Targets = value; }

        private Transform m_Transform;
        private Health m_Target;
        private float m_FireTime;
        private bool m_Pullback;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            m_Transform = transform;
            m_FireTime = -m_FireInterval;
        }

        /// <summary>
        /// Determines the target to fire upon.
        /// </summary>
        private void DetermineTarget()
        {
            if (m_Target == null || m_Target.Value <= 0 || (m_Target.transform.position - m_Transform.position).magnitude > m_MaxDistance) {
                var closestIndex = -1;
                var closestDistance = float.MaxValue;
                for (int i = 0; i < m_Targets.Count; ++i) {
                    if (m_Targets[i].Value <= 0) {
                        continue;
                    }
                    var distance = (m_Targets[i].transform.position - m_Transform.position).magnitude;
                    if (distance < closestDistance && distance <= m_MaxDistance) {
                        closestDistance = distance;
                        closestIndex = i;
                    }
                }

                m_Target = closestIndex == -1 ? null : m_Targets[closestIndex];

                if (closestIndex != -1 && !m_AllowMultipleHits) {
                    m_Targets.RemoveAt(closestIndex);
                }
            }
        }

        /// <summary>
        /// Rotates and fires upon the target.
        /// </summary>
        private void Update()
        {
            DetermineTarget();

            // The turret can pullback the gun indicating recoil.
            if (m_PullbackTransform != null) {
                var targetPosition = m_Pullback ? m_PullbackPosition : 0;
                var localPosition = m_PullbackTransform.localPosition;
                if (targetPosition != localPosition.z) {
                    localPosition.z = Mathf.MoveTowards(localPosition.z, targetPosition, m_PullbackSpeed * Time.deltaTime);
                    if (m_Pullback && Mathf.Abs(localPosition.z - targetPosition) < 0.01f) {
                        m_Pullback = false;
                    }
                    m_PullbackTransform.localPosition = localPosition;
                }
            }

            if (m_Target == null) {
                return;
            }

            var direction = m_Target.transform.position - m_Rotator.transform.position;
            direction.y = 0;
            var rotation = Quaternion.LookRotation(direction.normalized);
            m_Rotator.transform.rotation = Quaternion.RotateTowards(m_Rotator.transform.rotation, rotation, m_RotationSpeed * Time.deltaTime);

            if (m_FireTime + m_FireInterval < Time.time) {
                FireProjectile();
            }
        }

        /// <summary>
        /// Fires the projectile.
        /// </summary>
        private void FireProjectile()
        {
            var projectile = GameObject.Instantiate(m_Projectile, m_ProjectileSpawnLocation.position, m_ProjectileSpawnLocation.rotation);
            projectile.GetComponent<Projectile>().Initialize(m_Target.gameObject);
            m_FireTime = Time.time;
            m_Pullback = true;

            if (m_MuzzleFlashPrefab != null) {
                GameObject.Instantiate(m_MuzzleFlashPrefab, m_ProjectileSpawnLocation.position, m_ProjectileSpawnLocation.rotation * Quaternion.Euler(m_MuzzleFlashSpawnRotationOffset));
            }
        }
    }
}
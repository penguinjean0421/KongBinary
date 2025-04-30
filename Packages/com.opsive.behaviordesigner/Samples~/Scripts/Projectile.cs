/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Moves a projectile towards the specified target.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("The speed at which the projectile should move.")]
        [SerializeField] protected float m_Speed = 5;
        [Tooltip("The amount of damage that should be applied.")]
        [SerializeField] protected float m_DamageAmount = 20;
        [Tooltip("The +/- range of damage that should be applied.")]
        [SerializeField] protected float m_DamageVariance;
        [Tooltip("Should the y-axis direction be ignored?")]
        [SerializeField] protected bool m_IgnoreY = true;
        [Tooltip("Should the projectile rotate towards the target?")]
        [SerializeField] protected bool m_RotateTowardsTarget;

        private Transform m_Transform;
        private Transform m_Target;

        /// <summary>
        /// Initializes the projectile to the specified target.
        /// </summary>
        /// <param name="target">The target the projectile should move towards.</param>
        public void Initialize(GameObject target)
        {
            m_Transform = transform;
            m_Target = target.transform;
            if (m_RotateTowardsTarget) {
                var direction = GetDirection();
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                m_Transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        /// <summary>
        /// Returns the direction that the projectile should move.
        /// </summary>
        /// <returns>The direction that the projectile should move.</returns>
        private Vector3 GetDirection()
        {
            var direction = m_Target.position - m_Transform.position;
            if (m_IgnoreY) {
                direction.y = 0;
            }
            return direction.normalized;
        }

        /// <summary>
        /// Move towards the target.
        /// </summary>
        private void Update()
        {
            var direction = GetDirection();
            m_Transform.position += direction * m_Speed * Time.deltaTime;
            if (m_RotateTowardsTarget) {
                var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                m_Transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        /// <summary>
        /// The projectile collided with another object.
        /// </summary>
        /// <param name="collision">The other object.</param>
        private void OnCollisionEnter(Collision collision)
        {
            Health health;
            if (collision.transform == m_Target && (health = collision.collider.gameObject.GetComponent<Health>()) != null) {
                health.Value -= (m_DamageAmount + Random.Range(-m_DamageVariance, m_DamageVariance));
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// The projectile collided with another object.
        /// </summary>
        /// <param name="collision">The other object.</param>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            Health health;
            if (collision.transform == m_Target && (health = collision.collider.gameObject.GetComponent<Health>()) != null) {
                health.Value -= (m_DamageAmount + Random.Range(-m_DamageVariance, m_DamageVariance));
                Destroy(gameObject);
            }
        }
    }
}
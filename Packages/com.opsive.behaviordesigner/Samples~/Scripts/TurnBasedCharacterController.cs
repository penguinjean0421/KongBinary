/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Samples.SceneManagers;
    using UnityEngine;

    /// <summary>
    /// Character controller for the turn based scene.
    /// </summary>
    public class TurnBaseCharacterController : MonoBehaviour
    {
        [Tooltip("The amount of time it takes for the projectile to spawn after attacking")]
        [SerializeField] protected float m_ProjectileSpawnDelay = 0.1f;
        [Tooltip("A prefab of the projectile that should be spawned when the character attacks.")]
        [SerializeField] protected GameObject m_Projectile;
        [Tooltip("The location that the projectile should spawn.")]
        [SerializeField] protected GameObject m_ProjectileSpawnLocation;
        [Tooltip("The amount of time it takes before the attack state should be reset.")]
        [SerializeField] protected float m_AttackResetDelay = 0.5f;

        [Tooltip("The color to flash the material. Set to clear to disable.")]
        [SerializeField] protected Color m_MaterialFlashColor = Color.clear;
        [Tooltip("The amount of time the material color should stay the flash color.")]
        [SerializeField] protected float m_MaterialFlashCooldownDelay = 0.01f;
        [Tooltip("The amount of time that the material color should be interpolated back to the original value.")]
        [SerializeField] protected float m_MaterialFlashDuration = 0.25f;

        private Animator m_Animator;
        private Health m_Health;

        private TurnBasedSceneManager m_TurnBasedSceneManager;
        private GameObject m_Target;

        private Material m_FlashMaterial;
        private Color m_OriginalColor;
        private Coroutine m_MaterialFlashCoroutine;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_Health = GetComponentInChildren<Health>();
            m_Health.OnUpdateValue += OnUpdateHealthValue;

#if UNITY_2022
            m_TurnBasedSceneManager = Object.FindObjectOfType<TurnBasedSceneManager>();
#else
            m_TurnBasedSceneManager = Object.FindFirstObjectByType<TurnBasedSceneManager>();
#endif

            if (m_MaterialFlashColor != Color.clear) {
                m_FlashMaterial = GetComponent<Renderer>().material;
            }
        }

        /// <summary>
        /// The health value has been updated.
        /// </summary>
        /// <param name="value">The health value.</param>
        private void OnUpdateHealthValue(float value)
        {
            if (value == 0) { // Death state.
                m_Animator.SetInteger("State", 2);
                // Game over.
                if (gameObject == m_TurnBasedSceneManager.Player) {
                    Time.timeScale = 0;
                }
            }

            // Play a quick damage flash.
            if (m_FlashMaterial != null) {
                if (m_MaterialFlashCoroutine != null) {
                    m_FlashMaterial.color = m_OriginalColor;
                    StopCoroutine(FlashMaterial());
                    m_MaterialFlashCoroutine = null;
                }
                m_MaterialFlashCoroutine = StartCoroutine(FlashMaterial());
            }
        }

        /// <summary>
        /// Flashes the material.
        /// </summary>
        private System.Collections.IEnumerator FlashMaterial()
        {
            m_OriginalColor = m_FlashMaterial.color;
            m_FlashMaterial.color = m_MaterialFlashColor;

            yield return new WaitForSeconds(m_MaterialFlashCooldownDelay);

            // Cooldown.
            var endTime = Time.time + m_MaterialFlashDuration;
            while (Time.time < endTime) {
                var color = Color.Lerp(m_OriginalColor, m_MaterialFlashColor, (endTime - Time.time) / m_MaterialFlashDuration);
                m_FlashMaterial.color = color;
                yield return new WaitForEndOfFrame();
            }

            m_FlashMaterial.color = m_OriginalColor;
            m_MaterialFlashCoroutine = null;
        }

        /// <summary>
        /// Attacks the specified target.
        /// </summary>
        /// <param name="target">The target that should be attacked.</param>
        public void Attack(GameObject target)
        {
            m_Target = target;
            m_Animator.SetInteger("State", 1);

            Invoke("SpawnProjectile", m_ProjectileSpawnDelay);
            Invoke("ResetState", m_AttackResetDelay);
        }

        /// <summary>
        /// Spawns the projectile.
        /// </summary>
        private void SpawnProjectile()
        {
            var projectileObj = Object.Instantiate(m_Projectile);
            projectileObj.transform.SetPositionAndRotation(m_ProjectileSpawnLocation.transform.position, m_ProjectileSpawnLocation.transform.rotation);
            if (transform.localScale.x < 0) {
                var scale = projectileObj.transform.localScale;
                scale.x *= -1;
                projectileObj.transform.localScale = scale;
            }
            Physics2D.IgnoreCollision(projectileObj.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            projectileObj.GetComponent<Projectile>().Initialize(m_Target);
        }

        /// <summary>
        /// Resets the animator state.
        /// </summary>
        private void ResetState()
        {
            m_Animator.SetInteger("State", 0);
        }

        /// <summary>
        /// The sprite has been selected.
        /// </summary>
        public void OnMouseDown()
        {
            if (m_Health.Value <= 0) {
                return;
            }

            m_TurnBasedSceneManager.SelectedCharacter(gameObject);
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;
    using UnityEngine.AI;

    /// <summary>
    /// Controlling logic on the Save Load Agent.
    /// </summary>
    public class SaveLoadAgent : MonoBehaviour
    {
        [Tooltip("A reference to the muzzle flash.")]
        [SerializeField] protected GameObject m_MuzzleFlashPrefab;
        [Tooltip("The location the muzzle flash should spawn.")]
        [SerializeField] protected GameObject m_MuzzleFlashSpawnLocation;
        [Tooltip("The index of the animator when the agent dies.")]
        [SerializeField] protected int m_DeathStateIndex;

        /// <summary>
        /// Fires the weapon.
        /// </summary>
        /// <param name="target"></param>
        public void Fire(GameObject target)
        {
            // Instant killshot.
            target.GetComponent<Animator>().SetInteger("State", m_DeathStateIndex);
            target.GetComponent<NavMeshAgent>().isStopped = true;
            target.GetComponent<Health>().Value = 0;

            if (m_MuzzleFlashPrefab != null) {
                GameObject.Instantiate(m_MuzzleFlashPrefab, m_MuzzleFlashSpawnLocation.transform.position, m_MuzzleFlashSpawnLocation.transform.rotation);
            }
        }
    }
}
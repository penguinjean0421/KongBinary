/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;
    using UnityEngine.Pool;

    /// <summary>
    /// Returns the object back to the object pool when the particle system has stopped.
    /// </summary>
    public class ReturnToParticlePool : MonoBehaviour
    {
        private ObjectPool<GameObject> m_Pool;

        public ObjectPool<GameObject> Pool { set { m_Pool = value; } }

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        private void Start()
        {
            var mainParticleSystem = GetComponent<ParticleSystem>().main;
            mainParticleSystem.stopAction = ParticleSystemStopAction.Callback;
        }

        /// <summary>
        /// Returns the particle system to the pool.
        /// </summary>
        private void OnParticleSystemStopped()
        {
            m_Pool.Release(gameObject);
        }
    }
}
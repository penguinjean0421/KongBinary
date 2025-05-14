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
    /// Restarts the hide and seek scene when the player enters the trigger.
    /// </summary>
    public class HideSeekRestartTrigger : MonoBehaviour
    {
        [Tooltip("A reference to the scene manager.")]
        [SerializeField] protected HideSeekSceneManager m_HideSeekSceneManager;

        /// <summary>
        /// The player has entered the trigger.
        /// </summary>
        /// <param name="other">The player (AI agents do not have a path generated near the trigger).</param>
        private void OnTriggerEnter(Collider other)
        {
            m_HideSeekSceneManager.Restart();
        }
    }
}
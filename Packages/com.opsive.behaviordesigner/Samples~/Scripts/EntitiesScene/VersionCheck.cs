/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// The entities sample scene can cause URP to crash. Require Unity 6.
    /// </summary>
    public class VersionCheck : MonoBehaviour
    {
        [Tooltip("A reference to the entity panel.")]
        [SerializeField] protected GameObject m_EntityPanel;

        /// <summary>
        /// Sets the UI state.
        /// </summary>
#if UNITY_6000_0_OR_NEWER
        private void Awake()
        {
            gameObject.SetActive(false);
            m_EntityPanel.SetActive(true);
        }
#endif
    }
}
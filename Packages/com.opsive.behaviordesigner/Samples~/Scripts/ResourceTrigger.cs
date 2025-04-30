/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Manages the resources when gathered.
    /// </summary>
    public class ResourceTrigger : MonoBehaviour
    {
        [Tooltip("The location that the resource should be moved to when harvested.")]
        [SerializeField] protected CraftTrigger m_CraftLocation;

        private GameObject[] m_Resources;
        private int m_Index;

        /// <summary>
        /// Finds all of the resources that belong to the trigger.
        /// </summary>
        private void Start()
        {
            m_Resources = new GameObject[transform.childCount];
            for (int i = 0; i < m_Resources.Length; ++i) {
                m_Resources[i] = transform.GetChild(i).gameObject;
            }
        }

        /// <summary>
        /// Pickups the resource.
        /// </summary>
        public void Pickup()
        {
            m_CraftLocation.Pickup(m_Resources[m_Index]);
            m_Index++;
        }
    }
}
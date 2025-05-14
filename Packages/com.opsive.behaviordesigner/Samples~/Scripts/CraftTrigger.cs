/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Manages the items when crafted.
    /// </summary>
    public class CraftTrigger : MonoBehaviour
    {
        [Tooltip("The locations to store the ingredients that can be crafted.")]
        [SerializeField] protected GameObject[] m_IngredientsStorage;
        [Tooltip("The item that is crafted.")]
        [SerializeField] protected GameObject m_CraftedItem;
        [Tooltip("The location that the resource should be moved to when harvested.")]
        [SerializeField] protected GameObject m_StorageLocation;
        [Tooltip("The seperation between crafted items.")]
        [SerializeField] protected float m_CraftedItemSeparation = 0.1f;

        private GameObject[] m_CraftItems = new GameObject[2];
        private int m_Index;

        /// <summary>
        /// Pickups the resource.
        /// </summary>
        public void Pickup(GameObject item)
        {
            item.transform.SetParent(m_IngredientsStorage[m_Index].transform);
            item.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            item.transform.localScale = Vector3.one;
            m_Index++;
        }

        /// <summary>
        /// Starts to craft the item.
        /// </summary>
        public void StartCraft()
        {
            for (int i = 0; i < m_IngredientsStorage.Length; ++i) {
                m_IngredientsStorage[i].transform.GetChild(m_IngredientsStorage[i].transform.childCount - 1).gameObject.SetActive(false);
            }
            m_Index = 0;
        }

        /// <summary>
        /// Crafts the item.
        /// </summary>
        public void Craft()
        {
            var craftedItem = GameObject.Instantiate(m_CraftedItem, m_StorageLocation.transform);
            craftedItem.transform.SetLocalPositionAndRotation(Vector3.forward * ((m_StorageLocation.transform.childCount - 1) * m_CraftedItemSeparation), Quaternion.identity);
        }
    }
}
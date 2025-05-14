/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Equips or unequips the specified item.
    /// </summary>
    public class EquipUnequipItem : Action
    {
        [Tooltip("Should the item be equipped?")]
        [SerializeField] protected SharedVariable<bool> m_Equip = true;
        [Tooltip("The item that should be equipped or unequipped.")]
        [SerializeField] protected SharedVariable<GameObject> m_Item;
        [Tooltip("The location that the item should be parented to. Set to null to instead activate or deactivate the item.")]
        [SerializeField] protected SharedVariable<GameObject> m_Parent;

        /// <summary>
        /// Equips or unequips the item on the character.
        /// </summary>
        /// <returns>Success after the character has teleported.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Parent.Value != null) {
                m_Item.Value.transform.SetParent(m_Parent.Value.transform);
                m_Item.Value.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            } else {
                // If the parent is null then the object should be activated or deactivated.
                m_Item.Value.SetActive(m_Equip.Value);
            }
            return TaskStatus.Success;
        }
    }
}
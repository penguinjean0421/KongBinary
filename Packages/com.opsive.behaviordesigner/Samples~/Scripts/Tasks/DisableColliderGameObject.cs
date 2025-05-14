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
    /// Disables the GameObject attached to the specified collider.
    /// </summary>
    public class DisableColliderGameObject : Action
    {
        [Tooltip("The collider whose GameObject should be disabled..")]
        [SerializeField] protected SharedVariable<Collider> m_Collider;

        /// <summary>
        /// Teleports the character.
        /// </summary>
        /// <returns>Success after the character has teleported.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Collider.Value.gameObject.SetActive(false);
            return TaskStatus.Success;
        }
    }
}
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
    /// Teleports the character to the specified location.
    /// </summary>
    public class Teleport : Action
    {
        [Tooltip("The teleport location.")]
        [SerializeField] protected SharedVariable<GameObject> m_Location;

        /// <summary>
        /// Teleports the character.
        /// </summary>
        /// <returns>Success after the character has teleported.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Transform.SetPositionAndRotation(m_Location.Value.transform.position, m_Location.Value.transform.rotation);
            return TaskStatus.Success;
        }
    }
}
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using UnityEngine;

    /// <summary>
    /// Picks up the next available resource.
    /// </summary>
    public class Gather : Action
    {
        protected override bool ReceiveTriggerEnterCallback => true;

        private ResourceTrigger m_ResourceTrigger;

        /// <summary>
        /// Picks up the resource.
        /// </summary>
        /// <returns>Success if the agent was able to pickup the resource, otherwise failure if the resource is null.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_ResourceTrigger == null) {
                return TaskStatus.Failure;
            }

            m_ResourceTrigger.Pickup();
            return TaskStatus.Success;
        }

        /// <summary>
        /// The agent has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger that the agent entered.</param>
        protected override void OnTriggerEnter(Collider other)
        {
            ResourceTrigger resourceTrigger;
            if ((resourceTrigger = other.GetComponent<ResourceTrigger>()) != null) {
                m_ResourceTrigger = resourceTrigger;
            }
        }
    }
}
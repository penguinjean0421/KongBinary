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
    /// Uses the ingredients to craft the item
    /// </summary>
    public class Craft : Action
    {
        [Tooltip("Should the crafting be started?")]
        [SerializeField] protected bool m_StartCraft;

        protected override bool ReceiveTriggerEnterCallback => true;

        private CraftTrigger m_CraftTrigger;

        /// <summary>
        /// Picks up the resource.
        /// </summary>
        /// <returns>Success if the agent was able to pickup the resource, otherwise failure if the resource is null.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_CraftTrigger == null) {
                return TaskStatus.Failure;
            }

            if (m_StartCraft) {
                m_CraftTrigger.StartCraft();
            } else {
                m_CraftTrigger.Craft();
            }
            return TaskStatus.Success;
        }

        /// <summary>
        /// The agent has entered a trigger.
        /// </summary>
        /// <param name="other">The trigger that the agent entered.</param>
        protected override void OnTriggerEnter(Collider other)
        {
            CraftTrigger craftTrigger;
            if ((craftTrigger = other.GetComponent<CraftTrigger>()) != null) {
                m_CraftTrigger = craftTrigger;
            }
        }
    }
}
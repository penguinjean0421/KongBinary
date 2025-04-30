#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Events
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// The EventNode that is invoked when the specified event is received.
    /// </summary>
    [AllowMultipleTypes]
    [NodeDescription("EventNode that is invoked when the specified event is received")]
    public class OnReceivedEvent : EventNode
    {
        [Tooltip("The name of the event that starts the branch.")]
        [SerializeField] protected SharedVariable<string> m_EventName;
        [Tooltip("Optionally store the first sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue1;
        [Tooltip("Optionally store the second sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue2;
        [Tooltip("Optionally store the third sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue3;

        private string m_RegisteredEventName;
        private bool m_Initialized;

        /// <summary>
        /// Initializes the node to the specified graph.
        /// </summary>
        /// <param name="graph">The graph that is initializing the task.</param>
        public override void Initialize(IGraph graph)
        {
            if (m_Initialized) {
                return;
            }
            m_Initialized = true;

            base.Initialize(graph);

            m_BehaviorTree.OnBehaviorTreeDestroyed += Destroy;

            m_EventName.OnValueChange += UpdateEvents;
            if (m_StoredValue1 != null) { m_StoredValue1.OnValueChange += UpdateEvents; }
            if (m_StoredValue2 != null) { m_StoredValue2.OnValueChange += UpdateEvents; }
            if (m_StoredValue3 != null) { m_StoredValue3.OnValueChange += UpdateEvents; }

            RegisterEvents();
        }

        /// <summary>
        /// Registers for the events.
        /// </summary>
        private void RegisterEvents()
        {
            if (m_StoredValue1 == null || !m_StoredValue1.IsShared) {
                EventHandler.RegisterEvent(m_BehaviorTree, m_EventName.Value, ReceivedEvent);
            } else {
                if (m_StoredValue2 == null || !m_StoredValue2.IsShared) {
                    EventHandler.RegisterEvent<object>(m_BehaviorTree, m_EventName.Value, ReceivedEvent);
                } else {
                    if (m_StoredValue3 == null || !m_StoredValue3.IsShared) {
                        EventHandler.RegisterEvent<object, object>(m_BehaviorTree, m_EventName.Value, ReceivedEvent);
                    } else {
                        EventHandler.RegisterEvent<object, object, object>(m_BehaviorTree, m_EventName.Value, ReceivedEvent);
                    }
                }
            }

            m_RegisteredEventName = m_EventName.Value;
        }

        /// <summary>
        /// Unregisters for the events that were registered.
        /// </summary>
        private void UnregisterEvents()
        {
            // The events must be registered first in order to be unregistered.
            if (string.IsNullOrEmpty(m_RegisteredEventName)) {
                return;
            }

            // Unregister from all parameters. This will ensure no events are subscribed if the parameters change.
            EventHandler.UnregisterEvent(m_BehaviorTree, m_RegisteredEventName, ReceivedEvent);
            EventHandler.UnregisterEvent<object>(m_BehaviorTree, m_RegisteredEventName, ReceivedEvent);
            EventHandler.UnregisterEvent<object, object>(m_BehaviorTree, m_RegisteredEventName, ReceivedEvent);
            EventHandler.UnregisterEvent<object, object, object>(m_BehaviorTree, m_RegisteredEventName, ReceivedEvent);

            m_RegisteredEventName = string.Empty;
        }

        /// <summary>
        /// The event name or parameter count has changed. Update the events.
        /// </summary>
        private void UpdateEvents()
        {
            UnregisterEvents();
            RegisterEvents();
        }

        /// <summary>
        /// The event has been received.
        /// </summary>
        private void ReceivedEvent()
        {
            m_BehaviorTree.StartBranch(this);
        }

        /// <summary>
        /// A single parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        private void ReceivedEvent(object arg1)
        {
            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }

            m_BehaviorTree.StartBranch(this);
        }

        /// <summary>
        /// A two parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        private void ReceivedEvent(object arg1, object arg2)
        {
            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }
            if (m_StoredValue2 != null && m_StoredValue2.IsShared) { m_StoredValue2.SetValue(arg2); }

            m_BehaviorTree.StartBranch(this);
        }

        /// <summary>
        /// A three parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        private void ReceivedEvent(object arg1, object arg2, object arg3)
        {
            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }
            if (m_StoredValue2 != null && m_StoredValue2.IsShared) { m_StoredValue2.SetValue(arg2); }
            if (m_StoredValue3 != null && m_StoredValue3.IsShared) { m_StoredValue3.SetValue(arg3); }

            m_BehaviorTree.StartBranch(this);
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        private void Destroy()
        {
            m_BehaviorTree.OnBehaviorTreeDestroyed -= Destroy;

            m_EventName.OnValueChange -= UpdateEvents;
            if (m_StoredValue1 != null) { m_StoredValue1.OnValueChange -= UpdateEvents; }
            if (m_StoredValue2 != null) { m_StoredValue2.OnValueChange -= UpdateEvents; }
            if (m_StoredValue3 != null) { m_StoredValue3.OnValueChange -= UpdateEvents; }

            UnregisterEvents();
            m_Initialized = false;
        }
    }
}
#endif
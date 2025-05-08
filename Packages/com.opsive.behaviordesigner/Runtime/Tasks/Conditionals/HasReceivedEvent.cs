#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Events;
    using UnityEngine;

    /// <summary>
    /// A TaskObject implementation of the Conditional task. This class can be used when the task should not be grouped by the StackedConditional task.
    /// </summary>
    [NodeIcon("e6fc90c130121da4f9067b5e15b02975", "69959064b54a0cb4cb077dbb6967a3e1")]
    [NodeDescription("Returns success as soon as the event specified by eventName has been received.")]
    public class HasReceivedEvent : TargetBehaviorTreeConditional
    {
        [Tooltip("The name of the event that should be registered.")]
        [SerializeField] protected SharedVariable<string> m_EventName;
        [Tooltip("Is the event a global event?")]
        [SerializeField] protected SharedVariable<bool> m_GlobalEvent;
        [Tooltip("Optionally store the first sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue1;
        [Tooltip("Optionally store the second sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue2;
        [Tooltip("Optionally store the third sent argument.")]
        [RequireShared] [SerializeField] protected SharedVariable m_StoredValue3;

        private string m_RegisteredEventName;
        private bool m_EventRegistered;
        private bool m_EventReceived;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            RegisterEvents();

            m_EventName.OnValueChange += UpdateEvents;
            if (m_StoredValue1 != null) { m_StoredValue1.OnValueChange += UpdateEvents; }
            if (m_StoredValue2 != null) { m_StoredValue2.OnValueChange += UpdateEvents; }
            if (m_StoredValue3 != null) { m_StoredValue3.OnValueChange += UpdateEvents; }
        }

        /// <summary>
        /// Initializes the target behavior tree.
        /// </summary>
        protected override void InitializeTarget()
        {
            if (m_ResolvedBehaviorTree != null) {
                UnregisterEvents();
            }

            base.InitializeTarget();

            RegisterEvents();
        }

        /// <summary>
        /// Registers for the events.
        /// </summary>
        private void RegisterEvents()
        {
            if (m_EventRegistered) {
                return;
            }

            if (string.IsNullOrEmpty(m_EventName.Value)) {
                Debug.LogError("Error: Unable to receive event. The event name is empty.");
                return;
            }
             
            if (m_StoredValue1 == null || !m_StoredValue1.IsShared) {
                if (m_GlobalEvent.Value) {
                    EventHandler.RegisterEvent(m_EventName.Value, ReceivedEvent);
                } else {
                    EventHandler.RegisterEvent(m_ResolvedBehaviorTree, m_EventName.Value, ReceivedEvent);
                }
            } else {
                if (m_StoredValue2 == null || !m_StoredValue2.IsShared) {
                    if (m_GlobalEvent.Value) {
                        EventHandler.RegisterEvent<object>(m_EventName.Value, ReceivedEvent);
                    } else {
                        EventHandler.RegisterEvent<object>(m_ResolvedBehaviorTree, m_EventName.Value, ReceivedEvent);
                    }
                } else {
                    if (m_StoredValue3 == null || !m_StoredValue3.IsShared) {
                        if (m_GlobalEvent.Value) {
                            EventHandler.RegisterEvent<object, object>(m_EventName.Value, ReceivedEvent);
                        } else {
                            EventHandler.RegisterEvent<object, object>(m_ResolvedBehaviorTree, m_EventName.Value, ReceivedEvent);
                        }
                    } else {
                        if (m_GlobalEvent.Value) {
                            EventHandler.RegisterEvent<object, object, object>(m_EventName.Value, ReceivedEvent);
                        } else {
                            EventHandler.RegisterEvent<object, object, object>(m_ResolvedBehaviorTree, m_EventName.Value, ReceivedEvent);
                        }
                    }
                }
            }

            m_EventRegistered = true;
            m_RegisteredEventName = m_EventName.Value;
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
        /// A parameterless event has been recevied.
        /// </summary>
        private void ReceivedEvent()
        {
            // The task has to be reevaluated in order for the event to be received.
            if (!IsReevaluating()) {
                return;
            }

            m_EventReceived = true;
        }

        /// <summary>
        /// A single parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        private void ReceivedEvent(object arg1)
        {
            // The task has to be reevaluated in order for the event to be received.
            if (!IsReevaluating()) {
                return;
            }

            m_EventReceived = true;

            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }
        }

        /// <summary>
        /// A two parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        private void ReceivedEvent(object arg1, object arg2)
        {
            // The task has to be reevaluated in order for the event to be received.
            if (!IsReevaluating()) {
                return;
            }

            m_EventReceived = true;

            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }
            if (m_StoredValue2 != null && m_StoredValue2.IsShared) { m_StoredValue2.SetValue(arg2); }
        }

        /// <summary>
        /// A three parameter event has been received.
        /// </summary>
        /// <param name="arg1">The first parameter.</param>
        /// <param name="arg2">The second parameter.</param>
        /// <param name="arg3">The third parameter.</param>
        private void ReceivedEvent(object arg1, object arg2, object arg3)
        {
            // The task has to be reevaluated in order for the event to be received.
            if (!IsReevaluating()) {
                return;
            }

            m_EventReceived = true;

            if (m_StoredValue1 != null && m_StoredValue1.IsShared) { m_StoredValue1.SetValue(arg1); }
            if (m_StoredValue2 != null && m_StoredValue2.IsShared) { m_StoredValue2.SetValue(arg2); }
            if (m_StoredValue3 != null && m_StoredValue3.IsShared) { m_StoredValue3.SetValue(arg3); }
        }

        /// <summary>
        /// The task has been updated.
        /// </summary>
        /// <returns>True if an event has been received.</returns>
        public override TaskStatus OnUpdate()
        {
            if (string.IsNullOrEmpty(m_EventName.Value)) {
                return TaskStatus.Failure;
            }

            return m_EventReceived ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// The task has ended.
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();

            m_EventReceived = false;
        }

        /// <summary>
        /// The behavior tree has been stopped.
        /// </summary>
        /// <param name="paused">Is the behavior tree paused?</param>
        public override void OnStopped(bool paused)
        {
            base.OnStopped(paused);

            UnregisterEvents();
            m_EventReceived = false;
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
            if (m_GlobalEvent.Value) {
                EventHandler.UnregisterEvent(m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object>(m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object, object>(m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object, object, object>(m_RegisteredEventName, ReceivedEvent);
            } else {
                EventHandler.UnregisterEvent(m_ResolvedBehaviorTree, m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object>(m_ResolvedBehaviorTree, m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object, object>(m_ResolvedBehaviorTree, m_RegisteredEventName, ReceivedEvent);
                EventHandler.UnregisterEvent<object, object, object>(m_ResolvedBehaviorTree, m_RegisteredEventName, ReceivedEvent);
            }

            m_EventRegistered = false;
            m_RegisteredEventName = string.Empty;
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();

            m_EventName.OnValueChange -= UpdateEvents;
            if (m_StoredValue1 != null) { m_StoredValue1.OnValueChange -= UpdateEvents; }
            if (m_StoredValue2 != null) { m_StoredValue2.OnValueChange -= UpdateEvents; }
            if (m_StoredValue3 != null) { m_StoredValue3.OnValueChange -= UpdateEvents; }
        }
    }
}
#endif
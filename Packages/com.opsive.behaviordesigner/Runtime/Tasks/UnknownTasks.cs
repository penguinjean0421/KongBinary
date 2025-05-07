#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Composites;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.Shared.Utility;
    using UnityEngine;

    /// <summary>
    /// Represents a task node that can no longer be found.
    /// </summary>
    [HideInFilterWindow]
    public class UnknownTaskNode : ActionNode
    {
        private string m_UnknownType;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnknownTaskNode() { }

        /// <summary>
        /// UnknownTaskNode constructor.
        /// </summary>
        /// <param name="unknownType">The type that cannot be found.</param>
        public UnknownTaskNode(string unknownType)
        {
            m_UnknownType = unknownType;
        }

        /// <summary>
        /// Called once when the behavior tree is initialized.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            Debug.LogWarning($"Warning: Unable to find the task of type {m_UnknownType}. An unknown task has been replaced with it.");
        }
    }

    /// <summary>
    /// Represents a task that can no longer be found.
    /// </summary>
    [HideInFilterWindow]
    public class UnknownTask : Action
    {
        /// <summary>
        /// Called once when the behavior tree is initialized.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            Debug.LogWarning($"Warning: Unable to find the original task. An unknown task has been replaced with it.");
        }
    }

    /// <summary>
    /// Represents a parent task node that can no longer be found.
    /// </summary>
    [HideInFilterWindow]
    public class UnknownParentTaskNode : CompositeNode
    {
        private string m_UnknownType;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UnknownParentTaskNode() { }

        /// <summary>
        /// UnknownParentTaskNode constructor.
        /// </summary>
        /// <param name="unknownType">The type that cannot be found.</param>
        public UnknownParentTaskNode(string unknownType)
        {
            m_UnknownType = unknownType;
        }

        /// <summary>
        /// Called once when the behavior tree is initialized.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            Debug.LogWarning($"Warning: Unable to find the task of type {m_UnknownType}. An unknown task has been replaced with it.");
        }
    }

    /// <summary>
    /// Represents an event node that can no longer be found.
    /// </summary>
    [HideInFilterWindow]
    public class UnknownEventTask : IEventNode
    {
        public ushort ConnectedIndex { get => ushort.MaxValue; set { } }
    }
}
#endif
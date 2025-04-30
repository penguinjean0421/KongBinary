#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Components
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Unity.Entities;
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The runtime DOTS data associated with a task.
    /// </summary>
    [System.Serializable]
    public struct TaskComponent : IBufferElementData, IEnableableComponent
    {
        [Tooltip("The current execution status of the task.")]
        //public TaskStatus Status;
        [SerializeField] private TaskStatus m_Status;
        public TaskStatus Status { get => m_Status; 
            set { 
                m_Status = value;
                CanReevaluate = value != TaskStatus.Inactive;
               // UnityEngine.Debug.Log(string.Format("{0} status: {1}", Index, value));
            } 
        }
        [Tooltip("The index of the task within the behavior tree.")]
        public ushort Index;
        [Tooltip("The index of the parent task within the behavior tree.")]
        public ushort ParentIndex;
        //public ushort m_ParentIndex;
        //public ushort ParentIndex { get { return m_ParentIndex; } set { UnityEngine.Debug.Log(Index + " parent: " + value); m_ParentIndex = value; } }
        [Tooltip("The index of the sibling task within the behavior tree.")]
        public ushort SiblingIndex;
        //public ushort m_SiblingIndex;
        //public ushort SiblingIndex { get { return m_SiblingIndex; } set { UnityEngine.Debug.Log(Index + " sibling: " + value); m_SiblingIndex = value; } }
        [Tooltip("The index of the branch the task executes within.")]
        public ushort BranchIndex;
        [Tooltip("The component type responsible for indicating that the task is active.")]
        public ComponentType TagComponentType;
        [Tooltip("Can the task be reevaluated with conditional aborts?")]
        [MarshalAs(UnmanagedType.U1)]
        public bool CanReevaluate;
        [Tooltip("Is the task being reevaluated with conditional aborts?")]
        [MarshalAs(UnmanagedType.U1)]
        public bool Reevaluate;
        [Tooltip("Is the task disabled?")]
        [MarshalAs(UnmanagedType.U1)]
        public bool Disabled;
    }

    /// <summary>
    /// Specifies if the behavior tree is enabled.
    /// </summary>
    public struct EnabledTag : IComponentData, IEnableableComponent
    {
    }

    /// <summary>
    /// Specifies when the behavior tree should be updated.
    /// </summary>
    public enum UpdateMode
    {
        EveryFrame, // The behavior tree should be updated every frame.
        Manual      // The behavior tree should be updated manually via a user script.
    }

    /// <summary>
    /// Specifies how many tasks should be evaluated. Evaluation will end if all branches return a status of TaskStatus.Running.
    /// </summary>
    public enum EvaluationType : byte
    {
        EntireTree, // Evaluates up to all of the tasks within the tree.
        Count       // Evaluates up to the specified MaxEvaluationCount.
    }

    /// <summary>
    /// Specifies if the tree should be evaluated.
    /// </summary>
    public struct EvaluationComponent : IComponentData, IEnableableComponent
    {
        [Tooltip("Specifies how many tasks should be updated during a single tick.")]
        public EvaluationType EvaluationType;
        [Tooltip("The maximum number of tasks that can run if the evaluation type is set to EvaluationType.Count.")]
        public ushort MaxEvaluationCount;
        [Tooltip("The number of tasks that have been evaluated within the current frame.")]
        public ushort EvaluationCount;
    }

    /// <summary>
    /// Specifies how the branch was interrupted.
    /// </summary>
    public enum InterruptType : byte
    {
        None,               // No interrupt.
        Branch,             // A conditional abort or utility selector triggered the interruption.
        ImmediateSuccess,   // The branch was interrupted with a success status.
        ImmediateFailure,   // The branch was interrupted with a failure status.
    }

    /// <summary>
    /// The runtime DOTS data associated with a branch.
    /// </summary>
    public struct BranchComponent : IBufferElementData
    {
        [Tooltip("The index of the task that is currently active.")]
        public int ActiveIndex;
        //public int m_ActiveIndex;
        //public int ActiveIndex { get { return m_ActiveIndex; } set { Debug.Log(string.Format("Active: {0}", value)); m_ActiveIndex = value; } }
        [Tooltip("The index of the task that should execute next.")]
        public int NextIndex;
        //public int m_NextIndex;
        //public int NextIndex { get { return m_NextIndex; } set { Debug.Log(string.Format("Next: {0}", value)); m_NextIndex = value; } }
        [Tooltip("The component tag that is active.")]
        public ComponentType ActiveTagComponentType;
        //public ComponentType m_ActiveTagComponentType;
        //public ComponentType ActiveTagComponentType { get { return m_ActiveTagComponentType; } set { Debug.Log(string.Format("Tag: {0}", value)); m_ActiveTagComponentType = value; } }
        [Tooltip("Specifies how the branch is interrupted.")]
        public InterruptType InterruptType;
        //public InterruptType m_InterruptType;
        //public InterruptType InterruptType { get { return m_InterruptType; } set { m_InterruptType = value; Debug.Log("Interrupt Type " + value); } }
        [Tooltip("The index of the task that caused an interruption. A value of 0 indicates no interruption.")]
        public ushort InterruptIndex;
    }

    /// <summary>
    /// Tag used to indicate when the branch should be interrupted.
    /// </summary>
    public struct InterruptTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Tag used to indicate that the branch has been interrupted.
    /// </summary>
    public struct InterruptedTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Specifies the reevaluation status of the task.
    /// </summary>
    public enum ReevaluateStatus : byte
    {
        Inactive,   // The task is not being reevaluated.
        Active,     // The task is currently being reevaluated.
        Dirty       // The task was reevaluated and triggered a change.
    }

    /// <summary>
    /// The runtime DOTS data associated with conditional aborts.
    /// </summary>
    public struct ReevaluateTaskComponent : IBufferElementData
    {
        [Tooltip("The index of the task.")]
        public ushort Index;
        [Tooltip("The type of conditional abort.")]
        public ConditionalAbortType AbortType;
        [Tooltip("The lower bound index of the next task if a lower priority abort is specified.")]
        public ushort LowerPriorityLowerIndex;
        [Tooltip("The upper bound index of the next task if a lower priority abort is specified.")]
        public ushort LowerPriorityUpperIndex;
        [Tooltip("The upper bound index of the next task if a self priority abort is specified.")]
        public ushort SelfPriorityUpperIndex;
        [Tooltip("The original status of the task.")]
        public TaskStatus OriginalStatus;
        [Tooltip("The tag specifiying the task should be reevaluated.")]
        public ComponentType ReevaluateTagComponentType;
        [Tooltip("The current reevaluation status of the task.")]
        public ReevaluateStatus ReevaluateStatus;
    }

    /// <summary>
    /// Runtime representation of IEventNodes that can run its own entity logic.
    /// </summary>
    public interface IEventNodeEntityReceiver
    {
        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        void AddBufferElement(World world, Entity entity);

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        void ClearBufferElement(World world, Entity entity);
    }
}
#endif
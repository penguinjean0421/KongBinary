#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using Unity.Entities;
    using static Opsive.BehaviorDesigner.Runtime.BehaviorTreeData;

    /// <summary>
    /// Interface for action tasks.
    /// </summary>
    [ReflectedType]
    [ObjectName("Actions")]
    public interface IAction { }

    /// <summary>
    /// Interface for composite tasks.
    /// </summary>
    [ObjectName("Composites")]
    public interface IComposite { }

    /// <summary>
    /// Interface for conditional tasks.
    /// </summary>
    [ReflectedType(typeof(bool))]
    [ObjectName("Conditionals")]
    public interface IConditional { }

    /// <summary>
    /// Interface for reference-based conditional tasks that can be reevaluated.
    /// </summary>
    public interface IConditionalReevaluation
    {
        /// <summary>
        /// Reevaluates the task logic. Returns a TaskStatus indicating how the behavior tree flow should proceed.
        /// </summary>
        /// <returns>The status of the task during the reevaluation phase.</returns>
        public TaskStatus OnReevaluateUpdate();
    }

    /// <summary>
    /// Interface for decorator tasks.
    /// </summary>
    [ObjectName("Decorators")]
    public interface IDecorator { }

    /// <summary>
    /// Interface which specifies the IComponentData is a task.
    /// </summary>
    public interface ITaskComponentData
    {
        /// <summary>
        /// The type of tag that should be enabled when the task is running.
        /// </summary>
        public ComponentType Tag { get; }

        /// <summary>
        /// The system type that the component uses.
        /// </summary>
        public System.Type SystemType { get; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity);

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity);
    }

    /// <summary>
    /// Interface describing tasks that can be reevaluated.
    /// </summary>
    public interface IReevaluateResponder
    {
        /// <summary>
        /// The type of tag that should be enabled when the task is being reevaluated.
        /// </summary>
        public ComponentType ReevaluateTag { get; }

        /// <summary>
        /// The system type that the reevaluation component uses.
        /// </summary>
        public System.Type ReevaluateSystemType { get; }
    }

    /// <summary>
    /// Interface describing tasks that can respond to interruptions.
    /// </summary>
    public interface IInterruptResponder
    {
        /// <summary>
        /// The system type that the interrupt component uses.
        /// </summary>
        public System.Type InterruptSystemType { get; }
    }

    /// <summary>
    /// Represents a task that can be saved and restored later.
    /// </summary>
    public interface ISavableTask
    {
        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public MemberVisibility GetSaveReflectionType(int index);

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public object Save(World world, Entity entity) { return null; }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public void Load(object saveData, World world, Entity entity) { }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        /// <param name="taskReferences">A reference to the list of task references that need to be resolved later.</param>
        public void Load(object saveData, World world, Entity entity, Dictionary<VariableAssignment, SharedVariable> variableByNameMap,
                                    ref ResizableArray<TaskAssignment> taskReferences) { Load(saveData, world, entity); }
    }
}
#endif
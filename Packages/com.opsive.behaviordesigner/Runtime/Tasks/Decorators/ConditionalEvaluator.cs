#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Utility;
    using Opsive.Shared.Utility;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// The Conditional Evaluator is a decorator that will evaluate the specified conditional task. This conditional task can be reevaluated while the current task is active.
    /// </summary>
    [NodeIcon("63d6a403c13816a49b58d1de830ca51e", "3d3c18273075b3f40b6c921943f33964")]
    [NodeDescription("Evaluates the specified conditional task. If the conditional task returns success then the child task is run and the child status is returned. If the conditional task does not " +
                     "return success then the child task is not run and a failure status is immediately returned.")]
    public class ConditionalEvaluator : DecoratorNode
    {
        [Tooltip("The target conditional task that should be evaluated.")]
        [SerializeField] [InspectNode] protected Conditional m_Task;

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public override void Reset()
        {
            m_Task = null;
        }

        /// <summary>
        /// Initializes the base task parameters.
        /// </summary>
        /// <param name="behaviorTree">A reference to the owning BehaviorTree.</param>
        /// <param name="runtimeIndex">The runtime index of the node.</param>
        internal override void Initialize(BehaviorTree behaviorTree, ushort runtimeIndex)
        {
            base.Initialize(behaviorTree, runtimeIndex);

            if (m_Task != null) {
                m_Task.Initialize(behaviorTree, runtimeIndex);
            }
        }

        /// <summary>
        /// The behavior tree has been initialized.
        /// </summary>
        public override void OnAwake()
        {
            if (m_Task == null) {
                Debug.LogError("Error: The task is null within the conditional evaluator.");
                return;
            }

            m_Task.OnAwake();
        }

        /// <summary>
        /// The tree has been started.
        /// </summary>
        public override void OnStarted()
        {
            m_Task?.OnStarted();
        }

        /// <summary>
        /// Starts evaluating the specified task.
        /// </summary>
        public override void OnStart()
        {
            m_Task?.OnStart();
        }

        /// <summary>
        /// Updates the task.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Task == null) {
                return TaskStatus.Failure;
            }
            var status = m_Task.OnUpdate();
            if (status == TaskStatus.Failure) {
                return TaskStatus.Failure;
            }

            // If the child task returns success or failure then that status should be returned. Otherwise return running.
            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity);
            var childStatus = taskComponents[Index + 1].Status; // Index + 1 will always be the task's only child.
            if (childStatus == TaskStatus.Success || childStatus == TaskStatus.Failure) {
                return childStatus;
            }
            return TaskStatus.Running;
        }

        /// <summary>
        /// The task has stopped execution.
        /// </summary>
        public override void OnEnd()
        {
            m_Task?.OnEnd();
        }

        /// <summary>
        /// The tree has been stopped.
        /// </summary>
        /// <param name="paused">Is the behavior tree paused?</param>
        public override void OnStopped(bool paused)
        {
            m_Task?.OnStopped(paused);
        }

        /// <summary>
        /// The tree has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            m_Task?.OnDestroy();
        }

        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public override MemberVisibility GetSaveReflectionType(int index)
        {
            if (m_Task == null) {
                return MemberVisibility.None;
            }

            return m_Task.GetSaveReflectionType(index);
        }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public override object Save(World world, Entity entity)
        {
            if (m_Task == null) {
                return null;
            }

            return m_Task.Save(world, entity);
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public override void Load(object saveData, World world, Entity entity)
        {
            if (m_Task == null) {
                return;
            }

            m_Task.Load(saveData, world, entity);
        }
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;

    /// <summary>
    /// A TaskObject implementation of the Conditional task.
    /// </summary>
    [NodeIcon("dea5c23eac9d12c4cbd380cc879816ea", "2963cf3eb0c036449829254b2074c4c3")]
    public abstract class Conditional : Task, IConditional, IConditionalReevaluation
    {
        /// <summary>
        /// Reevaluates the task logic. Returns a TaskStatus indicating how the behavior tree flow should proceed.
        /// </summary>
        /// <returns>The status of the task during the reevaluation phase.</returns>
        public virtual TaskStatus OnReevaluateUpdate() { return OnUpdate(); }

        /// <summary>
        /// Is the task being reevaluated? 
        /// </summary>
        /// <returns>True if the task is being reevaluated.</returns>
        protected bool IsReevaluating()
        {
            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity, true);
            if (m_RuntimeIndex >= taskComponents.Length) {
                return false;
            }

            return taskComponents[m_RuntimeIndex].Reevaluate;
        }
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime;

    /// <summary>
    /// The StackedConditional task allows for multiple conditionals to be added to the same node.
    /// </summary>
    [NodeIcon("b2368834b8b80144a8b1ab97b609e966", "86fbf527a2c761e45bc4a47cf4894902")]
    [NodeDescription("Allows multiple conditional tasks to be added to a single node.")]
    public class StackedConditional : StackedTask, IConditional, IConditionalReevaluation
    {
        /// <summary>
        /// Reevaluates the task logic. Returns a TaskStatus indicating how the behavior tree flow should proceed.
        /// </summary>
        /// <returns>The status of the task during the reevaluation phase.</returns>
        public TaskStatus OnReevaluateUpdate()
        {
            if (m_Tasks == null) {
                return TaskStatus.Failure;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (m_Tasks[i] == null) {
                    continue;
                }

                TaskStatus executionStatus;
                if (m_Tasks[i] is IConditionalReevaluation reevaluateTask) {
                    executionStatus = reevaluateTask.OnReevaluateUpdate();
                } else { // Use the regular update method if the task isn't designed for conditional aborts.
                    executionStatus = m_Tasks[i].OnUpdate();
                }
                if (m_ComparisonType == ComparisonType.Sequence && executionStatus == TaskStatus.Failure) {
                    return TaskStatus.Failure;
                } else if (m_ComparisonType == ComparisonType.Selector && executionStatus == TaskStatus.Success) {
                    return TaskStatus.Success;
                }
            }
            return m_ComparisonType == ComparisonType.Sequence ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
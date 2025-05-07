#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime;

    [NodeIcon("e0a8f1df788b6274a9a24003859dfa7e")]
    [NodeDescription("Returns true if the specified behavior tree is active.")]
    public class IsBehaviorTreeActive : TargetBehaviorTreeConditional
    {
        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_ResolvedBehaviorTree == null) {
                return TaskStatus.Failure;
            }

            return m_ResolvedBehaviorTree.IsActive() ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
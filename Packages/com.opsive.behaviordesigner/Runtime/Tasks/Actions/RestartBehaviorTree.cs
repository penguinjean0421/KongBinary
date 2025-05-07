#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.GraphDesigner.Runtime;
    using System.Collections;
    using UnityEngine;

    [NodeIcon("e0a8f1df788b6274a9a24003859dfa7e")]
    [NodeDescription("Restarts the specified behavior tree.")]
    public class RestartBehaviorTree : TargetBehaviorTreeAction
    {
        private TaskStatus m_Status;

        /// <summary>
        /// The task has started.
        /// </summary>
        public override void OnStart()
        {
            m_Status = TaskStatus.Queued;
        }

        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            // The coroutine has already been started if the status is not queued.
            if (m_Status != TaskStatus.Queued) {
                return m_Status;
            }

            if (m_ResolvedBehaviorTree == null) {
                return TaskStatus.Failure;
            }

            m_Status = TaskStatus.Running;
            StartCoroutine(RestartBehavior());
            return m_Status;
        }

        /// <summary>
        /// Restarts the behavior tree using a coroutine to allow structural changes.
        /// </summary>
        private IEnumerator RestartBehavior()
        {
            yield return new WaitForEndOfFrame();

            m_Status = m_ResolvedBehaviorTree.RestartBehavior() ? TaskStatus.Success : TaskStatus.Failure;
        }


    }
}
#endif
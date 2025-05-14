#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// A wrapper for the coroutine object in order to add support for coroutines within Tasks.
    /// </summary>
    public class TaskCoroutine
    {
        private BehaviorTree m_BehaviorTree;

        private IEnumerator m_CoroutineEnumerator;
        private Coroutine m_Coroutine;
        private string m_Name;
        private bool m_Stop;

        public Coroutine Coroutine { get => m_Coroutine; }

        /// <summary>
        /// Initializes and starts a coroutine.
        /// </summary>
        /// <param name="behaviorTree">The BehaviorTree that the coroutine has been added to.</param>
        /// <param name="coroutine">A reference to the coroutine.</param>
        /// <param name="name">The name of the coroutine.</param>
        public TaskCoroutine(BehaviorTree behaviorTree, IEnumerator coroutine, string name)
        {
            m_BehaviorTree = behaviorTree;
            m_CoroutineEnumerator = coroutine;
            m_Name = name;

            m_Coroutine = m_BehaviorTree.StartCoroutine(RunCoroutine());
        }

        /// <summary>
        /// Runs the coroutine until it is complete or has been stopped.
        /// </summary>
        /// <returns>The active coroutine.</returns>
        public IEnumerator RunCoroutine()
        {
            while (!m_Stop) {
                if (m_CoroutineEnumerator != null && m_CoroutineEnumerator.MoveNext()) {
                    yield return m_CoroutineEnumerator.Current;
                } else {
                    break;
                }
            }
            m_BehaviorTree.TaskCoroutineEnded(this, m_Name);
        }

        /// <summary>
        /// Stops the coroutine.
        /// </summary>
        public void Stop() { m_Stop = true; }
    }
}
#endif
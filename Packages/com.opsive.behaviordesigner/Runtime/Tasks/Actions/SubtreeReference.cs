#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// Allows for subtrees to be loaded at runtime into the tree.
    /// </summary>
    [NodeIcon("e0a8f1df788b6274a9a24003859dfa7e")]
    [NodeDescription("Loads the specified subtrees in at runtime.")]
    public class SubtreeReference : ActionNode, ISubtreeReference
    {
        [Tooltip("The subtrees that should be loaded.")]
        [SubtreeListAttribute]
        [SerializeField] protected Subtree[] m_Subtrees;
        [Tooltip("The variables that should override the subtree variables.")]
        [SharedVariableOverridesListAttribute]
        [SerializeField] protected SharedVariableOverride[] m_Variables;

        public virtual Subtree[] Subtrees { get { return m_Subtrees; } }

        /// <summary>
        /// A list of mapped SharedVariables. These variables can override the subtree.
        /// </summary>
        public virtual SharedVariableOverride[] SharedVariableOverrides { get => m_Variables; set => m_Variables = value; }

        /// <summary>
        /// Performs any runtime operations to evaluate the array of subtrees that should be returned.
        /// </summary>
        public virtual void EvaluateSubtrees() { }

        /// <summary>
        /// If the task exists at runtime then the subtree didn't load. Return failure.
        /// </summary>
        /// <returns>The failure TaskStatus.</returns>
        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Failure;
        }
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.GraphDesigner.Runtime.Variables;

    /// <summary>
    /// Interface for tasks that can load subtrees.
    /// </summary>
    public interface ISubtreeReference
    {
        /// <summary>
        /// A list of mapped SharedVariables. These variables can override the subtree.
        /// </summary>
        SharedVariableOverride[] SharedVariableOverrides { get; set; }

        /// <summary>
        /// Performs any runtime operations to evaluate the array of subtrees that should be returned.
        /// </summary>
        void EvaluateSubtrees();

        /// <summary>
        /// The Subtrees that should be used at runtime.
        /// </summary>
        Subtree[] Subtrees { get; }
    }
}
#endif
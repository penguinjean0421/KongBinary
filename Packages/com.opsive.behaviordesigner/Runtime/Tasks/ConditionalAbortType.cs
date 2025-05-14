#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    /// <summary>
    /// Specifies that the task can interrupt using conditional aborts.
    /// </summary>
    public interface IConditionalAbortParent
    {
        ConditionalAbortType AbortType { get; }
    }

    /// <summary>
    /// The type of conditional abort.
    /// </summary>
    public enum ConditionalAbortType : byte
    {
        None,           // No abort specified.
        LowerPriority,  // Any task to the right of the current branch can be aborted.
        Self,           // Any task within the current branch can be aborted.
        Both            // A combination of LowerPriority and Self.
    }
}
#endif
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    /// <summary>
    /// The execution status of the task.
    /// </summary>
    public enum TaskStatus : byte
    {
        Inactive,   // The task is inactive and is not running.
        Queued,     // The task will run on the next update.
        Running,    // The task is currently running.
        Success,    // The task succeeded execution.
        Failure,    // The task failed execution.
    }
}
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Utility
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Unity.Burst;
    using Unity.Entities;

    /// <summary>
    /// Utility functions that are used throughout the behavior tree execution.
    /// </summary>
    [BurstCompile]
    public static class TraversalUtility
    {
        /// <summary>
        /// Returns true if the specified index is a child of the parent index.
        /// </summary>
        /// <param name="index">The index to determine if it is a child.</param>
        /// <param name="parentIndex">The index of the parent.</param>
        /// <param name="taskComponents">The array of nodes.</param>
        /// <returns>True if the specified index is a child of the parent index.</returns>
        [BurstCompile]
        public static bool IsParent(ushort index, ushort parentIndex, ref DynamicBuffer<TaskComponent> taskComponents)
        {
            if (parentIndex == ushort.MaxValue || index == ushort.MaxValue) {
                return false;
            }

            // The child can be considered a parent of itself.
            if (parentIndex == index) {
                return true;
            }

            // Return true as soon as there is a parent.
            while (index != ushort.MaxValue) {
                if (index == parentIndex) {
                    return true;
                }

                index = taskComponents[index].ParentIndex;
            }

            return false;
        }

        /// <summary>
        /// Returns the total number of children belonging to the specified node.
        /// </summary>
        /// <param name="index">The index of the task to retrieve the child count of.</param>
        /// <param name="taskComponents">The array of nodes.</param>
        /// <returns>The total number of children belonging to the specified node.</returns>
        public static int GetChildCount(int index, ref DynamicBuffer<TaskComponent> taskComponents)
        {
            var taskComponent = taskComponents[index];
            if (taskComponent.SiblingIndex != ushort.MaxValue) {
                return taskComponent.SiblingIndex - taskComponent.Index - 1;
            }

            if (taskComponent.Index + 1 == taskComponents.Length) {
                return 0;
            }

            var childTaskComponent = taskComponents[taskComponent.Index + 1];
            if (childTaskComponent.ParentIndex != taskComponent.Index) {
                return 0;
            }

            // Determine the child count based off of the sibling index.
            var lastChildTaskComponent = childTaskComponent;
            while (childTaskComponent.ParentIndex == taskComponent.Index) {
                lastChildTaskComponent = childTaskComponent;
                if (childTaskComponent.SiblingIndex == ushort.MaxValue) {
                    break;
                }
                childTaskComponent = taskComponents[childTaskComponent.SiblingIndex];
            }

            return lastChildTaskComponent.Index - taskComponent.Index + GetChildCount(lastChildTaskComponent.Index, ref taskComponents);
        }

        /// <summary>
        /// Returns the immediate number of children belonging to the specified task.
        /// </summary>
        /// <param name="task">The task to retrieve the children of.</param>
        /// <param name="taskComponents">The list of tasks.</param>
        /// <returns>The number of immediate children belonging to the specified task.</returns>
        [BurstCompile]
        public static int GetImmediateChildCount(ref TaskComponent task, ref DynamicBuffer<TaskComponent> taskComponents)
        {
            var count = 0;
            var siblingIndex = task.Index + 1;
            while (siblingIndex < taskComponents.Length && taskComponents[siblingIndex].ParentIndex == task.Index) {
                count++;
                siblingIndex = taskComponents[siblingIndex].SiblingIndex;
            }
            return count;
        }
    }
}
#endif
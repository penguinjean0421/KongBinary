#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using System.Collections;
    using Unity.Entities;
    using UnityEngine;

    [NodeIcon("3c1366e1dc8fe0b46b4a6c8724194cdd", "5b924a7ff18f0544aaa585af94ac536c")]
    [NodeDescription("Iterates through each element of the list. For each execution the Iterator task will set the next Element within the specified List.")]
    public class Iterator : DecoratorNode
    {
        [Tooltip("The list that should be iterated upon.")]
        [SerializeField] protected SharedVariable m_List;
        [Tooltip("The current element within the list.")]
        [SerializeField] [RequireShared] protected SharedVariable m_Element;
        [Tooltip("Should the interator end on a failure?")]
        [SerializeField] protected SharedVariable<bool> m_EndOnFailure;

        private int m_Index;

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public override void Reset()
        {
            m_List = null;
            m_Element = null;
            m_EndOnFailure = false;
        }

        /// <summary>
        /// Callback when the task is started.
        /// </summary>
        public override void OnStart()
        {
            base.OnStart();
            m_Index = 0;
        }

        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_List == null) {
                return TaskStatus.Failure;
            }
            var list = m_List.GetValue() as IList;
            if (list == null || list.Count == 0) {
                return TaskStatus.Failure;
            }

            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity);
            if (taskComponents[Index + 1].Status == TaskStatus.Queued || taskComponents[Index + 1].Status == TaskStatus.Running) {
                return TaskStatus.Running;
            }

            if (taskComponents[Index + 1].Status == TaskStatus.Failure && m_EndOnFailure.Value) {
                return TaskStatus.Failure;
            }

            // End when there are no more list elements.
            if (m_Index >= list.Count) {
                return taskComponents[Index + 1].Status; // Index + 1 will always be the task's only child.
            }

            m_Element.SetValue(list[m_Index]);
            m_Index++;

            return TaskStatus.Running;
        }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public override object Save(World world, Entity entity)
        {
            return m_Index;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public override void Load(object saveData, World world, Entity entity)
        {
            m_Index = (int)saveData;
            var list = m_List.GetValue() as IList;
            if (list != null && m_Index < list.Count) {
                m_Element.SetValue(list[m_Index]);
            }
        }
    }
}
#endif
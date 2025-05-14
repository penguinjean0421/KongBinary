#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Actions.UnityObjects
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using System.Collections.Generic;
    using UnityEngine;

    [NodeDescription("Selects the GameObject from the list.")]
    public class SelectGameObjectFromList : Action
    {
        [Tooltip("The list of possible GameObjects.")]
        [SerializeField] protected SharedVariable<List<GameObject>> m_GameObjectList;
        [Tooltip("The index of the GameObject that should be selected.")]
        [SerializeField] protected SharedVariable<int> m_ElementIndex;
        [Tooltip("The selected GameObject.")]
        [RequireShared] [SerializeField] protected SharedVariable<GameObject> m_StoreResult;


        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_GameObjectList.Value == null || m_ElementIndex.Value < 0 || m_ElementIndex.Value > m_GameObjectList.Value.Count) {
                return TaskStatus.Failure;
            }

            m_StoreResult.Value = m_GameObjectList.Value[m_ElementIndex.Value];
            return TaskStatus.Success;
        }
    }
}
#endif
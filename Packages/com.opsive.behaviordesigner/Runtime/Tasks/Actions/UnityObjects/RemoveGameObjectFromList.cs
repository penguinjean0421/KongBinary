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

    [NodeDescription("Removes the GameObject from the list.")]
    public class RemoveGameObjectFromList : TargetGameObjectAction
    {
        [Tooltip("The list of possible GameObjects.")]
        [RequireShared] [SerializeField] protected SharedVariable<List<GameObject>> m_StoreResult;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            return m_StoreResult.Value.Remove(m_ResolvedGameObject) ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}
#endif
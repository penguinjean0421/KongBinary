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

    [NodeDescription("Adds the GameObject to the list.")]
    public class AddGameObjectToList : TargetGameObjectAction
    {
        [Tooltip("The list of possible GameObjects.")]
        [RequireShared] [SerializeField] protected SharedVariable<List<GameObject>> m_StoreResult;
        [Tooltip("Are duplicates allowed to be added?")]
        [SerializeField] protected SharedVariable<bool> m_AllowDuplicates = true;

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>The execution status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (!m_AllowDuplicates.Value && m_StoreResult.Value.Contains(m_ResolvedGameObject)) {
                return TaskStatus.Failure;
            }

            m_StoreResult.Value.Add(m_ResolvedGameObject);
            return TaskStatus.Success;
        }
    }
}
#endif
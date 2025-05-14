#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Groups
{
    using System;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Group that executes all of the tasks.
    /// </summary>
    [UpdateInGroup(typeof(TraversalSystemGroup))]
    public partial class TraversalTaskSystemGroup : ComponentSystemGroup
    {
        [Tooltip("Callback before the outher tasks are updated.")]
        public Action OnPreUpdate;
        [Tooltip("Callback after the outher tasks are updated.")]
        public Action OnPostUpdate;

        /// <summary>
        /// Updates the group.
        /// </summary>
        protected override void OnUpdate()
        {
            if (OnPreUpdate != null) {
                OnPreUpdate();
            }

            base.OnUpdate();

            if (OnPostUpdate != null) {
                OnPostUpdate();
            }
        }
    }
}
#endif
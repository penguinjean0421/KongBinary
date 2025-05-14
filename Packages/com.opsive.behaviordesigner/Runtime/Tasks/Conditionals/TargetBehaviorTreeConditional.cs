#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Conditionals
{
    using Opsive.GraphDesigner.Runtime.Variables;
    using UnityEngine;

    /// <summary>
    /// A TaskObject Conditional task which implements the shared TargetGameObject/TreeUserID objects.
    /// </summary>
    public abstract class TargetBehaviorTreeConditional : Conditional
    {
        [Tooltip("The GameObject of the target behavior tree. If the value is null the current GameObject will be used.")]
        [SerializeField] protected SharedVariable<GameObject> m_TargetGameObject;
        [Tooltip("The index of the tree if there are multiple behavior trees on the same GameObject.")]
        [SerializeField] protected SharedVariable<int> m_TreeIndex;

        protected BehaviorTree m_ResolvedBehaviorTree;

        /// <summary>
        /// Initializes the task.
        /// </summary>
        public override void OnAwake()
        {
            m_TargetGameObject.OnValueChange += InitializeTarget;
            m_TreeIndex.OnValueChange += InitializeTarget;

            InitializeTarget();
        }

        /// <summary>
        /// Initializes the target behavior tree.
        /// </summary>
        protected virtual void InitializeTarget()
        {
            if (m_TargetGameObject.Value == null) {
                m_ResolvedBehaviorTree = m_BehaviorTree;
            } else {
                var behaviorTrees = m_TargetGameObject.Value.GetComponents<BehaviorTree>();
                if (behaviorTrees.Length == 1) {
                    m_ResolvedBehaviorTree = behaviorTrees[0];
                } else if (behaviorTrees.Length > 1) {
                    for (int i = 0; i < behaviorTrees.Length; ++i) {
                        if (behaviorTrees[i].Index == m_TreeIndex.Value) {
                            m_ResolvedBehaviorTree = behaviorTrees[i];
                            break;
                        }
                    }
                    // If the UserID can't be found then use the first behavior tree.
                    if (m_ResolvedBehaviorTree == null) {
                        m_ResolvedBehaviorTree = behaviorTrees[0];
                    }
                }
            }
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            m_TargetGameObject.OnValueChange -= InitializeTarget;
            m_TreeIndex.OnValueChange -= InitializeTarget;
        }
    }
}
#endif
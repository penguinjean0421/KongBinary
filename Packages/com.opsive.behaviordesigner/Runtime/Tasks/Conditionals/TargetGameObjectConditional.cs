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
    /// A TaskObject Conditional task which returns the current GameObject if the target is null.
    /// </summary>
    public abstract class TargetGameObjectConditional : Conditional
    {
        [Tooltip("The GameObject of the target behavior tree. If the value is null the current GameObject will be used.")]
        [SerializeField] protected SharedVariable<GameObject> m_TargetGameObject;
        protected override GameObject gameObject => m_ResolvedGameObject;
        protected override Transform transform => m_ResolvedTransform;

        protected GameObject m_ResolvedGameObject;
        protected Transform m_ResolvedTransform;

        /// <summary>
        /// Initializes the task.
        /// </summary>
        public override void OnAwake()
        {
            m_TargetGameObject.OnValueChange += InitializeTarget;

            InitializeTarget();
        }

        /// <summary>
        /// Initializes the target GameObject.
        /// </summary>
        protected virtual void InitializeTarget()
        {
            m_ResolvedGameObject = m_TargetGameObject.Value == null ? m_GameObject : m_TargetGameObject.Value;
            m_ResolvedTransform = m_ResolvedGameObject.transform;
        }

        /// <summary>
        /// The behavior tree has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            m_TargetGameObject.OnValueChange -= InitializeTarget;
        }
    }
}
#endif
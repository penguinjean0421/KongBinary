#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;
    using static Opsive.BehaviorDesigner.Runtime.BehaviorTreeData;

    /// <summary>
    /// The base class for a Mono task.
    /// </summary>
    public abstract class Task : ISavableTask
    {
        protected GameObject m_GameObject;
        protected Transform m_Transform;
        protected BehaviorTree m_BehaviorTree;
        protected ushort m_RuntimeIndex;

        protected virtual GameObject gameObject { get => m_GameObject; }
        protected virtual Transform transform { get => m_Transform; }

        private TaskStatus m_Status;
        internal TaskStatus Status { get => m_Status; set => m_Status = value; }

        /// <summary>
        /// Adds the task to the behavior tree buffer.
        /// </summary>
        /// <param name="world">The world that the task runs in.</param>
        /// <param name="entity">The entity that the task is connected to.</param>
        /// <param name="behaviorTreeID">The ID of the behavior tree running the task.</param>
        /// <param name="index">The index of the task.</param>
        public virtual void AddBufferElement(World world, Entity entity, int behaviorTreeID, ushort index)
        {
            DynamicBuffer<TaskObjectComponent> buffer;
            if (world.EntityManager.HasBuffer<TaskObjectComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<TaskObjectComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<TaskObjectComponent>(entity);
            }
            buffer.Add(new TaskObjectComponent()
            {
                Index = index,
            });
        }

        /// <summary>
        /// Clears all component buffers from the behavior tree buffer.
        /// </summary>
        /// <param name="world">The world that the task runs in.</param>
        /// <param name="entity">The entity that the task is connected to.</param>
        public virtual void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<TaskObjectComponent> buffer;
            if (world.EntityManager.HasBuffer<TaskObjectComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<TaskObjectComponent>(entity);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Initializes the base task parameters.
        /// </summary>
        /// <param name="behaviorTree">A reference to the owning BehaviorTree.</param>
        /// <param name="runtimeIndex">The runtime index of the node.</param>
        internal virtual void Initialize(BehaviorTree behaviorTree, ushort runtimeIndex)
        {
            if (!Application.isPlaying && m_BehaviorTree == behaviorTree) {
                return;
            }

            m_BehaviorTree = behaviorTree;
            m_GameObject = m_BehaviorTree.gameObject;
            m_Transform = m_BehaviorTree.transform;
            m_RuntimeIndex = runtimeIndex;

            if (ReceiveDrawGizmosCallback) { m_BehaviorTree.OnBehaviorTreeDrawGizmos += OnDrawGizmos; }
            if (ReceiveDrawGizmosSelectedCallback) { m_BehaviorTree.OnBehaviorTreeDrawGizmosSelected += OnDrawGizmosSelected; }

            if (!Application.isPlaying) {
                return;
            }

            m_BehaviorTree.OnBehaviorTreeStarted += OnStarted;
            m_BehaviorTree.OnBehaviorTreeStopped += OnStopped;
            m_BehaviorTree.OnBehaviorTreeDestroyed += OnDestroy;
            if (ReceiveCollisionEnterCallback) { m_BehaviorTree.OnBehaviorTreeCollisionEnter += OnCollisionEnter; }
            if (ReceiveCollisionExitCallback) { m_BehaviorTree.OnBehaviorTreeCollisionExit += OnCollisionExit; }
            if (ReceiveCollisionEnter2DCallback) { m_BehaviorTree.OnBehaviorTreeCollisionEnter2D += OnCollisionEnter2D; }
            if (ReceiveCollisionExit2DCallback) { m_BehaviorTree.OnBehaviorTreeCollisionExit2D += OnCollisionExit2D; }
            if (ReceiveTriggerEnterCallback) { m_BehaviorTree.OnBehaviorTreeTriggerEnter += OnTriggerEnter; }
            if (ReceiveTriggerExitCallback) { m_BehaviorTree.OnBehaviorTreeTriggerExit += OnTriggerExit; }
            if (ReceiveTriggerEnter2DCallback) { m_BehaviorTree.OnBehaviorTreeTriggerEnter2D += OnTriggerEnter2D; }
            if (ReceiveTriggerExit2DCallback) { m_BehaviorTree.OnBehaviorTreeTriggerExit2D += OnTriggerExit2D; }
            if (ReceiveControllerColliderHitCallback) { m_BehaviorTree.OnBehaviorTreeControllerColliderHit += OnControllerColliderHit; }

            OnAwake();
        }

        /// <summary>
        /// Callback when the behavior tree is initialized.
        /// </summary>
        public virtual void OnAwake() { }

        /// <summary>
        /// Callback when the behavior tree is started.
        /// </summary>
        public virtual void OnStarted() { }

        /// <summary>
        /// Callback when the task is started.
        /// </summary>
        public virtual void OnStart() { }

        /// <summary>
        /// Executes the task logic. Returns a TaskStatus indicating how the behavior tree flow should proceed.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public virtual TaskStatus OnUpdate() { return TaskStatus.Success; }

        /// <summary>
        /// Callback when the task stops.
        /// </summary>
        public virtual void OnEnd() { }

        /// <summary>
        /// Calls Unity's GetComponent method.
        /// </summary>
        /// <typeparam name="T">The type of component that should be retireved.</typeparam>
        /// <returns>The retrieved component (can be null).</returns>
        protected T GetComponent<T>() where T : Component
        {
            return gameObject.GetComponent<T>();
        }

        /// <summary>
        /// Calls Unity's GetComponent method.
        /// </summary>
        /// <param name="type">The component type that should be retrieved.</param>
        /// <returns>The retrieved component (can be null).</returns>
        protected Component GetComponent(System.Type type)
        {
            return gameObject.GetComponent(type);
        }

        /// <summary>
        /// Calls Unity's TryGetComponent method.
        /// </summary>
        /// <typeparam name="T">The type of component that should be retireved.</typeparam>
        /// <param name="component">The retrieved component.</param>
        protected void TryGetComponent<T>(out T component) where T : Component
        {
            gameObject.TryGetComponent<T>(out component);
        }

        /// <summary>
        /// Calls Unity's TryGetComponent method.
        /// </summary>
        /// <param name="type">The type of component to get.</param>
        /// <param name="component">The retrieved component.</param>
        protected void TryGetComponent(System.Type type, out Component component)
        {
            gameObject.TryGetComponent(type, out component);
        }
        protected void StartCoroutine(string methodName) { m_BehaviorTree.StartTaskCoroutine(this, methodName); }
        protected Coroutine StartCoroutine(System.Collections.IEnumerator routine) { return m_BehaviorTree.StartCoroutine(routine); }
        protected Coroutine StartCoroutine(string methodName, object value) { return m_BehaviorTree.StartTaskCoroutine(this, methodName, value); }
        protected void StopCoroutine(string methodName) { m_BehaviorTree.StopTaskCoroutine(methodName); }
        protected void StopCoroutine(System.Collections.IEnumerator routine) { m_BehaviorTree.StopCoroutine(routine); }
        protected void StopAllCoroutines() { m_BehaviorTree.StopAllTaskCoroutines(); }

        protected virtual bool ReceiveCollisionEnterCallback => false;
        /// <summary>
        /// Callback when OnCollisionEnter is triggered. This callback will only be received when ReceiveCollisionEnterCallback is true.
        /// </summary>
        /// <param name="collision">The resulting collision.</param>
        protected virtual void OnCollisionEnter(Collision collision) { }

        protected virtual bool ReceiveCollisionExitCallback => false;
        /// <summary>
        /// Callback when OnCollisionExit is triggered. This callback will only be received when ReceiveCollisionExitCallback is true.
        /// </summary>
        /// <param name="collision">The resulting collision.</param>
        protected virtual void OnCollisionExit(Collision collision) { }

        protected virtual bool ReceiveCollisionEnter2DCallback => false;
        /// <summary>
        /// Callback when OnCollisionEnter2D is triggered. This callback will only be received when ReceiveCollisionEnter2DCallback is true.
        /// </summary>
        /// <param name="collision">The resulting collision.</param>
        protected virtual void OnCollisionEnter2D(Collision2D collision) { }

        protected virtual bool ReceiveCollisionExit2DCallback => false;
        /// <summary>
        /// Callback when OnCollisionExit2D is triggered. This callback will only be received when ReceiveCollisionExit2DCallback is true.
        /// </summary>
        /// <param name="collision">The resulting collision.</param>
        protected virtual void OnCollisionExit2D(Collision2D collision) { }

        protected virtual bool ReceiveTriggerEnterCallback => false;
        /// <summary>
        /// Callback when OnTriggerEnter is triggered. This callback will only be received when ReceiveTriggerEnterCallback is true.
        /// </summary>
        /// <param name="other">The overlapping collider.</param>
        protected virtual void OnTriggerEnter(Collider other) { }

        protected virtual bool ReceiveTriggerExitCallback => false;
        /// <summary>
        /// Callback when OnTriggerExit is triggered. This callback will only be received when ReceiveTriggerExitCallback is true.
        /// </summary>
        /// <param name="other">The overlapping collider.</param>
        protected virtual void OnTriggerExit(Collider other) { }

        protected virtual bool ReceiveTriggerEnter2DCallback => false;
        /// <summary>
        /// Callback when OnTriggerEnter2D is triggered. This callback will only be received when ReceiveTriggerEnter2DCallback is true.
        /// </summary>
        /// <param name="other">The overlapping collider.</param>
        protected virtual void OnTriggerEnter2D(Collider2D other) { }

        protected virtual bool ReceiveTriggerExit2DCallback => false;
        /// <summary>
        /// Callback when OnTriggerExit2D is triggered. This callback will only be received when ReceiveTriggerExit2DCallback is true.
        /// </summary>
        /// <param name="other">The overlapping collider.</param>
        protected virtual void OnTriggerExit2D(Collider2D other) { }

        protected virtual bool ReceiveControllerColliderHitCallback => false;
        /// <summary>
        /// Callback when OnControllerColliderHit is triggered. This callback will only be received when ReceiveControllerColliderHitCallback is true.
        /// </summary>
        /// <param name="hit">The hit result.</param>
        protected virtual void OnControllerColliderHit(ControllerColliderHit hit) { }

        protected virtual bool ReceiveDrawGizmosCallback => false;
        /// <summary>
        /// Callback when OnDrawGizmos is triggered. This callback will only be received when ReceiveDrawGizmosCallback is true.
        /// </summary>
        protected virtual void OnDrawGizmos() { }

        protected virtual bool ReceiveDrawGizmosSelectedCallback => false;
        /// <summary>
        /// Callback when OnDrawGizmosSelected is triggered. This callback will only be received when ReceiveDrawGizmosSelectedCallback is true.
        /// </summary>
        protected virtual void OnDrawGizmosSelected() { }

        /// <summary>
        /// Callback when the behavior tree is stopped.
        /// </summary>
        /// <param name="paused">Is the behavior tree paused?</param>
        public virtual void OnStopped(bool paused) { }

        /// <summary>
        /// Callback when the behavior tree is destroyed.
        /// </summary>
        public virtual void OnDestroy() 
        {
            if (m_BehaviorTree == null) {
                return;
            }

            m_BehaviorTree.OnBehaviorTreeStarted -= OnStarted;
            m_BehaviorTree.OnBehaviorTreeStopped -= OnStopped;
            m_BehaviorTree.OnBehaviorTreeDestroyed -= OnDestroy;
            if (ReceiveCollisionEnterCallback) { m_BehaviorTree.OnBehaviorTreeCollisionEnter -= OnCollisionEnter; }
            if (ReceiveCollisionExitCallback) { m_BehaviorTree.OnBehaviorTreeCollisionExit -= OnCollisionExit; }
            if (ReceiveCollisionEnter2DCallback) { m_BehaviorTree.OnBehaviorTreeCollisionEnter2D -= OnCollisionEnter2D; }
            if (ReceiveCollisionExit2DCallback) { m_BehaviorTree.OnBehaviorTreeCollisionExit2D -= OnCollisionExit2D; }
            if (ReceiveTriggerEnterCallback) { m_BehaviorTree.OnBehaviorTreeTriggerEnter -= OnTriggerEnter; }
            if (ReceiveTriggerExitCallback) { m_BehaviorTree.OnBehaviorTreeTriggerExit -= OnTriggerExit; }
            if (ReceiveTriggerEnter2DCallback) { m_BehaviorTree.OnBehaviorTreeTriggerEnter2D -= OnTriggerEnter2D; }
            if (ReceiveTriggerExit2DCallback) { m_BehaviorTree.OnBehaviorTreeTriggerExit2D -= OnTriggerExit2D; }
            if (ReceiveControllerColliderHitCallback) { m_BehaviorTree.OnBehaviorTreeControllerColliderHit -= OnControllerColliderHit; }
            if (ReceiveDrawGizmosCallback) { m_BehaviorTree.OnBehaviorTreeDrawGizmos -= OnDrawGizmos; }
            if (ReceiveDrawGizmosSelectedCallback) { m_BehaviorTree.OnBehaviorTreeDrawGizmosSelected -= OnDrawGizmosSelected; }
        }

        /// <summary>
        /// Overrides ToString providing a nicer string value of the task.
        /// </summary>
        /// <returns>The overloaded ToString value.</returns>
        public override string ToString()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public virtual MemberVisibility GetSaveReflectionType(int index) { return MemberVisibility.Public; }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public virtual object Save(World world, Entity entity)
        {
            return null;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        public virtual void Load(object saveData, World world, Entity entity) { }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        /// <param name="taskReferences">A reference to the list of task references that need to be resolved later.</param>
        public virtual void Load(object saveData, World world, Entity entity, Dictionary<VariableAssignment, SharedVariable> variableByNameMap,
                                    ref ResizableArray<TaskAssignment> taskReferences)
        {
            Load(saveData, world, entity);
        }
    }
}
#endif
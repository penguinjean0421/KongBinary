#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;
    using static Opsive.BehaviorDesigner.Runtime.BehaviorTreeData;

    /// <summary>
    /// The StackedTask task allows for multiple tasks to be added to a single node.
    /// </summary>
    [HideInFilterWindow]
    [NodeIcon("e0a8f1df788b6274a9a24003859dfa7e")]
    public abstract class StackedTask : Task, ILogicNode, IStackedNode
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        /// <summary>
        /// Specifies how the tasks should be compared.
        /// </summary>
        public enum ComparisonType
        {
            Sequence,   // AND.
            Selector    // OR.
        }

        [Tooltip("The tasks that should run.")]
        [SerializeField] protected Task[] m_Tasks;
        [Tooltip("Specifies if the tasks should be traversed with an AND (Sequence) or an OR (Selector).")]
        [SerializeField] protected ComparisonType m_ComparisonType;

        private ushort m_ActiveIndex;
        private bool[] m_TaskStarted;
        private bool[] m_TaskEnded;

        public ushort ActiveIndex { get => m_ActiveIndex; }
        public object[] Nodes { get => m_Tasks; }
        public Task[] Tasks { get => m_Tasks; set => m_Tasks = value; }

        /// <summary>
        /// Adds the object to the action array.
        /// </summary>
        /// <param name="obj">The object that should be added.</param>
        public void Add(object obj)
        {
            Task task;
            if (obj is System.Reflection.MethodInfo) {
                // A delegate action needs to be created.
                var methodInfo = obj as System.Reflection.MethodInfo;
                var parameters = methodInfo.GetParameters();
                var types = new Type[(parameters != null ? parameters.Length : 0) + ((methodInfo.ReturnType != typeof(void)) ? 1 : 0)];
                if (parameters != null) {
                    for (int i = 0; i < parameters.Length; ++i) {
                        types[i] = parameters[i].ParameterType;
                    }
                }

                Type baseType;
                if (methodInfo.ReturnType == typeof(void)) {
                    if (parameters != null && parameters.Length > 0) {
                        if (parameters.Length == 1) { baseType = typeof(TaskDelegate<>); }
                        else if (parameters.Length == 2) { baseType = typeof(TaskDelegate<,>); }
                        else if (parameters.Length == 3) { baseType = typeof(TaskDelegate<,,>); }
                        else if (parameters.Length == 4) { baseType = typeof(TaskDelegate<,,,>); }
                        else if (parameters.Length == 5) { baseType = typeof(TaskDelegate<,,,,>); }
                        else if (parameters.Length == 6) { baseType = typeof(TaskDelegate<,,,,,>); }
                        else if (parameters.Length == 7) { baseType = typeof(TaskDelegate<,,,,,,>); }
                        else if (parameters.Length == 8) { baseType = typeof(TaskDelegate<,,,,,,,>); }
                        else if (parameters.Length == 9) { baseType = typeof(TaskDelegate<,,,,,,,,>); }
                        else if (parameters.Length == 10) { baseType = typeof(TaskDelegate<,,,,,,,,,>); }
                        else { Debug.LogError($"Error: Unable to create TaskDelegate with {parameters.Length}. Please send this error to support@opsive.com."); return; }
                    } else {
                        baseType = typeof(TaskDelegate);
                    }
                } else {
                    // The method has a returned parameter.
                    types[types.Length - 1] = methodInfo.ReturnType;
                    if (parameters != null && parameters.Length > 0) {
                        if (parameters.Length == 1) { baseType = typeof(TaskValueDelegate<,>); }
                        else if (parameters.Length == 2) { baseType = typeof(TaskValueDelegate<,,>); }
                        else if (parameters.Length == 3) { baseType = typeof(TaskValueDelegate<,,,>); }
                        else if (parameters.Length == 4) { baseType = typeof(TaskValueDelegate<,,,,>); }
                        else if (parameters.Length == 5) { baseType = typeof(TaskValueDelegate<,,,,,>); }
                        else if (parameters.Length == 6) { baseType = typeof(TaskValueDelegate<,,,,,,>); }
                        else if (parameters.Length == 7) { baseType = typeof(TaskValueDelegate<,,,,,,,>); }
                        else if (parameters.Length == 8) { baseType = typeof(TaskValueDelegate<,,,,,,,,>); }
                        else if (parameters.Length == 9) { baseType = typeof(TaskValueDelegate<,,,,,,,,,>); }
                        else if (parameters.Length == 10) { baseType = typeof(TaskValueDelegate<,,,,,,,,,>); }
                        else { Debug.LogError($"Error: Unable to create TaskValueDelegate with {parameters.Length}. Please send this error to support@opsive.com."); return; }
                    } else {
                        baseType = typeof(TaskValueDelegate<>);
                    }
                }

                Type actionDelegateType;
                if (types.Length > 0) {
                    actionDelegateType = baseType.MakeGenericType(types);
                } else {
                    actionDelegateType = baseType;
                }

                // The Action Delegate needs to be initialized to the method.
                var actionDelegate = Activator.CreateInstance(actionDelegateType) as TaskDelegateBase;
                actionDelegate.Bind(methodInfo);

                task = actionDelegate;
            } else if (obj is Type) {
                task = Activator.CreateInstance((Type)obj) as Task;
            } else { // Task.
                task = obj as Task;
            }

            task.Reset();

            if (m_Tasks == null) {
                m_Tasks = new Task[] { task };
            } else {
                Array.Resize(ref m_Tasks, m_Tasks.Length + 1);
                m_Tasks[m_Tasks.Length - 1] = task;
            }
        }

        /// <summary>
        /// Removes the action at the specified index.
        /// </summary>
        /// <param name="index">The index of the action that should be removed.</param>
        public void Remove(int index)
        {
            if (index < 0 || index >= m_Tasks.Length) {
                return;
            }

            m_Tasks[index].OnDestroy();
            for (int i = index; i < m_Tasks.Length - 1; ++i) {
                m_Tasks[i] = m_Tasks[i + 1];
            }
            Array.Resize(ref m_Tasks, m_Tasks.Length - 1);
        }

        /// <summary>
        /// Resets the task values back to their default.
        /// </summary>
        public override void Reset()
        {
            if (m_Tasks == null) {
                return;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                m_Tasks[i].Reset();
            }
        }

        /// <summary>
        /// Initializes the base task parameters.
        /// </summary>
        /// <param name="behaviorTree">A reference to the owning BehaviorTree.</param>
        /// <param name="runtimeIndex">The runtime index of the node.</param>
        internal override void Initialize(BehaviorTree behaviorTree, ushort runtimeIndex)
        {
            if (m_Tasks != null) {
                m_TaskStarted = new bool[m_Tasks.Length];
                m_TaskEnded = new bool[m_Tasks.Length];
                for (int i = 0; i < m_Tasks.Length; ++i) {
                    if (m_Tasks[i] == null) {
                        continue;
                    }
                    if (m_Tasks[i] is TaskDelegateBase taskDelegate) {
                        taskDelegate.Initialize(behaviorTree, runtimeIndex, this is IConditional);
                    } else {
                        m_Tasks[i].Initialize(behaviorTree, runtimeIndex);
                    }
                }
            }

            base.Initialize(behaviorTree, runtimeIndex);
        }

        /// <summary>
        /// Called when the task is started.
        /// </summary>
        public override void OnStart()
        {
            if (m_Tasks == null) {
                return;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (m_Tasks[i] == null) {
                    continue;
                }

                m_TaskStarted[i] = false;
                m_TaskEnded[i] = false;
            }
        }

        /// <summary>
        /// Updates all of the child tasks.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            if (m_Tasks == null || m_Tasks.Length == 0) {
                return TaskStatus.Failure;
            }

            while (m_ActiveIndex < m_Tasks.Length) {
                if (m_Tasks[m_ActiveIndex] == null) {
                    continue;
                }

                // Call start when the local task is started, not when the StackedTask starts.
                if (!m_TaskStarted[m_ActiveIndex]) {
                    m_Tasks[m_ActiveIndex].OnStart();
                    m_TaskStarted[m_ActiveIndex] = true;
                }

                var executionStatus = m_Tasks[m_ActiveIndex].OnUpdate();
                if (executionStatus == TaskStatus.Running) {
                    return TaskStatus.Running;
                }

                if (m_ComparisonType == ComparisonType.Sequence && executionStatus == TaskStatus.Failure) {
                    return TaskStatus.Failure;
                } else if (m_ComparisonType == ComparisonType.Selector && executionStatus == TaskStatus.Success) {
                    return TaskStatus.Success;
                }

                if (!m_TaskEnded[m_ActiveIndex]) {
                    m_Tasks[m_ActiveIndex].OnEnd();
                    m_TaskEnded[m_ActiveIndex] = true;
                }
                m_ActiveIndex++;
            }

            return m_ComparisonType == ComparisonType.Sequence ? TaskStatus.Success : TaskStatus.Failure;
        }

        /// <summary>
        /// Called when the task stops.
        /// </summary>
        public override void OnEnd()
        {
            if (m_TaskEnded == null) {
                return;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (m_Tasks[i] == null) {
                    continue;
                }
                if (!m_TaskEnded[i]) {
                    m_Tasks[i].OnEnd();
                }

                m_TaskStarted[i] = false;
                m_TaskEnded[i] = false;
            }
            m_ActiveIndex = 0;
        }

        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public override MemberVisibility GetSaveReflectionType(int index) { return index < 0 || index >= m_Tasks.Length ? MemberVisibility.None : m_Tasks[index].GetSaveReflectionType(index); }

        /// <summary>
        /// Returns the current task state.
        /// </summary>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <returns>The current task state.</returns>
        public override object Save(World world, Entity entity)
        {
            if (m_Tasks == null) {
                return null;
            }

            var saveData = new object[m_Tasks.Length + 1];
            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (m_Tasks[i] == null) {
                    continue;
                }
                var reflectionType = m_Tasks[i].GetSaveReflectionType(i);
                if (reflectionType != MemberVisibility.None) {
                    saveData[i] = Serialization.Serialize(m_Tasks[i], reflectionType, BehaviorTreeData.ValidateSerializedObject);
                } else {
                    saveData[i] = m_Tasks[i].Save(world, entity);
                }
            }
            saveData[m_Tasks.Length] = m_ActiveIndex;

            return saveData;
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        /// <param name="taskReferences">A reference to the list of task references that need to be resolved later.</param>
        public override void Load(object saveData, World world, Entity entity, Dictionary<VariableAssignment, SharedVariable> variableByNameMap,
                                    ref ResizableArray<TaskAssignment> taskReferences)
        {
            if (m_Tasks == null || saveData == null) {
                return;
            }

            var taskData = (object[])saveData;
            if (taskData.Length != m_Tasks.Length + 1) {
                Debug.LogError("Error: The save data does not match the task data length.");
                return;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (taskData[i] == null) {
                    continue;
                }

                Load(m_Tasks[i], i, taskData[i], world, entity, variableByNameMap, ref taskReferences);
            }
            m_ActiveIndex = (ushort)taskData[m_Tasks.Length];
        }

        /// <summary>
        /// Loads the previous task state.
        /// </summary>
        /// <param name="task">The task that the saveData belongs to.</param>
        /// <param name="index">The index of the task within the Tasks array.</param>
        /// <param name="saveData">The previous task state.</param>
        /// <param name="world">The DOTS world.</param>
        /// <param name="entity">The DOTS entity.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        /// <param name="taskReferences">A reference to the list of task references that need to be resolved later.</param>
        protected virtual void Load(Task task, int index, object saveData, World world, Entity entity, 
                                    Dictionary<VariableAssignment, SharedVariable> variableByNameMap, ref ResizableArray<TaskAssignment> taskReferences)
        {
            var reflectionType = task.GetSaveReflectionType(index);
            if (reflectionType != MemberVisibility.None) {
                var localTaskReferences = taskReferences;
                (saveData as Serialization).DeserializeFields(task, reflectionType, BehaviorTreeData.ValidateDeserializedTypeObject,
                                    (object fieldInfoObj, object task, object value) =>
                                    {
                                        return BehaviorTreeData.ValidateDeserializedObject(fieldInfoObj, task, value, ref variableByNameMap, ref localTaskReferences);
                                    });
                taskReferences = localTaskReferences;
            } else {
                task.Load(saveData, world, entity, variableByNameMap, ref taskReferences);
            }
        }

        /// <summary>
        /// The task has been destroyed.
        /// </summary>
        public override void OnDestroy()
        {
            if (m_Tasks != null) {
                for (int i = 0; i < m_Tasks.Length; ++i) {
                    if (m_Tasks[i] == null) {
                        continue;
                    }
                    m_Tasks[i].OnDestroy();
                }
            }

            base.OnDestroy();
        }
    }
}
#endif
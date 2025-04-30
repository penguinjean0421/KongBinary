#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Events;
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.GraphDesigner.Runtime.Utility;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Storage class for the graph data.
    /// </summary>
    [System.Serializable]
    public class BehaviorTreeData
    {
        [Tooltip("The serialized Task data.")]
        [SerializeField] private Serialization[] m_TaskData;
        [Tooltip("The serialized EventTask data.")]
        [SerializeField] private Serialization[] m_EventTaskData;
        [Tooltip("The serialized SharedVariable data.")]
        [SerializeField] private Serialization[] m_SharedVariableData;
        [Tooltip("The serialized disabled event nodes data.")]
        [SerializeField] private Serialization[] m_DisabledEventNodesData;
        [Tooltip("The serialized disabled logic nodes data.")]
        [SerializeField] private Serialization[] m_DisabledLogicNodesData;
        [Tooltip("The unique ID of the data.")]
        [SerializeField] private int m_UniqueID;

        private ILogicNode[] m_Tasks;
        private IEventNode[] m_EventTasks;
        private SharedVariable[] m_SharedVariables;
        private ushort[] m_DisabledLogicNodes;
        private ushort[] m_DisabledEventNodes;
        private Dictionary<VariableAssignment, SharedVariable> m_VariableByNameMap;
        private int m_RuntimeUniqueID;

        public ILogicNode[] LogicNodes
        {
            get => m_Tasks;
            set {
                if (value == null) {
                    m_Tasks = null;
                } else {
                    if (m_Tasks == null) {
                        m_Tasks = new ILogicNode[value.Length];
                    } else if (m_Tasks.Length != value.Length) {
                        Array.Resize(ref m_Tasks, value.Length);
                    }
                    for (int i = 0; i < value.Length; ++i) {
                        m_Tasks[i] = value[i];
                    }
                }
            }
        }
        public IEventNode[] EventNodes
        {
            get => m_EventTasks;
            set {
                if (value == null) {
                    m_EventTasks = null;
                } else {
                    if (m_EventTasks == null) {
                        m_EventTasks = new IEventNode[value.Length];
                    } else if (m_EventTasks.Length != value.Length) {
                        Array.Resize(ref m_EventTasks, value.Length);
                    }
                    for (int i = 0; i < value.Length; ++i) {
                        m_EventTasks[i] = value[i];
                    }
                }
            }
        }
        public SharedVariable[] SharedVariables { get => m_SharedVariables; set => m_SharedVariables = value; }
        public int UniqueID { get => m_RuntimeUniqueID != 0 ? m_RuntimeUniqueID : m_UniqueID; }
        public ushort[] DisabledLogicNodes { get => m_DisabledLogicNodes; set => m_DisabledLogicNodes = value; }
        public ushort[] DisabledEventNodes { get => m_DisabledEventNodes; set => m_DisabledEventNodes = value; }

#if UNITY_EDITOR
        [Tooltip("The serialized logic node properties data.")]
        [SerializeField] private Serialization[] m_LogicNodePropertiesData;
        [Tooltip("The serialized event node properties data.")]
        [SerializeField] private Serialization[] m_EventNodePropertiesData;
        [Tooltip("The serialized group properties data.")]
        [SerializeField] private Serialization[] m_GroupPropertiesData;

        private LogicNodeProperties[] m_LogicNodeProperties;
        private NodeProperties[] m_EventNodeProperties;
        private GroupProperties[] m_GroupProperties;

        public LogicNodeProperties[] LogicNodeProperties { get => m_LogicNodeProperties; set { m_LogicNodeProperties = value; } }
        public NodeProperties[] EventNodeProperties { get => m_EventNodeProperties; set { m_EventNodeProperties = value; } }
        public GroupProperties[] GroupProperties { get => m_GroupProperties; set => m_GroupProperties = value; }
#endif

        private ResizableArray<SubtreeNodesReference> m_SubtreeNodesReference;
        [System.NonSerialized] private bool m_Deserializing;

        internal ResizableArray<SubtreeNodesReference> SubtreeNodesReferences { get => m_SubtreeNodesReference; set => m_SubtreeNodesReference = value; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BehaviorTreeData()
        {
            m_UniqueID = Guid.NewGuid().GetHashCode();
        }

        /// <summary>
        /// Adds the specified node.
        /// </summary>
        /// <param name="node">The node that should be added.</param>
        public void AddNode(ILogicNode node)
        {
            if (m_Tasks == null) {
                m_Tasks = new ILogicNode[1];
            } else {
                Array.Resize(ref m_Tasks, m_Tasks.Length + 1);
            }
            node.Index = (ushort)(m_Tasks.Length - 1);
            node.ParentIndex = ushort.MaxValue;
            node.SiblingIndex = ushort.MaxValue;
            node.RuntimeIndex = ushort.MaxValue;
            m_Tasks[m_Tasks.Length - 1] = node;
        }

        /// <summary>
        /// Removes the specified logic node.
        /// </summary>
        /// <param name="node">The node that should be removed.</param>
        /// <returns>True if the node was removed.</returns>
        public bool RemoveNode(ILogicNode node)
        {
            if (m_Tasks == null || node.Index >= m_Tasks.Length) {
                return false;
            }

            var dest = new ILogicNode[m_Tasks.Length - 1];
            Array.Copy(m_Tasks, dest, node.Index);
            Array.Copy(m_Tasks, node.Index + 1, dest, node.Index, m_Tasks.Length - node.Index - 1);
            m_Tasks = dest;
            return true;
        }

        /// <summary>
        /// Adds the specified event node.
        /// </summary>
        /// <param name="eventNode">The event node that should be added.</param>
        public void AddNode(IEventNode eventNode)
        {
            if (m_EventTasks == null) {
                m_EventTasks = new IEventNode[1];
            } else {
                Array.Resize(ref m_EventTasks, m_EventTasks.Length + 1);
            }
            m_EventTasks[m_EventTasks.Length - 1] = eventNode;
        }

        /// <summary>
        /// Removes the specified event node.
        /// </summary>
        /// <param name="eventNode">The event node that should be removed.</param>
        /// <returns>True if the event node was removed.</returns>
        public bool RemoveNode(IEventNode eventNode)
        {
            if (m_EventTasks == null) {
                return false;
            }

            var index = m_EventTasks.IndexOf(eventNode);
            if (index == -1) {
                return false;
            }

            var dest = new IEventNode[m_EventTasks.Length - 1];
            Array.Copy(m_EventTasks, dest, index);
            Array.Copy(m_EventTasks, index + 1, dest, index, m_EventTasks.Length - index - 1);
            m_EventTasks = dest;
            return true;
        }

        /// <summary>
        /// Serializes the behavior tree.
        /// </summary>
        public void Serialize()
        {
            m_TaskData = Serialization.Serialize<ILogicNode>(m_Tasks, ValidateSerializedObject);
            m_EventTaskData = Serialization.Serialize<IEventNode>(m_EventTasks, ValidateSerializedObject);
            SerializeSharedVariables();
            m_DisabledEventNodesData = Serialization.Serialize<ushort>(m_DisabledEventNodes);
            m_DisabledLogicNodesData = Serialization.Serialize<ushort>(m_DisabledLogicNodes);
            m_UniqueID = Guid.NewGuid().GetHashCode();

#if UNITY_EDITOR
            // Ensure the node data is up to date.
            if (m_LogicNodeProperties != null) {
                for (int i = 0; i < m_LogicNodeProperties.Length; ++i) {
                    var nodeData = m_LogicNodeProperties[i].Data;
                    nodeData.ParentIndex = m_Tasks[i].ParentIndex;
                    nodeData.SiblingIndex = m_Tasks[i].SiblingIndex;
                    nodeData.IsParent = m_Tasks[i] is IParentNode;
                    m_LogicNodeProperties[i].Data = nodeData;
                }
            }
            m_LogicNodePropertiesData = Serialization.Serialize<LogicNodeProperties>(m_LogicNodeProperties);
            m_EventNodePropertiesData = Serialization.Serialize<NodeProperties>(m_EventNodeProperties);
            m_GroupPropertiesData = Serialization.Serialize<GroupProperties>(m_GroupProperties);
#endif
        }

        /// <summary>
        /// Validates the serialized object.
        /// </summary>
        /// <param name="type">The type of object.</param>
        /// <param name="field">The field that the object belongs to.</param>
        /// <param name="value">The value of the object</param>
        /// <returns>The validated object.</returns>
        public static Serialization.ValidatedObject ValidateSerializedObject(Type type, FieldInfo field, object value)
        {
            if (value == null) {
                return new Serialization.ValidatedObject() { Type = type, Obj = value };
            }

            // Replace ILogicNode with ushort index values.
            if (typeof(IList).IsAssignableFrom(type)) {
                var elementType = Serializer.GetElementType(type);
                if (typeof(ILogicNode).IsAssignableFrom(elementType)) {
                    if (field == null || field.GetCustomAttribute<InspectNodeAttribute>() == null) {
                        var tasks = value as IList;
                        if (tasks == null) {
                            return new Serialization.ValidatedObject() { Type = type, Obj = value };
                        }

                        var indexValues = new ushort[tasks.Count];
                        for (int i = 0; i < indexValues.Length; ++i) {
                            indexValues[i] = ((ILogicNode)tasks[i]).Index;
                        }
                        return new Serialization.ValidatedObject() { Type = typeof(ushort[]), Obj = indexValues };
                    }
                } else if (Application.isPlaying && (typeof(GameObject).IsAssignableFrom(elementType) || typeof(Component).IsAssignableFrom(elementType))) { // Scene objects cannot be serialized at runtime.
                    var listValue = value as IList;
                    if (listValue != null) {
                        IList objects;
                        if (type.IsArray) {
                            objects = Array.CreateInstance(elementType, listValue.Count);
                        } else {
                            if (type.IsGenericType) {
                                objects = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                            } else {
                                objects = Activator.CreateInstance(type) as IList;
                            }
                        }
                        for (int i = 0; i < listValue.Count; ++i) {
                            GameObject gameObjectValue = null;
                            if (listValue[i] is Component componentValue) {
                                gameObjectValue = componentValue.gameObject;
                            } else {
                                gameObjectValue = listValue[i] as GameObject;
                            }
                            if (gameObjectValue != null && gameObjectValue.scene.IsValid()) {
                                if (type.IsArray) {
                                    objects[i] = null;
                                } else {
                                    objects.Add(null);
                                }
                            } else {
                                if (type.IsArray) {
                                    objects[i] = listValue[i];
                                } else {
                                    objects.Add(listValue[i]);
                                }
                            }
                        }
                        listValue = objects;
                    }
                    return new Serialization.ValidatedObject() { Type = type, Obj = listValue };
                }
            } else if (typeof(ILogicNode).IsAssignableFrom(type)) {
                if (field == null || field.GetCustomAttribute<InspectNodeAttribute>() == null) {
                    return new Serialization.ValidatedObject() { Type = typeof(ushort), Obj = ((ILogicNode)value).Index };
                }
            } else if (Application.isPlaying && (typeof(GameObject).IsAssignableFrom(type) || typeof(Component).IsAssignableFrom(type))) { // Scene objects cannot be serialized at runtime.
                GameObject gameObjectValue = null;
                if (value is Component componentValue) {
                    gameObjectValue = componentValue.gameObject;
                } else {
                    gameObjectValue = value as GameObject;
                }
                if (gameObjectValue != null && gameObjectValue.scene.IsValid()) {
                    return new Serialization.ValidatedObject() { Type = type, Obj = null };
                }
            }
            return new Serialization.ValidatedObject() { Type = type, Obj = value };
        }

        /// <summary>
        /// Serializes the SharedVariables. This allows the SharedVariables to be serialized independently.
        /// </summary>
        public void SerializeSharedVariables()
        {
            m_SharedVariableData = Serialization.Serialize<SharedVariable>(m_SharedVariables);

            // Update the mapping for any variable name changes.
            if (m_VariableByNameMap == null) {
                m_VariableByNameMap = new Dictionary<VariableAssignment, SharedVariable>();
            } else {
                m_VariableByNameMap.Clear();
            }
            if (m_SharedVariables != null) {
                for (int i = 0; i < m_SharedVariables.Length; ++i) {
                    m_VariableByNameMap.Add(new VariableAssignment(m_SharedVariables[i].Name, m_SharedVariables[i].Scope), m_SharedVariables[i]);
                }
            }
        }

        /// <summary>
        /// Internal data structure for restoring a task reference after it has been deserialized.
        /// </summary>
        public struct TaskAssignment
        {
            [Tooltip("The field of the task.")]
            public FieldInfo Field;
            [Tooltip("The task that the field belongs to.")]
            public object Target;
            [Tooltip("The value of the field. This will be the task object that should be assigned after the tree has been loaded.")]
            public object Value;
        }

        /// <summary>
        /// Deserialize the behavior tree.
        /// </summary>
        /// <param name="graph">The graph that is being deserialized.</param>
        /// <param name="force">Should the behavior tree be force deserialized?</param>
        /// <param name="variableForce">Should the shared variables be force deserialized?</param>
        /// <param name="injectSubtrees">Should the subtrees be injected into the behavior tree?</param>
        /// <param name="sharedVariableOverrides">A list of SharedVariables that should override the current SharedVariable value.</param>
        /// <returns>True if the tree was deserialized.</returns>
        public bool Deserialize(IGraph graph, bool force, bool variableForce, bool injectSubtrees, SharedVariableOverride[] sharedVariableOverrides = null)
        {
            // No need to deserialize if the data is already deserialized.
            if ((m_Tasks != null || m_EventTasks != null || m_SharedVariables != null) && !force) {
                if (Application.isPlaying && m_RuntimeUniqueID == 0) {
                    m_RuntimeUniqueID = m_UniqueID;
                }
                return true;
            }

            return DeserializeInternal(graph, force, variableForce, injectSubtrees, sharedVariableOverrides);
        }

        /// <summary>
        /// Internal method which deserialize the behavior tree.
        /// </summary>
        /// <param name="graph">The graph that is being deserialized.</param>
        /// <param name="force">Should the behavior tree be force deserialized?</param>
        /// <param name="variableForce">Should the shared variables be force deserialized?</param>
        /// <param name="injectSubtrees">Should the subtrees be injected into the behavior tree?</param>
        /// <param name="sharedVariableOverrides">A list of SharedVariables that should override the current SharedVariable value.</param>
        /// <returns>True if the tree was deserialized.</returns>
        private bool DeserializeInternal(IGraph graph, bool force, bool variableForce, bool injectSubtrees, SharedVariableOverride[] sharedVariableOverrides = null)
        {
            // Prevent the tree from being deserialized recusrively.
            if (m_Deserializing) {
                Debug.LogError($"Error: Unable to deserialize {graph}. This can be caused by recursive subtree references.");
                return false;
            }

            m_Deserializing = true;
            m_RuntimeUniqueID = Application.isPlaying ? m_UniqueID : 0;
            var errorState = false;
#if UNITY_EDITOR
            // Deserialize the properties first so it can be used elsewhere.
            if (m_LogicNodePropertiesData != null && m_LogicNodePropertiesData.Length > 0) {
                m_LogicNodeProperties = new LogicNodeProperties[m_LogicNodePropertiesData.Length];
                for (int i = 0; i < m_LogicNodePropertiesData.Length; ++i) {
                    m_LogicNodeProperties[i] = m_LogicNodePropertiesData[i].DeserializeFields(MemberVisibility.Public) as LogicNodeProperties;
                }
            }
            if (m_EventNodePropertiesData != null && m_EventNodePropertiesData.Length > 0) {
                m_EventNodeProperties = new NodeProperties[m_EventNodePropertiesData.Length];
                for (int i = 0; i < m_EventNodePropertiesData.Length; ++i) {
                    m_EventNodeProperties[i] = m_EventNodePropertiesData[i].DeserializeFields(MemberVisibility.Public) as NodeProperties;
                }
            }
            if (m_GroupPropertiesData != null && m_GroupPropertiesData.Length > 0) {
                m_GroupProperties = new GroupProperties[m_GroupPropertiesData.Length];
                for (int i = 0; i < m_GroupPropertiesData.Length; ++i) {
                    m_GroupProperties[i] = m_GroupPropertiesData[i].DeserializeFields(MemberVisibility.Public) as GroupProperties;
                }
            }
#endif
            DeserializeSharedVariables(variableForce, sharedVariableOverrides);
            PopulateSharedVariablesMapping(graph, ref m_VariableByNameMap);

            // The disabled node indicies need to be deserialized before the nodes.
            if (m_DisabledEventNodesData != null && m_DisabledEventNodesData.Length > 0 && m_EventTaskData != null) {
                m_DisabledEventNodes = new ushort[m_DisabledEventNodesData.Length];
                var offset = 0;
                for (int i = 0; i < m_DisabledEventNodesData.Length; ++i) {
                    m_DisabledEventNodes[i] = (ushort)m_DisabledEventNodesData[i].DeserializeFields(MemberVisibility.Public);
                    // The node index may no longer be valid.
                    if (m_DisabledEventNodes[i - offset] >= m_EventTaskData.Length) {
                        offset++;
                    }
                }
                if (offset > 0) {
                    Array.Resize(ref m_DisabledEventNodes, m_DisabledEventNodes.Length - offset);
                }
            }
            if (m_DisabledLogicNodesData != null && m_DisabledLogicNodesData.Length > 0 && m_TaskData != null) {
                m_DisabledLogicNodes = new ushort[m_DisabledLogicNodesData.Length];
                var offset = 0;
                for (int i = 0; i < m_DisabledLogicNodesData.Length; ++i) {
                    m_DisabledLogicNodes[i - offset] = (ushort)m_DisabledLogicNodesData[i].DeserializeFields(MemberVisibility.Public);
                    // The node index may no longer be valid.
                    if (m_DisabledLogicNodes[i - offset] >= m_TaskData.Length) {
                        offset++;
                    }
                }
                if (offset > 0) {
                    Array.Resize(ref m_DisabledLogicNodes, m_DisabledLogicNodes.Length - offset);
                }
            }

            ResizableArray<TaskAssignment> taskReferences = null;
            if (m_SubtreeNodesReference != null) {
                m_SubtreeNodesReference.Clear();
            }
            if (m_TaskData != null && m_TaskData.Length > 0) {
                m_Tasks = new ILogicNode[m_TaskData.Length];
                for (int i = 0; i < m_TaskData.Length; ++i) {
                    m_Tasks[i] = m_TaskData[i].DeserializeFields(MemberVisibility.Public, ValidateDeserializedTypeObject, (object fieldInfoObj, object task, object value) =>
                    {
                        return ValidateDeserializedObject(fieldInfoObj, task, value, ref m_VariableByNameMap, ref taskReferences, sharedVariableOverrides);
                    }) as ILogicNode;

                    // Account for tasks where the object no longer exists.
                    if (m_Tasks[i] == null) {
#if UNITY_EDITOR
                        if (m_LogicNodeProperties[i].Data.IsParent) {
                            m_Tasks[i] = new UnknownParentTaskNode(m_TaskData[i].ObjectType);
                        } else {
                            m_Tasks[i] = new UnknownTaskNode(m_TaskData[i].ObjectType);
                        }
                        m_Tasks[i].Index = (ushort)i;
                        m_Tasks[i].ParentIndex = m_LogicNodeProperties[i].Data.ParentIndex;
                        m_Tasks[i].SiblingIndex = m_LogicNodeProperties[i].Data.SiblingIndex;
#else
                        if (i + 1 < m_Tasks.Length && m_Tasks[i + 1] != null && m_Tasks[i + 1].ParentIndex == i) {
                            m_Tasks[i] = new UnknownParentTaskNode(m_TaskData[i].ObjectType);
                        } else {
                            m_Tasks[i] = new UnknownTaskNode(m_TaskData[i].ObjectType);
                        }
                        m_Tasks[i].Index = (ushort)i;
#endif
                    }

                    // The RuntimeIndex is assigned later when the tree is initialized.
                    m_Tasks[i].RuntimeIndex = ushort.MaxValue;

                    if (injectSubtrees) {
                        // If the previous task is a parent the current task has to be a child otherwise the tree is in an error state. The error will also occur
                        // if there is only one task and that task is a parent task.
                        if (m_Tasks[i].ParentIndex != ushort.MaxValue && ((i > 0 && m_Tasks[i - 1] is IParentNode && m_Tasks[i].ParentIndex != m_Tasks[i - 1].Index) || (m_Tasks[i] is IParentNode && i + 1 == m_Tasks.Length))) {
                            Debug.LogError($"Error: {graph} contains the parent task {m_Tasks[i].GetType().Name} which does not have any children. All parent tasks must contain at least one child.", graph.Parent);
                            errorState = true;
                            continue;
                        }

                        // Subtrees will be evaluated after all tasks are assigned.
                        if (m_Tasks[i] is ISubtreeReference subtreeReference) {
                            // Subtrees can be nested.
                            subtreeReference.EvaluateSubtrees();
                            var subtrees = subtreeReference.Subtrees;
                            if (subtrees != null) {
                                // The parent must be able to accept the number of subtrees that there are.
                                var parentIndex = m_Tasks[i].ParentIndex;
                                IParentNode parentNode = null;
                                if (parentIndex != ushort.MaxValue) {
                                    parentNode = m_Tasks[parentIndex] as IParentNode;
                                }

                                if ((parentNode == null && subtrees.Length > 1) || (parentNode != null && subtrees.Length > parentNode.MaxChildCount)) {
                                    Debug.LogError($"Error: {graph} on object {graph.Parent} contains multiple subtrees as the starting task or as a child of a parent task which cannot contain so many children (such as a decorator).", graph.Parent);
                                    errorState = true;
                                    continue;
                                }

                                var deserializedNodes = new ILogicNode[subtrees.Length][];
                                for (int j = 0; j < subtrees.Length; ++j) {
                                    if (subtrees[j] == null) {
                                        continue;
                                    }
                                    if (!subtrees[j].Deserialize(force, true, subtreeReference.SharedVariableOverrides)) {
                                        errorState = true;
                                        break;
                                    };
                                    // Keep a reference to the deserialized nodes. This will ensure they are unique and do not get overwritten.
                                    deserializedNodes[j] = subtrees[j].LogicNodes;

                                    // Add any new subtree variables to the current tree.
                                    if (subtrees[j].SharedVariables != null) {
                                        // In order to reduce allocations the first loop will determine the number of variables that need to be added.
                                        var length = subtrees[j].SharedVariables.Length;
                                        var variableCount = 0;
                                        for (int k = 0; k < length; ++k) {
                                            var subtreeVariable = subtrees[j].SharedVariables[k];
                                            if (GetVariable(graph, subtreeVariable.Name, SharedVariable.SharingScope.Graph) == null) {
                                                variableCount++;
                                            }
                                        }

                                        // And the second loop will actually add the variables.
                                        if (variableCount > 0) {
                                            var insertIndex = 0;
                                            if (m_SharedVariables == null) {
                                                m_SharedVariables = new SharedVariable[variableCount];
                                            } else {
                                                insertIndex = m_SharedVariables.Length;
                                                Array.Resize(ref m_SharedVariables, m_SharedVariables.Length + variableCount);
                                            }
                                            for (int k = 0; k < length; ++k) {
                                                var subtreeVariable = subtrees[j].SharedVariables[k];
                                                if (!m_VariableByNameMap.ContainsKey(new VariableAssignment(subtreeVariable.Name, SharedVariable.SharingScope.Graph))) {
                                                    m_SharedVariables[insertIndex] = subtreeVariable;
                                                    m_VariableByNameMap.Add(new VariableAssignment(subtreeVariable.Name, SharedVariable.SharingScope.Graph), subtreeVariable);
                                                    insertIndex++;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Do not add the subtree if it causes an error.
                                if (!errorState) {
                                    if (m_SubtreeNodesReference == null) { m_SubtreeNodesReference = new ResizableArray<SubtreeNodesReference>(); }
                                    m_SubtreeNodesReference.Add(new SubtreeNodesReference()
                                    {
                                        SubtreeReference = subtreeReference,
                                        NodeIndex = (ushort)i,
                                        Subtrees = subtrees,
                                        Nodes = deserializedNodes
                                    });
                                }
                            }
                        }
                    }
                }
            } else {
                m_Tasks = null;
            }

            // Subtrees should be injected into the tree.
            InjectSubtrees();

            // Add the event tasks after the subtrees have been injected to ensure the connected index is correct.
            if (m_EventTaskData != null && m_EventTaskData.Length > 0) {
                m_EventTasks = new IEventNode[m_EventTaskData.Length];
                for (int i = 0; i < m_EventTaskData.Length; ++i) {
                    m_EventTasks[i] = m_EventTaskData[i].DeserializeFields(MemberVisibility.Public, ValidateDeserializedTypeObject, (object fieldInfoObj, object task, object value) =>
                    {
                        return ValidateDeserializedObject(fieldInfoObj, task, value, ref m_VariableByNameMap, ref taskReferences);
                    }) as IEventNode;

                    if (m_SubtreeNodesReference != null) {
                        // A subtree may have injected nodes before the originally connected index. Modify the index to match the injection.
                        var offset = 0;
                        for (int j = 0; j < m_SubtreeNodesReference.Count; ++j) {
                            if (m_SubtreeNodesReference[j].NodeIndex >= m_EventTasks[i].ConnectedIndex) {
                                break;
                            }
                            offset += m_SubtreeNodesReference[j].NodeCount - 1;
                        }
                        if (offset != 0) {
                            m_EventTasks[i].ConnectedIndex += (ushort)offset;
                        }
                    }

                    if (m_EventTasks[i] == null) {
                        m_EventTasks[i] = new UnknownEventTask();
                    }
                }
            } else {
                m_EventTasks = null;
            }

            // After the tree has been deserialized the task references need to be assigned.
            AssignTaskReferences(m_Tasks, taskReferences);

            m_Deserializing = false;

            return !errorState;
        }

        /// <summary>
        /// Validates the object type when deserializing.
        /// </summary>
        /// <param name="type">The type of object that should be validated.</param>
        /// <param name="field">The field that contains the object.</param>
        /// <returns>The validated type.</returns>
        public static Type ValidateDeserializedTypeObject(Type type, FieldInfo field)
        {
            if (typeof(IList).IsAssignableFrom(type)) {
                var elementType = Serializer.GetElementType(type);
                if (typeof(ILogicNode).IsAssignableFrom(elementType) && (field == null || field.GetCustomAttribute<InspectNodeAttribute>() == null)) {
                    return typeof(ushort[]);
                }
            } else if (typeof(ILogicNode).IsAssignableFrom(type) && (field == null || field.GetCustomAttribute<InspectNodeAttribute>() == null)) {
                return typeof(ushort);
            }
            return type;
        }

        /// <summary>
        /// Validates the object when deserializing.
        /// </summary>
        /// <param name="fieldInfoObj">The FieldInfo that is being deserialized.</param>
        /// <param name="target">The object being deserialized.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        /// <param name="taskReferences">A reference to the list of task references that need to be resolved later.</param>
        /// <param name="sharedVariableOverrides">A list of SharedVariables that should override the current SharedVariable value.</param>
        /// <returns>The validated object.</returns>
        public static object ValidateDeserializedObject(object fieldInfoObj, object target, object value, ref Dictionary<VariableAssignment, SharedVariable> variableByNameMap,
                                    ref ResizableArray<TaskAssignment> taskReferences, SharedVariableOverride[] sharedVariableOverrides = null)
        {
            var fieldInfo = fieldInfoObj as FieldInfo;
            if (fieldInfo == null) {
                return value;
            }

            var type = fieldInfo.FieldType;
            if (value == null) {
                // A SharedVariable object should always exist.
                if (!type.IsAbstract && typeof(SharedVariable).IsAssignableFrom(type)) {
                    return Activator.CreateInstance(type);
                }
                return null;
            }

            if (typeof(IList).IsAssignableFrom(type)) {
                var elementType = Serializer.GetElementType(type);
                if (typeof(ILogicNode).IsAssignableFrom(elementType) && fieldInfo.GetCustomAttribute<InspectNodeAttribute>() == null) {
                    // The task reference will be assigned after all of the tasks have been deserialized.
                    if (taskReferences == null) { taskReferences = new ResizableArray<TaskAssignment>(); }
                    taskReferences.Add(new TaskAssignment() { Field = fieldInfo, Target = target, Value = value });
                } else if (typeof(SharedVariable).IsAssignableFrom(elementType)) {
                    var listValue = value as IList;
                    if (listValue != null) {
                        for (int i = 0; i < listValue.Count; ++i) {
                            var sharedVariableElement = listValue[i] as SharedVariable;
                            if (variableByNameMap != null && sharedVariableElement != null && !string.IsNullOrEmpty(sharedVariableElement.Name)) {
                                if (variableByNameMap.TryGetValue(new VariableAssignment(sharedVariableElement.Name, sharedVariableElement.Scope), out var mappedSharedVariable)) {
                                    if (Application.isPlaying && sharedVariableElement.Scope == SharedVariable.SharingScope.Dynamic && sharedVariableElement.GetType() != mappedSharedVariable.GetType()) {
                                        Debug.LogError($"Error: The dynamic variables with name {sharedVariableElement.Name} have different types. All dynamic variables must have the same type.");
                                        listValue[i] = sharedVariableElement;
                                    } else {
                                        listValue[i] = GetOverrideVariable(sharedVariableOverrides, mappedSharedVariable, false);
                                    }
                                } else if (sharedVariableElement.Scope == SharedVariable.SharingScope.Dynamic) {
                                    // New dynamic variables should have the default value.
                                    var sharedVariableValueType = sharedVariableElement.GetType().GetGenericArguments()[0];
                                    if (sharedVariableValueType.IsValueType) {
                                        sharedVariableElement.SetValue(Activator.CreateInstance(sharedVariableValueType));
                                    } else {
                                        sharedVariableElement.SetValue(null);
                                    }

                                    // Dynamic variables are created when the task is deserialized. The variable needs to be added to the mapping so it can be reused.
                                    variableByNameMap.Add(new VariableAssignment(sharedVariableElement.Name, sharedVariableElement.Scope), sharedVariableElement);
                                    listValue[i] = sharedVariableElement;
                                }
                            }

                        }
                        return listValue;
                    }
                }
            } else if (typeof(ILogicNode).IsAssignableFrom(type) && fieldInfo.GetCustomAttribute<InspectNodeAttribute>() == null) {
                // The task reference will be assigned after all of the tasks have been deserialized.
                if (taskReferences == null) { taskReferences = new ResizableArray<TaskAssignment>(); }
                taskReferences.Add(new TaskAssignment() { Field = fieldInfo, Target = target, Value = value });
            } else if (typeof(SharedVariable).IsAssignableFrom(type)) {
                var sharedVariable = value as SharedVariable;
                if (variableByNameMap != null && sharedVariable != null && !string.IsNullOrEmpty(sharedVariable.Name)) {
                    if (variableByNameMap.TryGetValue(new VariableAssignment(sharedVariable.Name, sharedVariable.Scope), out var mappedSharedVariable)) {
                        if (Application.isPlaying && sharedVariable.Scope == SharedVariable.SharingScope.Dynamic && sharedVariable.GetType() != mappedSharedVariable.GetType()) {
                            Debug.LogError($"Error: The dynamic variables with name {sharedVariable.Name} have different types. Dynamic variables with the same name must have the same type.");
                            return sharedVariable;
                        }
                        return GetOverrideVariable(sharedVariableOverrides, mappedSharedVariable, false);
                    } else if (Application.isPlaying && sharedVariable.Scope == SharedVariable.SharingScope.Dynamic) {
                        // New dynamic variables should have the default value.
                        var sharedVariableValueType = sharedVariable.GetType().GetGenericArguments()[0];
                        if (sharedVariableValueType.IsValueType) {
                            sharedVariable.SetValue(Activator.CreateInstance(sharedVariableValueType));
                        } else {
                            sharedVariable.SetValue(null);
                        }

                        // Dynamic variables are created when the task is deserialized. The variable needs to be added to the mapping so it can be reused.
                        variableByNameMap.Add(new VariableAssignment(sharedVariable.Name, sharedVariable.Scope), sharedVariable);
                        return sharedVariable;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Deserializes the SharedVariables. This allows the SharedVariables to be deserialized independently.
        /// </summary>
        /// <param name="force">Should the variables be forced deserialized?</param>
        /// <param name="sharedVariableOverrides">A list of SharedVariables that should override the current SharedVariable value.</param>
        /// <returns>True if the SharedVariables were deserialized.</returns>
        public bool DeserializeSharedVariables(bool force, SharedVariableOverride[] sharedVariableOverrides = null)
        {
            // No need to deserialize if the data is already deserialized.
            if (m_SharedVariables != null && !force) {
                return false;
            }

            if (m_SharedVariableData != null && m_SharedVariableData.Length > 0) {
                m_SharedVariables = new SharedVariable[m_SharedVariableData.Length];
                for (int i = 0; i < m_SharedVariableData.Length; ++i) {
                    m_SharedVariables[i] = m_SharedVariableData[i].DeserializeFields(MemberVisibility.Public) as SharedVariable;
                    if (m_SharedVariables[i] == null) {
                        Debug.LogError($"Error: Unable to deserialize SharedVariable of type {m_SharedVariableData[i].ObjectType}.");
                        continue;
                    }

                    // The override variable can set a value specific for the subtree.
                    if (Application.isPlaying) {
                        m_SharedVariables[i].Initialize();

                        var overrideVariable = GetOverrideVariable(sharedVariableOverrides, m_SharedVariables[i], true);
                        // If the overridden scope is self then only the value should be overridden and not the SharedVariable reference.
                        if (overrideVariable != null && overrideVariable.Scope == SharedVariable.SharingScope.Self) {
                            m_SharedVariables[i].SetValue(overrideVariable.GetValue());
                        }
                    }
                }
            } else {
                m_SharedVariables = null;
            }

            return true;
        }

        /// <summary>
        /// Returns the override SharedVariable from the source SharedVariable.
        /// </summary>
        /// <param name="sharedVariableOverrides">The list of override SharedVariables.</param>
        /// <param name="graphVariable">The variable that should be overridden.</param>
        /// <param name="deserialize">Is the method being called when the variables are being deserialized?</param>
        /// <returns>The override SharedVariable (can be null).</returns>
        private static SharedVariable GetOverrideVariable(SharedVariableOverride[] sharedVariableOverrides, SharedVariable graphVariable, bool deserialize)
        {
            if (sharedVariableOverrides == null) {
                return deserialize ? null : graphVariable;
            }

            for (int i = 0; i < sharedVariableOverrides.Length; ++i) {
                var overrideVariable = sharedVariableOverrides[i].Override;
                // Empty variables indicate that the variable should not be overridden.
                if (overrideVariable == null || overrideVariable.Scope == SharedVariable.SharingScope.Empty || (!deserialize && overrideVariable.Scope == SharedVariable.SharingScope.Self)) {
                    continue;
                }

                // The override variable should be used if the name and the type matches.
                var sourceVariable = sharedVariableOverrides[i].Source;
                if (sourceVariable.GetType() != graphVariable.GetType() || sourceVariable.Name != graphVariable.Name) {
                    continue;
                }

                return overrideVariable;
            }

            return graphVariable;
        }

        /// <summary>
        /// Internal data structure for referencing a SharedVariable to its name/scope.
        /// </summary>
        public struct VariableAssignment
        {
            [Tooltip("The name of the SharedVariable.")]
            public string Name;
            [Tooltip("The scope of the SharedVariable.")]
            public SharedVariable.SharingScope Scope;

            /// <summary>
            /// VariableAssignment constructor.
            /// </summary>
            /// <param name="name">The name of the SharedVariable.</param>
            /// <param name="scope">The scope of the SharedVariable.</param>
            public VariableAssignment(string name, SharedVariable.SharingScope scope)
            {
                Name = name;
                Scope = scope;
            }
        }

        /// <summary>
        /// Populates the SharedVariable Mapping at runtime.
        /// </summary>
        /// <param name="graph">The graph that is being deserialized.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        public static void PopulateSharedVariablesMapping(IGraph graph, ref Dictionary<VariableAssignment, SharedVariable> variableByNameMap)
        {
            if (variableByNameMap != null) {
                variableByNameMap.Clear();
            }
            PopulateSharedVariablesMapping(graph.SharedVariables, SharedVariable.SharingScope.Graph, ref variableByNameMap);

            if (graph.Parent is GameObject parentGameObject) {
                var gameObjectSharedVariablesContainer = parentGameObject.GetComponent<GameObjectSharedVariables>();
                if (gameObjectSharedVariablesContainer != null) {
                    gameObjectSharedVariablesContainer.Deserialize(false);
                    PopulateSharedVariablesMapping(gameObjectSharedVariablesContainer.SharedVariables, SharedVariable.SharingScope.GameObject, ref variableByNameMap);
                }
            }

            var sceneSharedVariablesContainer = SceneSharedVariables.Instance;
            if (sceneSharedVariablesContainer != null) {
                sceneSharedVariablesContainer.Deserialize(false);
                PopulateSharedVariablesMapping(sceneSharedVariablesContainer.SharedVariables, SharedVariable.SharingScope.Scene, ref variableByNameMap);
            }

            var projectSharedVariablesContainer = ProjectSharedVariables.Instance;
            if (projectSharedVariablesContainer != null) {
                projectSharedVariablesContainer.Deserialize(false);
                PopulateSharedVariablesMapping(projectSharedVariablesContainer.SharedVariables, SharedVariable.SharingScope.Project, ref variableByNameMap);
            }
        }

        /// <summary>
        /// Populates the name variables mapping with the specified SharedVariables.
        /// </summary>
        /// <param name="sharedVariables">The SharedVariables that should be populated.</param>
        /// <param name="scope">The scope of SharedVariables.</param>
        /// <param name="variableByNameMap">A reference to the map between the VariableAssignment and SharedVariable.</param>
        private static void PopulateSharedVariablesMapping(SharedVariable[] sharedVariables, SharedVariable.SharingScope scope, ref Dictionary<VariableAssignment, SharedVariable> variableByNameMap)
        {
            if (sharedVariables == null) {
                return;
            }

            if (variableByNameMap == null) { variableByNameMap = new Dictionary<VariableAssignment, SharedVariable>(); }
            for (int i = 0; i < sharedVariables.Length; ++i) {
                if (variableByNameMap.ContainsKey(new VariableAssignment(sharedVariables[i].Name, scope))) {
#if UNITY_EDITOR
                    Debug.LogWarning("Warning: Multiple SharedVariables with the same name have been added. Please email support@opsive.com with the steps to reproduce this warning. Thank you.");
#endif
                    continue;
                }
                variableByNameMap.Add(new VariableAssignment(sharedVariables[i].Name, scope), sharedVariables[i]);
            }
        }

        /// <summary>
        /// When the behavior tree loads not all tasks will be deserialized instantly. TaskA may reference TaskB but TaskB hasn't
        /// been deserialized yet. The TaskAssignment data structure will store all of the references that need to be restored after
        /// the behavior tree has fully been deserialized.
        /// </summary>
        /// <param name="tasks">The tasks that belong to the graph.</param>
        /// <param name="taskReferences">The tasks that should be referenced.</param>
        public static void AssignTaskReferences(ILogicNode[] tasks, ResizableArray<TaskAssignment> taskReferences)
        {
            if (taskReferences == null) {
                return;
            }

            for (int i = 0; i < taskReferences.Count; ++i) {
                var taskReference = taskReferences[i];
                var fieldType = taskReference.Field.FieldType;
                object value = null;

                // The field can be a list or single value.
                if (typeof(IList).IsAssignableFrom(fieldType)) {
                    var elements = (IList)taskReferences[i].Value;
                    if (fieldType.IsArray) {
                        // The field type is an array. Create a new array with all of the task instances.
                        var array = Array.CreateInstance(Serializer.GetElementType(fieldType), elements.Count) as ILogicNode[];
                        for (int j = 0; j < array.Length; ++j) {
                            var index = (ushort)elements[j];
                            if (index < tasks.Length) {
                                array[j] = tasks[index];
                            }
                        }
                        value = array;
                    } else {
                        // The field type is a list. Create a new list with all of the task instances.
                        IList taskList;
                        if (fieldType.IsGenericType) {
                            taskList = Activator.CreateInstance(typeof(List<>).MakeGenericType(Serializer.GetElementType(fieldType))) as IList;
                        } else {
                            taskList = Activator.CreateInstance(fieldType) as IList;
                        }

                        for (int j = 0; j < elements.Count; ++j) {
                            var index = (ushort)elements[j];
                            if (index < tasks.Length) {
                                taskList.Add(tasks[index]);
                            }
                        }
                        value = taskList;
                    }
                } else { // Single ILogicNode value.
                    var index = (ushort)taskReference.Value;
                    if (index < tasks.Length) {
                        value = tasks[index];
                    }
                }
                if (value != null) {
                    taskReference.Field.SetValue(taskReference.Target, value);
                }
            }
        }

        /// <summary>
        /// Contains a reference to the subtree index and nodes.
        /// </summary>
        internal struct SubtreeNodesReference
        {
            [Tooltip("The ISubtreeReference.")]
            public ISubtreeReference SubtreeReference;
            [Tooltip("The index of the ISubtreeReference.")]
            public ushort NodeIndex;
            [Tooltip("The total number of nodes contained within the ISubtreeReference.")]
            public ushort NodeCount;
            [Tooltip("A reference to the subtrees that are loaded.")]
            public Subtree[] Subtrees;
            [Tooltip("The deserialized nodes.")]
            public ILogicNode[][] Nodes;
        }

        /// <summary>
        /// Data structure which contains the properties for a subtree that will be injected.
        /// </summary>
        private struct SubtreeAssignment
        {
            [Tooltip("The index of the SubtreeNodesReference element.")]
            public int ReferenceIndex;
            [Tooltip("The index of the ISubtreeReference task.")]
            public ushort NodeIndex;
            [Tooltip("The index of the Subtree.")]
            public int SubtreeIndex;
            [Tooltip("The subtree that the task references.")]
            public Subtree Subtree;
            [Tooltip("The offset of the index. This will change as subtrees are added.")]
            public ushort IndexOffset;
            [Tooltip("The original parent index of the ISubtreeReference task.")]
            public ushort ParentIndex;
            [Tooltip("The original sibling index of the ISubtreeReference task.")]
            public ushort SiblingIndex;
            [Tooltip("The number of nodes that are a child of the ISubtreeReference.")]
            public ushort NodeCount;
#if UNITY_EDITOR
            [Tooltip("The position of the ISubtreeReference task.")]
            public Vector2 NodePropertiesPosition;
            [Tooltip("Is the ISubtreeReference task collapsed?")]
            public bool Collapsed;
#endif
        }

        /// <summary>
        /// Injects the subtree into the task list.
        /// </summary>
        private void InjectSubtrees()
        {
            if (m_SubtreeNodesReference == null || m_SubtreeNodesReference.Count == 0) {
                return;
            }

            // The behavior tree must generate a new ID when subtrees are injected.
            m_RuntimeUniqueID = Guid.NewGuid().GetHashCode();

            var taskCount = 0;
            var subtreeReferenceCount = 0;
            var subtreeAssignments = new ResizableArray<SubtreeAssignment>();
            var lastParentIndex = m_Tasks[m_SubtreeNodesReference[0].NodeIndex].ParentIndex;
            var parentIndexOffset = 0;
            for (int i = 0; i < m_SubtreeNodesReference.Count; ++i) {
                var subtreeReference = m_Tasks[m_SubtreeNodesReference[i].NodeIndex] as ISubtreeReference;
                var subtrees = subtreeReference.Subtrees;
                if (subtrees != null) {
                    var indexOffset = (ushort)0; // The index offset is relative to each individual ISubtreeReference task.

                    // The parent index will change based on the number of tasks that have been added.
                    var parentIndex = m_Tasks[m_SubtreeNodesReference[i].NodeIndex].ParentIndex;
                    if (parentIndex > lastParentIndex) {
                        parentIndexOffset = (ushort)(taskCount - subtreeReferenceCount);
                        lastParentIndex = parentIndex;
                    }

                    // Calculate the parent index offset based on previously injected subtrees
                    for (int j = 0; j < subtrees.Length; ++j) {
                        if (subtrees[j] == null || subtrees[j].LogicNodes == null || subtrees[j].EventNodes == null) {
                            continue;
                        }

                        // The subtree should start from the start node.
                        var startNode = subtrees[j].GetEventNode(typeof(Start)); // Returns (IEventNode, index).
                        if (startNode.Item1 == null || startNode.Item1.ConnectedIndex == ushort.MaxValue || !subtrees[j].IsNodeEnabled(false, startNode.Item2)) {
                            continue;
                        }

                        var firstNode = m_SubtreeNodesReference[i].Nodes[j][startNode.Item1.ConnectedIndex];
                        var subtreeNodeCount = GetChildCount(firstNode, m_SubtreeNodesReference[i].Nodes[j]) + 1; // firstNode should be included in addition to the children.
                        taskCount += subtreeNodeCount;
                        subtreeAssignments.Add(new SubtreeAssignment()
                        {
                            ReferenceIndex = i,
                            NodeIndex = m_SubtreeNodesReference[i].NodeIndex,
                            SubtreeIndex = j,
                            Subtree = subtrees[j],
                            NodeCount = (ushort)subtreeNodeCount,
                            IndexOffset = indexOffset,
                            ParentIndex = (ushort)(parentIndex + parentIndexOffset),
                            SiblingIndex = m_Tasks[m_SubtreeNodesReference[i].NodeIndex].SiblingIndex,
#if UNITY_EDITOR
                            NodePropertiesPosition = m_LogicNodeProperties[m_SubtreeNodesReference[i].NodeIndex].Position,
                            Collapsed = m_LogicNodeProperties[m_SubtreeNodesReference[i].NodeIndex].Collapsed
#endif
                        });
                        indexOffset += (ushort)subtreeNodeCount;
                    }

                    // Update the parent index offset for the next subtree reference
                    if (indexOffset > 0) { // Subtree References may not contain any valid subtrees.
                        subtreeReferenceCount++;
                    }
                    var subtreeNodesReferenceOrig = m_SubtreeNodesReference[i];
                    subtreeNodesReferenceOrig.NodeCount = indexOffset;
                    m_SubtreeNodesReference[i] = subtreeNodesReferenceOrig;
                }
            }

            if (taskCount == 0) {
                return;
            }

            var targetCount = m_Tasks.Length + taskCount - subtreeReferenceCount;
            var originalTaskCount = m_Tasks.Length;
            if (m_Tasks.Length != targetCount) {
                Array.Resize(ref m_Tasks, targetCount);
#if UNITY_EDITOR
                Array.Resize(ref m_LogicNodeProperties, targetCount);
#endif
            }

            // Make space for all of the subtree tasks.
            var addedTasks = 0;
            for (int i = 0; i < subtreeAssignments.Count; ++i) {
                var subtreeIndex = (ushort)(subtreeAssignments[i].NodeIndex + addedTasks);
                var subtreeTaskCount = subtreeAssignments[i].NodeCount - (subtreeAssignments[i].IndexOffset == 0 ? 1 : 0);
                if (subtreeTaskCount > 0) { // subtreeTaskCount will be zero if a single task replaces the reference task.
                    for (int j = originalTaskCount - 1 + addedTasks; j > subtreeIndex; --j) {
                        var node = m_Tasks[j];
                        node.Index += (ushort)subtreeTaskCount;
                        if (node.ParentIndex > subtreeIndex && node.ParentIndex != ushort.MaxValue) {
                            node.ParentIndex += (ushort)subtreeTaskCount;
                        }
                        if (node.SiblingIndex > subtreeIndex && node.SiblingIndex != ushort.MaxValue) {
                            node.SiblingIndex += (ushort)subtreeTaskCount;
                        }
                        m_Tasks[j + subtreeTaskCount] = node;
                        m_Tasks[j] = null;
#if UNITY_EDITOR
                        m_LogicNodeProperties[j + subtreeTaskCount] = m_LogicNodeProperties[j];
#endif
                    }

                    // The parents need to adjust their sibling index offsets for the newly added nodes. This should only be done with an index offset of 0
                    // as grouped subtrees have the same parents.
                    if (subtreeAssignments[i].IndexOffset == 0) {
                        var parentIndex = m_Tasks[subtreeIndex].ParentIndex;
                        while (parentIndex != ushort.MaxValue) {
                            var parentNode = m_Tasks[parentIndex];
                            if (parentNode.SiblingIndex != ushort.MaxValue) {
                                parentNode.SiblingIndex += (ushort)subtreeTaskCount;
                                m_Tasks[parentIndex] = parentNode;
                            }
                            parentIndex = parentNode.ParentIndex;
                        }
                    }

                    // Any disabled nodes after the insertion needs to shift.
                    var lastDisabledNodeIndex = 0;
                    if (m_DisabledLogicNodes != null) {
                        for (int j = 0; j < m_DisabledLogicNodes.Length; ++j) {
                            if (m_DisabledLogicNodes[j] > subtreeIndex) {
                                m_DisabledLogicNodes[j] += (ushort)subtreeTaskCount;
                            } else { // Remember the last index that was greater than the subtree index so any disabled subtree nodes can be inserted.
                                lastDisabledNodeIndex = j + 1;
                            }
                        }
                    }

                    // If the parent reference task is disabled then all subtree nodes should be disabled.
                    var subtreeDisabledLogicNodes = subtreeAssignments[i].Subtree.DisabledLogicNodes;
                    if (!IsNodeEnabled(true, m_SubtreeNodesReference[subtreeAssignments[i].ReferenceIndex].NodeIndex)) {
                        subtreeDisabledLogicNodes = new ushort[subtreeAssignments[i].NodeCount];
                        for (ushort j = 0; j < subtreeDisabledLogicNodes.Length; ++j) {
                            subtreeDisabledLogicNodes[j] = j;
                        }
                    }

                    // The subtree may have disabled tasks.
                    if (subtreeDisabledLogicNodes != null && subtreeDisabledLogicNodes.Length > 0) {
                        var subtreeDisabledLength = subtreeDisabledLogicNodes.Length;
                        // Ensure all of the disabled logic nodes have been transferred.
                        for (int j = subtreeDisabledLength - 1; j >= 0; --j) {
                            if (subtreeDisabledLogicNodes[j] > subtreeTaskCount) {
                                subtreeDisabledLength--;
                            }
                        }

                        if (subtreeDisabledLength > 0) {
                            if (m_DisabledLogicNodes == null) {
                                m_DisabledLogicNodes = new ushort[subtreeDisabledLength];
                            } else {
                                Array.Resize(ref m_DisabledLogicNodes, m_DisabledLogicNodes.Length + subtreeDisabledLength);
                            }
                            var originalLength = m_DisabledLogicNodes.Length - subtreeDisabledLength;
                            for (int j = lastDisabledNodeIndex; j < originalLength; ++j) {
                                m_DisabledLogicNodes[j + subtreeDisabledLength] = m_DisabledLogicNodes[j];
                            }
                            for (int j = 0; j < subtreeDisabledLength; ++j) {
                                if (subtreeDisabledLogicNodes[j] > subtreeTaskCount) {
                                    continue;
                                }
                                m_DisabledLogicNodes[lastDisabledNodeIndex + j] = (ushort)(subtreeIndex + subtreeDisabledLogicNodes[j]);
                            }
                        }
                    }
                }
                // Tasks were added to the tree. Update the tree to the correct indicies.
                var subtreeAssignment = subtreeAssignments[i];
                subtreeAssignment.IndexOffset = (ushort)(addedTasks + (subtreeAssignments[i].IndexOffset == 0 ? 0 : 1));
                subtreeAssignments[i] = subtreeAssignment;

                addedTasks += subtreeTaskCount;
            }

            // Populate the tasks with the subtree.
            for (int i = 0; i < subtreeAssignments.Count; ++i) {
                var subtreeIndex = (ushort)(subtreeAssignments[i].NodeIndex + subtreeAssignments[i].IndexOffset);
                var subtreeParentIndex = subtreeAssignments[i].ParentIndex;
#if UNITY_EDITOR
                var positionOffset = Vector2.zero;
#endif
                for (int j = 0; j < subtreeAssignments[i].NodeCount; ++j) {
                    var origNode = m_SubtreeNodesReference[subtreeAssignments[i].ReferenceIndex].Nodes[subtreeAssignments[i].SubtreeIndex][j];
                    // The node needs to be copied to prevent the same node from being used in multiple trees.
                    var node = CopyUtility.DeepCopy(m_SubtreeNodesReference[subtreeAssignments[i].ReferenceIndex].Nodes[subtreeAssignments[i].SubtreeIndex][j]) as ILogicNode;
                    node.Index = (ushort)(subtreeIndex + j);
                    node.RuntimeIndex = ushort.MaxValue;
                    if (j == 0) {
                        node.ParentIndex = subtreeParentIndex;
                        subtreeParentIndex = node.Index; // The subsequent subtree tasks should use the first subtree task as the parent reference.
                        node.SiblingIndex = subtreeAssignments[i].SiblingIndex != ushort.MaxValue ? (ushort)(subtreeIndex + subtreeAssignments[i].NodeCount) : ushort.MaxValue;
                    } else {
                        // Adjust the subsequent subtree tasks by the location of the insertion.
                        node.ParentIndex += subtreeParentIndex;
                        if (node.SiblingIndex != ushort.MaxValue) {
                            node.SiblingIndex += subtreeIndex;
                        }
                    }
                    m_Tasks[subtreeIndex + j] = node;
#if UNITY_EDITOR
                    var nodeProperties = CopyUtility.DeepCopy(subtreeAssignments[i].Subtree.LogicNodeProperties[j]) as LogicNodeProperties;
                    nodeProperties.GuidString = Guid.NewGuid().ToString();
                    if (j == 0) {
                        // Keep the tasks in the same relative position as the subtree reference.
                        positionOffset = subtreeAssignments[i].NodePropertiesPosition - subtreeAssignments[i].Subtree.LogicNodeProperties[j].Position;
                    } else {
                        // Apply a small offset for stacked subtrees so they are not directly overlapping.
                        positionOffset += new Vector2(2, 2);
                    }
                    nodeProperties.Position += positionOffset;
                    nodeProperties.Collapsed = subtreeAssignments[i].Collapsed;
                    m_LogicNodeProperties[subtreeIndex + j] = nodeProperties;
#endif
                }
            }
        }

        /// <summary>
        /// Returns the Node of the specified type.
        /// </summary>
        /// <param name="type">The type of Node that should be retrieved.</typeparam>
        /// <returns>The Node of the specified type (can be null).</returns>
        public ILogicNode GetNode(Type type)
        {
            if (m_Tasks == null) {
                return null;
            }

            for (int i = 0; i < m_Tasks.Length; ++i) {
                if (m_Tasks[i].GetType() == type) {
                    return m_Tasks[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the event node of the specified type.
        /// </summary>
        /// <param name="type">The type of EventNode that should be retrieved.</typeparam>
        /// <returns>The EventNode of the specified type (can be null). If the node is found the index will also be returned.</returns>
        public (IEventNode, ushort) GetEventNode(Type type)
        {
            if (m_EventTasks == null) {
                return (null, ushort.MaxValue);
            }

            for (ushort i = 0; i < m_EventTasks.Length; ++i) {
                if (m_EventTasks[i].GetType() == type) {
                    return (m_EventTasks[i], i);
                }
            }
            return (null, ushort.MaxValue);
        }

        /// <summary>
        /// Returns the total number of children belonging to the specified node.
        /// </summary>
        /// <param name="node">The node to retrieve the child count of.</param>
        /// <param name="nodes">All of the nodes that belong to the graph.</param>
        /// <returns>The total number of children belonging to the specified node.</returns>
        public int GetChildCount(ILogicNode node, ILogicNode[] nodes)
        {
            if (node.SiblingIndex != ushort.MaxValue) {
                return node.SiblingIndex - node.Index - 1;
            }

            if (node.Index + 1 == nodes.Length) {
                return 0;
            }

            var child = nodes[node.Index + 1];
            if (child.ParentIndex != node.Index) {
                return 0;
            }

            // Determine the child count based off of the sibling index.
            while (child.SiblingIndex != ushort.MaxValue) {
                child = nodes[child.SiblingIndex];
            }

            return child.Index - node.Index + GetChildCount(child, nodes);
        }

        /// <summary>
        /// Reevaluates the SubtreeReferences by calling the EvaluateSubtrees method.
        /// </summary>
        /// <param name="OnBeforeReevaluationSwap">Action that should be done before the tasks are swapped.</param>
        /// <returns>True if the subtree was reevaluated.</returns>
        public bool ReevaluateSubtreeReferences(Action onBeforeReevaluationSwap)
        {
            // The tree must contain tasks.
            if (!Application.isPlaying || m_Tasks == null || m_Tasks.Length == 0) {
                return false;
            }

            // Subtree references must exist.
            if (m_SubtreeNodesReference == null || m_SubtreeNodesReference.Count == 0) {
                return false;
            }

            if (onBeforeReevaluationSwap != null) {
                onBeforeReevaluationSwap();
            }

            // Find the new reevaluated nodes.
            for (int i = m_SubtreeNodesReference.Count - 1; i >= 0; --i) {
                var subtreeNodesReference = m_SubtreeNodesReference[i];
                var subtreeReference = m_SubtreeNodesReference[i].SubtreeReference;
                subtreeReference.EvaluateSubtrees();
                var reevaluatedSubtrees = subtreeReference.Subtrees;
                ILogicNode[][] reevaluatedNodes;
                if (reevaluatedSubtrees == null) {
                    continue;
                }

                // The parent must be able to accept the number of subtrees that there are.
                var parentIndex = m_Tasks[m_SubtreeNodesReference[i].NodeIndex].ParentIndex;
                IParentNode parentNode = null;
                if (parentIndex != ushort.MaxValue) {
                    parentNode = m_Tasks[parentIndex] as IParentNode;
                }

                if ((parentNode == null && reevaluatedSubtrees.Length > 1) || (parentNode != null && reevaluatedSubtrees.Length > parentNode.MaxChildCount)) {
                    Debug.LogError($"Error: the reevaluated graph contains multiple subtrees as the starting task or as a child of a parent task which cannot contain so many children (such as a decorator).");
                    continue;
                }

                reevaluatedNodes = new ILogicNode[reevaluatedSubtrees.Length][];
                var errorState = false;
                for (int j = 0; j < reevaluatedSubtrees.Length; ++j) {
                    if (reevaluatedSubtrees[j] == null) {
                        continue;
                    }
                    if (!reevaluatedSubtrees[j].Deserialize(true, true, subtreeReference.SharedVariableOverrides)) {
                        errorState = true;
                        break;
                    };
                    // Keep a reference to the deserialized nodes. This will ensure they are unique and do not get overwritten.
                    reevaluatedNodes[j] = reevaluatedSubtrees[j].LogicNodes;
                }
                if (errorState) {
                    continue;
                }

                // The subtree index will be offsetted from the original index value if there are multiple subtree references.
                var nodeOffset = 0;
                for (int j = i - 1; j >= 0; --j) {
                    nodeOffset += m_SubtreeNodesReference[j].NodeCount - 1;
                }

                // All of the reevaluated nodes have been determined. Remove the old subtree nodes.
                var nodeCount = m_SubtreeNodesReference[i].NodeCount;

                // Replace the first node with the subtree reference, and remove the rest of the added nodes.
                m_Tasks[m_SubtreeNodesReference[i].NodeIndex + nodeOffset] = m_SubtreeNodesReference[i].SubtreeReference as ILogicNode;
                for (int j = m_SubtreeNodesReference[i].NodeIndex + nodeOffset + 1; j < m_Tasks.Length - nodeCount + 1; ++j) {
                    m_Tasks[j] = m_Tasks[j + nodeCount - 1];
                    m_Tasks[j].Index = (ushort)j;
                    if (m_Tasks[j].ParentIndex != ushort.MaxValue && m_Tasks[j].ParentIndex > m_SubtreeNodesReference[i].NodeIndex + nodeOffset) {
                        m_Tasks[j].ParentIndex -= (ushort)(nodeCount - 1);
                    }
                    if (m_Tasks[j].SiblingIndex != ushort.MaxValue && m_Tasks[j].SiblingIndex > m_SubtreeNodesReference[i].NodeIndex + nodeOffset) {
                        m_Tasks[j].SiblingIndex -= (ushort)(nodeCount - 1);
                    }

#if UNITY_EDITOR
                    m_LogicNodeProperties[j] = m_LogicNodeProperties[j + nodeCount - 1];
#endif
                }

                // Restore the original ConnectedIndex value.
                if (m_EventTasks != null) {
                    for (int j = 0; j < m_EventTasks.Length; ++j) {
                        if (m_EventTasks[j].ConnectedIndex > m_SubtreeNodesReference[i].NodeIndex) {
                            m_EventTasks[j].ConnectedIndex -= (ushort)(nodeCount - 1);
                        }
                    }
                }
                Array.Resize(ref m_Tasks, m_Tasks.Length - nodeCount + 1);
#if UNITY_EDITOR
                Array.Resize(ref m_LogicNodeProperties, m_LogicNodeProperties.Length - nodeCount + 1);
#endif

                // Replace the old nodes with the new nodes.
                subtreeNodesReference.Nodes = reevaluatedNodes;
                m_SubtreeNodesReference[i] = subtreeNodesReference;

                // The disabled nodes also need to be removed.
                var subtrees = m_SubtreeNodesReference[i].Subtrees;
                var disabledNodesCount = 0;
                for (int j = 0; j < subtrees.Length; ++j) {
                    if (subtrees[j].DisabledLogicNodes == null || subtrees[j].DisabledLogicNodes.Length == 0) {
                        continue;
                    }
                    disabledNodesCount += subtrees[j].DisabledLogicNodes.Length;
                }
                if (disabledNodesCount > 0) {
                    if (m_DisabledLogicNodes.Length > disabledNodesCount) { // The local tree may not have any disabled nodes.
                        for (int j = 0; j < m_DisabledLogicNodes.Length - disabledNodesCount; ++j) {
                            if (m_DisabledLogicNodes[j] >= m_SubtreeNodesReference[i].NodeIndex + nodeOffset + 1) {
                                m_DisabledLogicNodes[j] = (ushort)(m_DisabledLogicNodes[j + disabledNodesCount] - nodeCount + 1);
                            }
                        }
                    }
                    Array.Resize(ref m_DisabledLogicNodes, m_DisabledLogicNodes.Length - disabledNodesCount);
                }
            }

            // The tasks array has been restored to the original set of nodes with the ISubtreeReference. Inject the new nodes.
            InjectSubtrees();

            // Modify the ConnectedIndex to match the injection.
            if (m_EventTasks != null && m_SubtreeNodesReference != null) {
                for (int i = 0; i < m_EventTasks.Length; ++i) {
                    var offset = 0;
                    for (int j = 0; j < m_SubtreeNodesReference.Count; ++j) {
                        if (m_SubtreeNodesReference[j].NodeIndex >= m_EventTasks[i].ConnectedIndex) {
                            break;
                        }
                        offset += m_SubtreeNodesReference[j].NodeCount - 1;
                    }
                    if (offset != 0) {
                        m_EventTasks[i].ConnectedIndex += (ushort)offset;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the SharedVariable with the specified name.
        /// </summary>
        /// <param name="graph">The graph that the data belongs to.</typeparam>
        /// <param name="name">The name of the SharedVariable that should be retrieved.</typeparam>
        /// <param name="scope">The scope of the SharedVariable that should be retrieved.</param>
        /// <returns>The SharedVariable with the specified name (can be null).</returns>
        public SharedVariable GetVariable(IGraph graph, string name, SharedVariable.SharingScope scope)
        {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            DeserializeSharedVariables(false, null);
            PopulateSharedVariablesMapping(graph, ref m_VariableByNameMap);

            if (m_VariableByNameMap != null && m_VariableByNameMap.TryGetValue(new VariableAssignment(name, scope), out var variable)) {
                return variable;
            }

            return null;
        }

        /// <summary>
        /// Returns the SharedVariable of the specified type.
        /// </summary>
        /// <param name="graph">The graph that the data belongs to.</typeparam>
        /// <param name="name">The name of the SharedVariable that should be retrieved.</typeparam>
        /// <param name="scope">The scope of the SharedVariable that should be retrieved.</param>
        /// <returns>The SharedVariable with the specified name (can be null).</returns>
        public SharedVariable<T> GetVariable<T>(IGraph graph, string name, SharedVariable.SharingScope scope)
        {
            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            DeserializeSharedVariables(false, null);
            PopulateSharedVariablesMapping(graph, ref m_VariableByNameMap);

            if (m_VariableByNameMap != null && m_VariableByNameMap.TryGetValue(new VariableAssignment(name, scope), out var variable)) {
                return variable as SharedVariable<T>;
            }

            return null;
        }

        /// <summary>
        /// Sets the value of the SharedVariable.
        /// </summary>
        /// <typeparam name="T">The type of SharedVarible.</typeparam>
        /// <param name="graph">The graph that the data belongs to.</typeparam>
        /// <param name="name">The name of the SharedVariable.</param>
        /// <param name="value">The value of the SharedVariable.</param>
        public void SetVariableValue<T>(IGraph graph, string name, T value, SharedVariable.SharingScope scope)
        {
            if (string.IsNullOrEmpty(name)) {
                return;
            }

            DeserializeSharedVariables(false, null);
            PopulateSharedVariablesMapping(graph, ref m_VariableByNameMap);

            if (m_VariableByNameMap == null || !m_VariableByNameMap.TryGetValue(new VariableAssignment(name, scope), out var variable)) {
                return;
            }

            (variable as SharedVariable<T>).Value = value;
        }

        /// <summary>
        /// Overrides the SharedVariable reference. The name must match an exsting variable.
        /// </summary>
        /// <param name="graph">The graph that the data belongs to.</typeparam>
        /// <param name="variable">The reference to the SharedVariable.</param>
        internal void OverrideVariable(IGraph graph, SharedVariable variable)
        {
            if (string.IsNullOrEmpty(variable.Name)) {
                return;
            }

            DeserializeSharedVariables(false, null);
            var dirty = false;
            if (m_SharedVariables != null) {
                for (int i = 0; i < m_SharedVariables.Length; ++i) {
                    if (m_SharedVariables[i].Name == variable.Name) {
                        m_SharedVariables[i] = variable;
                        dirty = true;
                        break;
                    }
                }
            }

            if (dirty) {
                PopulateSharedVariablesMapping(graph, ref m_VariableByNameMap);
            }
        }

        /// <summary>
        /// Is the node with the specified index enabled?
        /// </summary>
        /// <param name="logicNode">Is the node a LogicNode?</param>
        /// <param name="index">The index of the node.</param>
        /// <returns>True if the node with the specified index is enabled.</returns>
        public bool IsNodeEnabled(bool logicNode, int index)
        {
            if (index == ushort.MaxValue) {
                return true;
            }

            var disabledNodes = logicNode ? m_DisabledLogicNodes : m_DisabledEventNodes;
            if (disabledNodes == null) {
                return true;
            }
            for (int i = 0; i < disabledNodes.Length; ++i) {
                if (disabledNodes[i] == index) {
                    return false;
                }
            }
            return true;
        }
    }
}
#endif
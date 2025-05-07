#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using System;
    using UnityEngine;

    /// <summary>
    /// The behavior tree stored on a Scriptable Object.
    /// </summary>
    [CreateAssetMenu(fileName = "Subtree", menuName = "Opsive/Behavior Designer/Subtree", order = 1)]
    public class Subtree : ScriptableObject, IGraph, ISharedVariableContainer
    {
        [Tooltip("The behavior tree data.")]
        [SerializeField] private BehaviorTreeData m_Data = new BehaviorTreeData();

        public string Name { get => name; set => name = value; }
        public SharedVariable.SharingScope VariableScope { get => SharedVariable.SharingScope.Graph; }
        public BehaviorTreeData Data { get => m_Data; }
        public UnityEngine.Object Parent { get => this; }
        public ILogicNode[] LogicNodes { get => m_Data.LogicNodes; set => m_Data.LogicNodes = value; }
        public IEventNode[] EventNodes { get => m_Data.EventNodes; set => m_Data.EventNodes = value; }
        public SharedVariable[] SharedVariables { get => m_Data.SharedVariables; set => m_Data.SharedVariables = value; }
        public ushort[] DisabledLogicNodes { get => m_Data.DisabledLogicNodes; set => m_Data.DisabledLogicNodes = value; }
        public ushort[] DisabledEventNodes { get => m_Data.DisabledEventNodes; set => m_Data.DisabledEventNodes = value; }

        public LogicNodeProperties[] LogicNodeProperties
        {
#if UNITY_EDITOR
            get => m_Data.LogicNodeProperties;
            set => m_Data.LogicNodeProperties = value;
#else
            get => null;
            set { }
#endif
        }
        public NodeProperties[] EventNodeProperties
        {
#if UNITY_EDITOR
            get => m_Data.EventNodeProperties;
            set => m_Data.EventNodeProperties = value;
#else
            get => null;
            set { }
#endif
        }
        public GroupProperties[] GroupProperties
        {
#if UNITY_EDITOR
            get => m_Data.GroupProperties;
            set => m_Data.GroupProperties = value;
#else
            get => null;
            set { }
#endif
        }

        public int UniqueID { get => m_Data.UniqueID; }

        /// <summary>
        /// Serializes the behavior tree.
        /// </summary>
        public void Serialize()
        {
            m_Data.Serialize();
        }

        /// <summary>
        /// Deserialize the behavior tree.
        /// </summary>
        /// <param name="force">Should the behavior tree be force deserialized?</param>
        /// <returns>True if the tree was deserialized.</returns>
        public bool Deserialize(bool force = false)
        {
            return Deserialize(force, false, null);
        }

        /// <summary>
        /// Deserialize the behavior tree.
        /// </summary>
        /// <param name="force">Should the behavior tree be force deserialized?</param>
        /// <param name="injectSubtrees">Should the subtrees be injected into the behavior tree?</param>
        /// <returns>True if the tree was deserialized.</returns>
        public bool Deserialize(bool force, bool injectSubtrees)
        {
            return Deserialize(force, injectSubtrees, null);
        }

        /// <summary>
        /// Deserialize the behavior tree.
        /// </summary>
        /// <param name="force">Should the behavior tree be force deserialized?</param>
        /// <param name="injectSubtrees">Should the subtrees be injected into the behavior tree?</param>
        /// <param name="sharedVariableOverrides">A list of SharedVariables that should override the current SharedVariable value.</param>
        /// <returns>True if the tree was deserialized.</returns>
        public bool Deserialize(bool force, bool injectSubtrees, SharedVariableOverride[] sharedVariableOverrides)
        {
            if (m_Data == null) {
                return false;
            }

            return m_Data.Deserialize(this, force, !Application.isPlaying, injectSubtrees, sharedVariableOverrides);
        }

        /// <summary>
        /// Deserializes the SharedVariables. This allows the SharedVariables to be deserialized independently.
        /// </summary>
        /// <param name="force">Should the variables be forced deserialized?</param>
        /// <returns>True if the SharedVariables were deserialized.</returns>
        public bool DeserializeSharedVariables(bool force)
        {
            if (m_Data == null) {
                return false;
            }

            return m_Data.DeserializeSharedVariables(force);
        }

        /// <summary>
        /// Adds the specified logic node.
        /// </summary>
        /// <param name="node">The node that should be added.</param>
        public void AddNode(ILogicNode node)
        {
            m_Data.AddNode(node);
        }

        /// <summary>
        /// Removes the specified logic node.
        /// </summary>
        /// <param name="node">The node that should be removed.</param>
        /// <returns>True if the node was removed.</returns>
        public bool RemoveNode(ILogicNode node)
        {
            return m_Data.RemoveNode(node);
        }

        /// <summary>
        /// Adds the specified event node.
        /// </summary>
        /// <param name="eventNode">The event node that should be added.</param>
        public void AddNode(IEventNode eventNode)
        {
            m_Data.AddNode(eventNode);
        }

        /// <summary>
        /// Removes the specified event node.
        /// </summary>
        /// <param name="eventNode">The event node that should be removed.</param>
        /// <returns>True if the event node was removed.</returns>
        public bool RemoveNode(IEventNode eventNode)
        {
            return m_Data.RemoveNode(eventNode);
        }

        /// <summary>
        /// Returns the Node of the specified type.
        /// </summary>
        /// <param name="type">The type of Node that should be retrieved.</typeparam>
        /// <returns>The Node of the specified type (can be null).</returns>
        public ILogicNode GetNode(Type type)
        {
            return m_Data.GetNode(type);
        }

        /// <summary>
        /// Returns the EventNode of the specified type.
        /// </summary>
        /// <param name="type">The type of EventNode that should be retrieved.</typeparam>
        /// <returns>The EventNode of the specified type (can be null). If the node is found the index will also be returned.</returns>
        public (IEventNode, ushort) GetEventNode(Type type)
        {
            return m_Data.GetEventNode(type);
        }

        /// <summary>
        /// Returns the SharedVariable with the specified name.
        /// </summary>
        /// <param name="name">The name of the SharedVariable that should be retrieved.</typeparam>
        /// <returns>The SharedVariable with the specified name (can be null).</returns>
        public SharedVariable GetVariable(string name)
        {
            return m_Data.GetVariable(this, name, SharedVariable.SharingScope.Graph);
        }

        /// <summary>
        /// Returns the SharedVariable of the specified name.
        /// </summary>
        /// <param name="name">The name of the SharedVariable that should be retrieved.</typeparam>
        /// <returns>The SharedVariable with the specified name (can be null).</returns>
        public SharedVariable<T> GetVariable<T>(string name)
        {
            return m_Data.GetVariable<T>(this, name, SharedVariable.SharingScope.Graph);
        }

        /// <summary>
        /// Sets the value of the SharedVariable.
        /// </summary>
        /// <typeparam name="T">The type of SharedVarible.</typeparam>
        /// <param name="name">The name of the SharedVariable.</param>
        /// <param name="value">The value of the SharedVariable.</param>
        public void SetVariableValue<T>(string name, T value)
        {
            m_Data.SetVariableValue<T>(this, name, value, SharedVariable.SharingScope.Graph);
        }

        /// <summary>
        /// Overrides the SharedVariable reference. The name must match an existing variable.
        /// </summary>
        /// <param name="variable">The reference to the SharedVariable.</param>
        internal void OverrideVariable(SharedVariable variable)
        {
            m_Data.OverrideVariable(this, variable);
        }

        /// <summary>
        /// Is the node with the specified index enabled?
        /// </summary>
        /// <param name="logicNode">Is the node a LogicNode?</param>
        /// <param name="index">The index of the node.</param>
        /// <returns>True if the node with the specified index is enabled.</returns>
        public bool IsNodeEnabled(bool logicNode, int index)
        {
            return m_Data.IsNodeEnabled(logicNode, index);
        }

        /// <summary>
        /// Is the node with the specified index active?
        /// </summary>
        /// <param name="logicNode">Is the node a LogicNode?</param>
        /// <param name="index">The index of the node.</param>
        /// <returns>True if the node with the specified index is active.</returns>
        public bool IsNodeActive(bool logicNode, int index)
        {
            return false; // The subtree node itself is never active.
        }

        /// <summary>
        /// Copies the graph onto the current graph.
        /// </summary>
        /// <param name="other">The graph that should be copied.</param>
        public void Clone(IGraph other)
        {
            m_Data = new BehaviorTreeData();
            m_Data.EventNodes = other.EventNodes;
            m_Data.LogicNodes = other.LogicNodes;
            m_Data.SharedVariables = other.SharedVariables;

#if UNITY_EDITOR
            m_Data.EventNodeProperties = other.EventNodeProperties;
            m_Data.LogicNodeProperties = other.LogicNodeProperties;
            m_Data.GroupProperties = other.GroupProperties;
#endif

            m_Data.Serialize();
        }

        /// <summary>
        /// Overrides ToString.
        /// </summary>
        /// <returns>The desired string value.</returns>
        public override string ToString()
        {
            return name;
        }
    }

    /// <summary>
    /// Attribute indicating that a ReorderableList should be used for the Subtree array.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SubtreeListAttribute : System.Attribute
    {

    }
}
#endif
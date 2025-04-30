#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks
{
    using Opsive.GraphDesigner.Runtime;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using System;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// The TaskDelegateBase task is an abstract class used for any action classes that use reflection to execute the action.
    /// </summary>
    [HideInFilterWindow]
    public abstract class TaskDelegateBase : Task, IAction
    {
        [Tooltip("The object that the delegate belongs to. Can be null for static variables.")]
        [SerializeField] [HideInInspector] protected SharedVariable m_Target;
        [Tooltip("The type of delegate that should be created.")]
        [SerializeField] [HideInInspector] protected string m_ReflectedType;
        [Tooltip("The type of parameters that the delegate uses.")]
        [SerializeField] [HideInInspector] protected string[] m_ParameterTypes;
        [Tooltip("The name of the method that should be called by the delegate.")]
        [SerializeField] [HideInInspector] protected string m_MethodName;

        public SharedVariable Target => m_Target;
        public string ReflectedType => m_ReflectedType;
        public string[] ParameterTypes => m_ParameterTypes;
        public string MethodName => m_MethodName;

        protected bool m_ConditionalTask;

        /// <summary>
        /// Binds the task to the specified method.
        /// </summary>
        /// <param name="methodInfo">The MethodInfo to bind the task to.</param>
        internal void Bind(MethodInfo methodInfo)
        {
            m_ReflectedType = methodInfo.ReflectedType.FullName;
            var parameters = methodInfo.GetParameters();
            if (parameters != null && parameters.Length > 0) {
                m_ParameterTypes = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i) {
                    m_ParameterTypes[i] = parameters[i].ParameterType.FullName;
                }
            }
            m_MethodName = methodInfo.Name;
        }

        /// <summary>
        /// Initializes the task.
        /// </summary>
        /// <param name="behaviorTree">A reference to the owning BehaviorTree.</param>
        /// <param name="conditionalTask">Does the delegate belong to an IConditional task?</param>
        /// <param name="runtimeIndex">The runtime index of the node.</param>
        internal void Initialize(BehaviorTree behaviorTree, ushort runtimeIndex, bool conditionalTask)
        {
            m_ConditionalTask = conditionalTask;

            base.Initialize(behaviorTree, runtimeIndex);
        }

        /// <summary>
        /// Returns the target value.
        /// </summary>
        /// <returns>The target value.</returns>
        protected object GetTargetValue()
        {
            // The target will be null if no SharedVariable value has been assigned.
            var target = m_Target.GetValue();
            if (target == null) {
                var targetType = m_ReflectedType.Replace("UnityEngine.", string.Empty);
                if (string.Equals(targetType, "GameObject")) {
                    return m_GameObject;
                }
                var value = m_GameObject.GetComponent(targetType);
                if (value == null) {
                    var splitType = m_ReflectedType.Split(".");
                    value = m_GameObject.GetComponent(splitType[splitType.Length - 1]);
                    if (value == null) {
                        Debug.LogError($"Error: Unable to find the component {m_ReflectedType} on the {m_GameObject.name} GameObject.");
                    }
                }
                return value;
            }
            return target;
        }

        /// <summary>
        /// Returns the method from the given type and name.
        /// </summary>
        /// <param name="reflectedType">The object type that the method belongs to.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="parameterTypeNames">The types of parameters that the method uses.</param>
        /// <returns>The method from the given type and name.</returns>
        protected static MethodInfo GetMethod(string reflectedType, string methodName, string[] parameterTypeNames)
        {
            var type = TypeUtility.GetType(reflectedType);
            if (type == null) {
                Debug.LogError($"Error: Unable to find the type {reflectedType}.");
                return null;
            }

            Type[] parameterTypes;
            if (parameterTypeNames != null) {
                parameterTypes = new Type[parameterTypeNames.Length];
                for (int i = 0; i < parameterTypeNames.Length; ++i) {
                    parameterTypes[i] = TypeUtility.GetType(parameterTypeNames[i]);
                    if (parameterTypes[i] == null) {
                        Debug.LogError($"Error: Unable to find the parameter type {parameterTypeNames[i]}.");
                        return null;
                    }
                }
            } else {
                parameterTypes = new Type[0];
            }

            // Get the method based on the type and parameter types.
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static, null, parameterTypes, null);
            if (method == null) {
                Debug.LogError($"Error: Unable to find the method {methodName} on type {type}.");
                return null;
            }

            return method;
        }

        /// <summary>
        /// Specifies the type of reflection that should be used to save the task.
        /// </summary>
        /// <param name="index">The index of the sub-task. This is used for the task set allowing each contained task to have their own save type.</param>
        public override MemberVisibility GetSaveReflectionType(int index)
        {
            return MemberVisibility.Public;
        }

        /// <summary>
        /// Returns a friendly name for the task.
        /// </summary>
        /// <returns>A friendly name for the task.</returns>
        public override string ToString()
        {
            return m_MethodName;
        }
    }

    /// <summary>
    /// Task which executes a delegate with no parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate : TaskDelegateBase
    {
        private Action m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(System.Action)) as System.Action;
            } else {
                m_Delegate = method.CreateDelegate(typeof(System.Action), GetTargetValue()) as System.Action;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate();
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with no parameters but a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<TResult> : TaskDelegateBase
    {
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<TResult>)) as Func<TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<TResult>), GetTargetValue()) as Func<TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate();
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with one parameter.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;

        private Action<T1> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1>)) as Action<T1>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1>), GetTargetValue()) as Action<T1>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with one parameter and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, TResult>)) as Func<T1, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, TResult>), GetTargetValue()) as Func<T1, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with two parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;

        private Action<T1, T2> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2>)) as Action<T1, T2>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2>), GetTargetValue()) as Action<T1, T2>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with two parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, TResult>)) as Func<T1, T2, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, TResult>), GetTargetValue()) as Func<T1, T2, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with three parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;

        private Action<T1, T2, T3> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3>)) as Action<T1, T2, T3>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3>), GetTargetValue()) as Action<T1, T2, T3>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with three parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, TResult>)) as Func<T1, T2, T3, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, TResult>), GetTargetValue()) as Func<T1, T2, T3, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with four parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;

        private Action<T1, T2, T3, T4> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4>)) as Action<T1, T2, T3, T4>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4>), GetTargetValue()) as Action<T1, T2, T3, T4>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with four parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, TResult>)) as Func<T1, T2, T3, T4, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with five parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;

        private Action<T1, T2, T3, T4, T5> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5>)) as Action<T1, T2, T3, T4, T5>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5>), GetTargetValue()) as Action<T1, T2, T3, T4, T5>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with five parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, TResult>)) as Func<T1, T2, T3, T4, T5, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with six parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5, T6> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;

        private Action<T1, T2, T3, T4, T5, T6> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6>)) as Action<T1, T2, T3, T4, T5, T6>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6>), GetTargetValue()) as Action<T1, T2, T3, T4, T5, T6>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with six parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, T6, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, T6, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, TResult>)) as Func<T1, T2, T3, T4, T5, T6, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, T6, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with seven parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5, T6, T7> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;

        private Action<T1, T2, T3, T4, T5, T6, T7> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7>)) as Action<T1, T2, T3, T4, T5, T6, T7>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7>), GetTargetValue()) as Action<T1, T2, T3, T4, T5, T6, T7>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with seven parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, T6, T7, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, T6, T7, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>)) as Func<T1, T2, T3, T4, T5, T6, T7, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, T6, T7, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with eight parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5, T6, T7, T8> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;

        private Action<T1, T2, T3, T4, T5, T6, T7, T8> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>)) as Action<T1, T2, T3, T4, T5, T6, T7, T8>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8>), GetTargetValue()) as Action<T1, T2, T3, T4, T5, T6, T7, T8>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with eight parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)) as Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with nine parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;
        [Tooltip("The ninth parameter.")]
        [SerializeField] protected SharedVariable<T9> m_Parameter9;

        private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>)) as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>), GetTargetValue()) as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value, m_Parameter9.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with nine parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;
        [Tooltip("The ninth parameter.")]
        [SerializeField] protected SharedVariable<T9> m_Parameter9;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)) as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value, m_Parameter9.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with ten parameters.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;
        [Tooltip("The ninth parameter.")]
        [SerializeField] protected SharedVariable<T9> m_Parameter9;
        [Tooltip("The tenth parameter.")]
        [SerializeField] protected SharedVariable<T10> m_Parameter10;

        private Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>)) as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>), GetTargetValue()) as Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value, m_Parameter9.Value, m_Parameter10.Value);
            return TaskStatus.Success;
        }
    }

    /// <summary>
    /// Task which executes a delegate with ten parameters and a returned value.
    /// </summary>
    [NodeIcon("3bbdfa553da4d554e9d74f8d88915aac", "6437308e972f99f48953f20198fd4e94")]
    public class TaskValueDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> : TaskDelegateBase
    {
        [Tooltip("The first parameter.")]
        [SerializeField] protected SharedVariable<T1> m_Parameter1;
        [Tooltip("The second parameter.")]
        [SerializeField] protected SharedVariable<T2> m_Parameter2;
        [Tooltip("The third parameter.")]
        [SerializeField] protected SharedVariable<T3> m_Parameter3;
        [Tooltip("The fourth parameter.")]
        [SerializeField] protected SharedVariable<T4> m_Parameter4;
        [Tooltip("The fifth parameter.")]
        [SerializeField] protected SharedVariable<T5> m_Parameter5;
        [Tooltip("The sixth parameter.")]
        [SerializeField] protected SharedVariable<T6> m_Parameter6;
        [Tooltip("The seventh parameter.")]
        [SerializeField] protected SharedVariable<T7> m_Parameter7;
        [Tooltip("The eigth parameter.")]
        [SerializeField] protected SharedVariable<T8> m_Parameter8;
        [Tooltip("The ninth parameter.")]
        [SerializeField] protected SharedVariable<T9> m_Parameter9;
        [Tooltip("The tenth parameter.")]
        [SerializeField] protected SharedVariable<T10> m_Parameter10;
        [Tooltip("The returned result.")]
        [SerializeField] [RequireShared] protected SharedVariable<TResult> m_Result;

        private Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> m_Delegate;

        /// <summary>
        /// Creates the delegate.
        /// </summary>
        public override void OnAwake()
        {
            base.OnAwake();

            var method = GetMethod(m_ReflectedType, m_MethodName, m_ParameterTypes);
            if (method == null) {
                return;
            }

            if (method.IsStatic) {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>)) as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>;
            } else {
                m_Delegate = method.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>), GetTargetValue()) as Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>;
            }
        }

        /// <summary>
        /// Invokes the delegate.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            m_Result.Value = m_Delegate(m_Parameter1.Value, m_Parameter2.Value, m_Parameter3.Value, m_Parameter4.Value, m_Parameter5.Value, m_Parameter6.Value, m_Parameter7.Value, m_Parameter8.Value, m_Parameter9.Value, m_Parameter10.Value);
            if (m_ConditionalTask) {
                return Convert.ToBoolean(m_Result.Value) ? TaskStatus.Success : TaskStatus.Failure;
            }
            return TaskStatus.Success;
        }
    }
}
#endif
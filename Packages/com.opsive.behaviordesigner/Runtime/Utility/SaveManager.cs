#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Utility
{
    using Opsive.BehaviorDesigner.Runtime;
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.GraphDesigner.Runtime.Variables;
    using Opsive.Shared.Utility;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using Unity.Entities;
    using UnityEngine;
    using static Opsive.BehaviorDesigner.Runtime.BehaviorTreeData;

    /// <summary>
    /// Helper class which will save and load behavior tree tasks.
    /// </summary>
    public static class SaveManager
    {
        /// <summary>
        /// Specifies which objects should be saved.
        /// </summary>
        public enum VariableSaveScope : byte
        {
            GameObjectVariables = 1,    // Saves the GameObjectVariables.
            SceneVariables = 2,         // Saves the SceneVariables.
            ProjectVariables = 4        // Saves the ProjectVariables.
        }

        /// <summary>
        /// The save data associated with the behavior tree and shared variables.
        /// </summary>
        [System.Serializable]
        public struct SaveData
        {
            [Tooltip("The behavior tree save data.")]
            public BehaviorTreeSaveData[] BehaviorTreeSaveData;
            [Tooltip("The external shared variable save data.")]
            public VariableSaveData[] VariableSaveData;
        }

        /// <summary>
        /// The save data associated with the behavior tree.
        /// </summary>
        [System.Serializable]
        public struct BehaviorTreeSaveData
        {
            [Tooltip("The unique ID of the save data. This will change each time the data is serialized")]
            public int UniqueID;
            [Tooltip("The loaded task components.")]
            public Serialization[] TaskComponents;
            [Tooltip("The loaded branch components.")]
            public Serialization[] BranchComponents;
            [Tooltip("The loaded reevaluate task components.")]
            public Serialization[] ReevaluateTaskComponents;
            [Tooltip("The user task data.")]
            public TaskSaveData[] TaskData;
            [Tooltip("The values of the graph SharedVariables.")]
            public VariableSaveData GraphSharedVariables;
        }

        /// <summary>
        /// The save data associated with the shared variables.
        /// </summary>
        [System.Serializable]
        public struct VariableSaveData
        {
            [Tooltip("The unique ID of the variable data. This will change each time the data is serialized")]
            public int UniqueID;
            [Tooltip("The values of the SharedVariables.")]
            public Serialization[] Values;
            [Tooltip("The scope of the variables.")]
            public SharedVariable.SharingScope Scope;
        }

        /// <summary>
        /// Container for the task save data. Allows for nested tasks.
        /// </summary>
        [System.Serializable]
        public struct TaskSaveData
        {
            [Tooltip("The save data associated with each task.")]
            public Serialization[] Value;
        }

        /// <summary>
        /// Gets the save data from the specified behavior trees.
        /// </summary>
        /// <param name="behaviorTrees">The behavior trees that should be saved.</param>
        /// <param name="variableSaveScope">Specifies which variables should be saved. Graph variables will automatically be saved.</param>
        /// <returns>The resulting save data. Can be null.</returns>
        public static SaveData? Save(BehaviorTree[] behaviorTrees, VariableSaveScope variableSaveScope = 0)
        {
            if (!Application.isPlaying) {
                Debug.LogWarning($"Warning: Behavior trees can only be saved at runtime.");
                return null;
            }

            if (behaviorTrees.Length == 0) {
                return null;
            }

            // Assume all behavior trees can be saved.
            var saveData = new SaveData();
            saveData.BehaviorTreeSaveData = new BehaviorTreeSaveData[behaviorTrees.Length];
            var variableSaveDataList = new List<VariableSaveData>();
            HashSet<GameObject> savedGameObjectVariables = null;
            var behaviorTreeSaveCount = 0;
            for (int i = 0; i < behaviorTrees.Length; ++i) {
                if (behaviorTrees[i] == null) {
                    continue;
                }

                // Add the save data to the array if the save is valid.
                var behaviorTreeSaveData = Save(behaviorTrees[i]);
                if (behaviorTreeSaveData.HasValue) {
                    saveData.BehaviorTreeSaveData[behaviorTreeSaveCount] = behaviorTreeSaveData.Value;
                    behaviorTreeSaveCount++;

                    // The associated GameObject variables may also need to be saved.
                    if ((variableSaveScope & VariableSaveScope.GameObjectVariables) != 0) {
                        // Only save the GameObject variables once.
                        if (savedGameObjectVariables == null) {
                            savedGameObjectVariables = new HashSet<GameObject>();
                        }

                        if (savedGameObjectVariables.Contains(behaviorTrees[i].gameObject)) {
                            continue;
                        }
                        savedGameObjectVariables.Add(behaviorTrees[i].gameObject);

                        // The GameObject variables can be saved.
                        var gameObjectSharedVariables = behaviorTrees[i].gameObject.GetComponent<GameObjectSharedVariables>();
                        if (gameObjectSharedVariables != null) {
                            var variableSaveData = Save(gameObjectSharedVariables);
                            if (variableSaveData.HasValue) {
                                variableSaveDataList.Add(variableSaveData.Value);
                            }
                        }
                    }
                }
            }

            // Not all behavior trees can be saved.
            if (behaviorTrees.Length != behaviorTreeSaveCount) {
                var behaviorTreesSaveData = saveData.BehaviorTreeSaveData;
                System.Array.Resize(ref behaviorTreesSaveData, behaviorTreeSaveCount);
                saveData.BehaviorTreeSaveData = behaviorTreesSaveData;
            }

            // The scene variables can be saved.
            if ((variableSaveScope & VariableSaveScope.SceneVariables) != 0) {
                if (SceneSharedVariables.Instance != null) {
                    var variableSaveData = Save(SceneSharedVariables.Instance);
                    if (variableSaveData.HasValue) {
                        variableSaveDataList.Add(variableSaveData.Value);
                    }
                }
            }

            // The project variables can be saved.
            if ((variableSaveScope & VariableSaveScope.ProjectVariables) != 0) {
                if (ProjectSharedVariables.Instance != null) {
                    var variableSaveData = Save(ProjectSharedVariables.Instance);
                    if (variableSaveData.HasValue) {
                        variableSaveDataList.Add(variableSaveData.Value);
                    }
                }
            }

            // There has to be something to be saved.
            if (behaviorTreeSaveCount == 0 && variableSaveDataList.Count == 0) {
                return null;
            }

            // Persist the variable save data.
            if (variableSaveDataList != null && variableSaveDataList.Count > 0) {
                saveData.VariableSaveData = variableSaveDataList.ToArray();
            }

            return saveData;
        }

        /// <summary>
        /// Saves the save data from the specified behavior trees to the specified file.
        /// </summary>
        /// <param name="behaviorTrees">The behavior trees that should be saved.</param>
        /// <param name="filePath">The save data path.</param>
        /// <param name="variableSaveScope">Specifies which variables should be saved. Graph variables will automatically be saved.</param>
        /// <returns>True if at least one behavior tree's save data was saved.</returns>
        public static bool Save(BehaviorTree[] behaviorTrees, string filePath, VariableSaveScope variableSaveScope = 0)
        {
            var saveData = Save(behaviorTrees, variableSaveScope);
            if (!saveData.HasValue) {
                return false;
            }

            // Save the save data.
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
            try {
                if (!Directory.Exists(Path.GetDirectoryName(filePath))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }
                var fileStream = File.Create(filePath);
                using (var streamWriter = new StreamWriter(fileStream)) {
                    streamWriter.Write(JsonUtility.ToJson(saveData));
                }
                fileStream.Close();
            } catch (System.Exception e) {
                Debug.LogException(e);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the save data associated with the behavior tree.
        /// </summary>
        /// <param name="behaviorTree">The behavior tree that should be saved.</param>
        /// <returns>The save data associated with the behavior tree.</returns>
        private static BehaviorTreeSaveData? Save(BehaviorTree behaviorTree)
        {
            if (behaviorTree.Entity.Index == 0) {
                Debug.LogWarning($"Warning: The behavior tree {behaviorTree.name} must be active in order to be saved.", behaviorTree);
                return null;
            }

            if (behaviorTree.LogicNodes == null || behaviorTree.LogicNodes.Length == 0) {
                Debug.LogWarning($"Warning: The behavior tree {behaviorTree.name} must have tasks in order to be saved.", behaviorTree);
                return null;
            }

            // The current task status must be serialized.
            var taskComponents = behaviorTree.World.EntityManager.GetBuffer<TaskComponent>(behaviorTree.Entity);
            var serializedTaskComponents = new Serialization[taskComponents.Length];
            for (int i = 0; i < taskComponents.Length; ++i) {
                serializedTaskComponents[i] = Serialization.Serialize(taskComponents[i]);
            }

            // The branch info must be serialized.
            var branchComponents = behaviorTree.World.EntityManager.GetBuffer<BranchComponent>(behaviorTree.Entity);
            var serializedBranchComponents = new Serialization[branchComponents.Length];
            for (int i = 0; i < branchComponents.Length; ++i) {
                serializedBranchComponents[i] = Serialization.Serialize(branchComponents[i]);
            }

            // The reevaluation status must be serialized.
            Serialization[] serializedReevaluateTaskComponents = null;
            if (behaviorTree.World.EntityManager.HasBuffer<ReevaluateTaskComponent>(behaviorTree.Entity)) {
                var reevaluateTaskComponents = behaviorTree.World.EntityManager.GetBuffer<ReevaluateTaskComponent>(behaviorTree.Entity);
                serializedReevaluateTaskComponents = new Serialization[reevaluateTaskComponents.Length];
                for (int i = 0; i < reevaluateTaskComponents.Length; ++i) {
                    serializedReevaluateTaskComponents[i] = Serialization.Serialize(reevaluateTaskComponents[i]);
                }
            }

            // Each task can serialize their own data.
            var tasks = behaviorTree.LogicNodes;
            var serializedTaskData = new List<TaskSaveData>();
            for (int i = 0; i < tasks.Length; ++i) {
                if (!(tasks[i] is ISavableTask)) {
                    continue;
                }

                var saveableTask = tasks[i] as ISavableTask;
                var reflectionType = saveableTask.GetSaveReflectionType(-1);
                if (reflectionType != MemberVisibility.None) {
                    var serializedData = Serialization.Serialize(saveableTask, reflectionType, BehaviorTreeData.ValidateSerializedObject);
                    serializedTaskData.Add(new TaskSaveData() { Value = new Serialization[] { serializedData } });
                } else {
                    var taskData = saveableTask.Save(behaviorTree.World, behaviorTree.Entity);
                    if (taskData == null) {
                        continue;
                    }

                    // Tasks can contain more tasks. Serialize each task element separately.
                    if (typeof(IList).IsAssignableFrom(taskData.GetType())) {
                        var taskDataList = taskData as IList;
                        var taskSaveData = new Serialization[taskDataList.Count];
                        for (int j = 0; j < taskDataList.Count; ++j) {
                            if (taskDataList[j] is Serialization) {
                                taskSaveData[j] = taskDataList[j] as Serialization;
                            } else {
                                taskSaveData[j] = Serialization.Serialize(taskDataList[j]);
                            }
                        }

                        serializedTaskData.Add(new TaskSaveData() { Value = taskSaveData });
                    } else {
                        var serializedValue = new Serialization[] { (taskData is Serialization) ? (taskData as Serialization) : Serialization.Serialize(taskData) };
                        serializedTaskData.Add(new TaskSaveData() { Value = serializedValue });
                    }
                }
            }

            var behaviorTreeSaveData = new BehaviorTreeSaveData() {
                                                UniqueID = behaviorTree.UniqueID,
                                                TaskComponents = serializedTaskComponents,
                                                BranchComponents = serializedBranchComponents,
                                                ReevaluateTaskComponents = serializedReevaluateTaskComponents,
                                                TaskData = serializedTaskData.ToArray(),
            };
            var variableSaveData = Save(behaviorTree as ISharedVariableContainer);
            if (variableSaveData.HasValue) {
                behaviorTreeSaveData.GraphSharedVariables = variableSaveData.Value;
            }
            return behaviorTreeSaveData;
        }

        /// <summary>
        /// Returns the save data associated with the variable container.
        /// </summary>
        /// <param name="variableContainer">The variable container that should be saved.</param>
        /// <returns>The save data associated with the variable container.</returns>
        private static VariableSaveData? Save(ISharedVariableContainer variableContainer)
        {
            if (variableContainer == null) {
                return null;
            }

            var sharedVariables = variableContainer.SharedVariables;
            if (sharedVariables == null || sharedVariables.Length == 0) {
                return null;
            }

            Serialization[] serializedSharedVariablesData = null;
            if (sharedVariables != null) {
                serializedSharedVariablesData = new Serialization[sharedVariables.Length];
                for (int i = 0; i < sharedVariables.Length; ++i) {
                    serializedSharedVariablesData[i] = Serialization.Serialize(sharedVariables[i].GetValue(), BehaviorTreeData.ValidateSerializedObject);
                }
            }

            return new VariableSaveData() {
                UniqueID = variableContainer.UniqueID,
                Values = serializedSharedVariablesData,
                Scope = variableContainer.VariableScope
            };
        }

        /// <summary>
        /// Loads the save data contained within the specified file.
        /// </summary>
        /// <param name="behaviorTrees">The behavior trees that should be loaded.</param>
        /// <param name="filePath">The save data path.</param>
        /// <param name="afterVariablesRestored">Optional callback after the graph variables have been restored.</param>
        /// <returns>True if at least one behavior tree's save data was loaded.</returns>
        public static bool Load(BehaviorTree[] behaviorTrees, string filePath, Action<BehaviorTree> afterVariablesRestored = null)
        {
            if (!Application.isPlaying) {
                Debug.LogWarning($"Warning: Behavior trees can only be loaded at runtime.");
                return false;
            }

            if (!File.Exists(filePath)) {
                Debug.LogWarning($"Warning: The file at path {filePath} does not exist.");
                return false;
            }

            var fileStream = File.Open(filePath, FileMode.Open);
            var saveData = new SaveData();
            using (var streamReader = new StreamReader(fileStream)) {
                var fileData = streamReader.ReadToEnd();
                saveData = JsonUtility.FromJson<SaveData>(fileData);
            }
            fileStream.Close();

            return Load(behaviorTrees, saveData, afterVariablesRestored);
        }

        /// <summary>
        /// Loads the save data.
        /// </summary>
        /// <param name="behaviorTrees">The behavior trees that should be loaded.</param>
        /// <param name="saveData">The loaded save data.</param>
        /// <param name="afterVariablesRestored">Optional callback after the graph variables have been restored.</param>
        /// <returns>True if at least one behavior tree's save data was loaded.</returns>
        public static bool Load(BehaviorTree[] behaviorTrees, SaveData saveData, Action<BehaviorTree> afterVariablesRestored = null)
        {
            // Load the shared variables before the behavior trees so the trees can reference the variables.
            var loadedSceneVariables = false;
            var loadedProjectVariables = false;
            var loadCount = 0;
            Dictionary<int, VariableSaveData> variableSaveDataByID = null;
            if (saveData.VariableSaveData != null && saveData.VariableSaveData.Length > 0) {
                // The save data is unique to the variable container specified by the unique ID.
                for (int i = 0; i < saveData.VariableSaveData.Length; ++i) {
                    // Remember the scope to determine if the variable scope needs to be checked again when loading.
                    if (saveData.VariableSaveData[i].Scope == SharedVariable.SharingScope.GameObject) {
                        // The GameObject SharedVariables will be loaded with the behavior tree.
                        if (variableSaveDataByID == null) {
                            variableSaveDataByID = new Dictionary<int, VariableSaveData>();
                        }
                        variableSaveDataByID.Add(saveData.VariableSaveData[i].UniqueID, saveData.VariableSaveData[i]);
                    } else if (saveData.VariableSaveData[i].Scope == SharedVariable.SharingScope.Scene) {
                        // Restore the SceneSharedVariables if it hasn't already been loaded.
                        if (!loadedSceneVariables) {
                            if (SceneSharedVariables.Instance != null) {
                                if (Load(SceneSharedVariables.Instance, saveData.VariableSaveData[i])) {
                                    loadCount++;
                                }
                            }
                            loadedSceneVariables = true;
                        }
                    } else if (saveData.VariableSaveData[i].Scope == SharedVariable.SharingScope.Project) {
                        // Restore the ProjectSharedVariables if it hasn't already been loaded.
                        if (!loadedProjectVariables) {
                            if (ProjectSharedVariables.Instance != null) {
                                if (Load(ProjectSharedVariables.Instance, saveData.VariableSaveData[i])) {
                                    loadCount++;
                                }
                            }
                        }
                    }
                }
            }

            if (saveData.BehaviorTreeSaveData != null && saveData.BehaviorTreeSaveData.Length > 0) {
                // The save data is unique to the behavior tree specified by the unique ID.
                var behaviorTreeSaveDataByID = new Dictionary<int, BehaviorTreeSaveData>();
                for (int i = 0; i < saveData.BehaviorTreeSaveData.Length; ++i) {
                    behaviorTreeSaveDataByID.Add(saveData.BehaviorTreeSaveData[i].UniqueID, saveData.BehaviorTreeSaveData[i]);
                }

                // Load the save data for each behavior tree.
                for (int i = 0; i < behaviorTrees.Length; ++i) {
                    var behaviorTree = behaviorTrees[i];
                    if (behaviorTreeSaveDataByID.TryGetValue(behaviorTree.UniqueID, out var behaviorTreeSaveData)) {
                        // Restore the GameObjectSharedVariables if they haven't already been restored.
                        if (variableSaveDataByID != null) {
                            var gameObjectSharedVariables = behaviorTree.GetComponent<GameObjectSharedVariables>();
                            if (gameObjectSharedVariables != null) {
                                if (variableSaveDataByID.TryGetValue(gameObjectSharedVariables.UniqueID, out var variableSaveData)) {
                                    if (Load(gameObjectSharedVariables, variableSaveData)) {
                                        loadCount++;
                                    }
                                    // Remove the ID after it has been loaded so the variables aren't loaded again. This can happen if multiple
                                    // trees on the same GameObject reference the same GameObject variable.
                                    variableSaveDataByID.Remove(gameObjectSharedVariables.UniqueID);
                                }
                            }
                        }

                        // Restore the Graph SharedVariables.
                        if (Load(behaviorTree, behaviorTreeSaveData.GraphSharedVariables)) {
                            loadCount++;
                        }

                        // Callback after the variables have been restored.
                        afterVariablesRestored?.Invoke(behaviorTree);

                        // Populate the variables in an internal mapping for quick lookup.
                        Dictionary<VariableAssignment, SharedVariable> variableByNameMap = null;
                        BehaviorTreeData.PopulateSharedVariablesMapping(behaviorTree, ref variableByNameMap);

                        // Restore the tree after the variables have been restored.
                        if (Load(behaviorTree, behaviorTreeSaveData, afterVariablesRestored, variableByNameMap)) {
                            loadCount++;
                        }
                    }
                }
            }

            return loadCount > 0;
        }

        /// <summary>
        /// Loads the behavior tree from the specified save data.
        /// </summary>
        /// <param name="behaviorTree">The behavior tree that should be restored.</param>
        /// <param name="saveData">The save data associated with the behavior tree.</param>
        /// <param name="afterVariablesRestored">Optional callback after the graph variables have been restored.</param>
        /// <param name="variableByNameMap">A mapping between the variable name and the variable reference.</param>
        /// <returns>True if the behavior tree was successfully loaded.</returns>
        private static bool Load(BehaviorTree behaviorTree, BehaviorTreeSaveData saveData, Action<BehaviorTree> afterVariablesRestored, Dictionary<VariableAssignment, SharedVariable> variableByNameMap)
        {
            // The ID must match.
            if (behaviorTree.UniqueID != saveData.UniqueID) {
                Debug.LogError($"Error: The behavior tree {behaviorTree.name} cannot be loaded due to being saved in a different version of the behavior tree.");
                return false;
            }

            // The behavior tree must be initialized in order to be loaded.
            if (!behaviorTree.InitializeTree()) {
                return false;
            }

            // Stop the behavior tree so all tasks issue their end callback.
            var enableEntity = behaviorTree.World.EntityManager.HasComponent<EnabledTag>(behaviorTree.Entity) && behaviorTree.World.EntityManager.IsComponentEnabled<EnabledTag>(behaviorTree.Entity);
            var evaluateEntity = behaviorTree.World.EntityManager.HasComponent<EvaluationComponent>(behaviorTree.Entity) && behaviorTree.World.EntityManager.IsComponentEnabled<EvaluationComponent>(behaviorTree.Entity);
            var active = behaviorTree.IsActive();
            if (active) {
                behaviorTree.StopBehavior();
            }

            // Restore the task component status.
            var taskComponents = behaviorTree.World.EntityManager.GetBuffer<TaskComponent>(behaviorTree.Entity);
            for (int i = 0; i < saveData.TaskComponents.Length; ++i) {
                taskComponents[i] = (TaskComponent)saveData.TaskComponents[i].DeserializeFields(MemberVisibility.Public);
            }

            var branchComponents = behaviorTree.World.EntityManager.GetBuffer<BranchComponent>(behaviorTree.Entity);
            // Restore the branch info components.
            for (int i = 0; i < saveData.BranchComponents.Length; ++i) {
                branchComponents[i] = (BranchComponent)saveData.BranchComponents[i].DeserializeFields(MemberVisibility.Public);
                if (branchComponents[i].ActiveTagComponentType.TypeIndex == TypeIndex.Null) {
                    continue;
                }
                behaviorTree.World.EntityManager.SetComponentEnabled(behaviorTree.Entity, branchComponents[i].ActiveTagComponentType, true);
            }

            if (behaviorTree.World.EntityManager.HasBuffer<ReevaluateTaskComponent>(behaviorTree.Entity)) {
                var reevaluatedTaskComponents = behaviorTree.World.EntityManager.GetBuffer<ReevaluateTaskComponent>(behaviorTree.Entity);
                // Restore the reevaluated components.
                for (int i = 0; i < saveData.ReevaluateTaskComponents.Length; ++i) {
                    reevaluatedTaskComponents[i] = (ReevaluateTaskComponent)saveData.ReevaluateTaskComponents[i].DeserializeFields(MemberVisibility.Public);
                    if (reevaluatedTaskComponents[i].ReevaluateTagComponentType.TypeIndex == TypeIndex.Null) {
                        continue;
                    }
                    behaviorTree.World.EntityManager.SetComponentEnabled(behaviorTree.Entity, reevaluatedTaskComponents[i].ReevaluateTagComponentType, true);
                }
            }

            // Each task can serialize their own data.
            var tasks = behaviorTree.LogicNodes;
            var saveDataIndex = 0;
            ResizableArray<TaskAssignment> taskReferences = null;
            for (int i = 0; i < tasks.Length; ++i) {
                if (!(tasks[i] is ISavableTask)) {
                    if (tasks[i] is Task) {
                        (tasks[i] as Task).Initialize(behaviorTree, (ushort)i);
                    }
                    continue;
                }

                var taskSaveData = saveData.TaskData[saveDataIndex];
                saveDataIndex++;
                if (taskSaveData.Value == null || taskSaveData.Value.Length == 0) {
                    if (tasks[i] is Task) {
                        (tasks[i] as Task).Initialize(behaviorTree, (ushort)i);
                    }
                    continue;
                }

                var saveableTask = tasks[i] as ISavableTask;
                var reflectionType = saveableTask.GetSaveReflectionType(-1);
                if (reflectionType != MemberVisibility.None) {
                    taskSaveData.Value[0].DeserializeFields(saveableTask, saveableTask.GetSaveReflectionType(0), BehaviorTreeData.ValidateDeserializedTypeObject,
                        (object fieldInfoObj, object task, object value) =>
                        {
                            return BehaviorTreeData.ValidateDeserializedObject(fieldInfoObj, task, value, ref variableByNameMap, ref taskReferences);
                        });
                } else {
                    if (taskSaveData.Value.Length == 1) {
                        if (saveableTask.GetSaveReflectionType(0) != MemberVisibility.None) {
                            saveableTask.Load(taskSaveData.Value[0], behaviorTree.World, behaviorTree.Entity, variableByNameMap, ref taskReferences);
                        } else {
                            saveableTask.Load(taskSaveData.Value[0].DeserializeFields(MemberVisibility.Public, BehaviorTreeData.ValidateDeserializedTypeObject,
                                    (object fieldInfoObj, object task, object value) =>
                                    {
                                        return BehaviorTreeData.ValidateDeserializedObject(fieldInfoObj, task, value, ref variableByNameMap, ref taskReferences);
                                    }), behaviorTree.World, behaviorTree.Entity, variableByNameMap, ref taskReferences);
                        }
                    } else {
                        var taskData = new object[taskSaveData.Value.Length];
                        for (int j = 0; j < taskSaveData.Value.Length; ++j) {
                            if (taskSaveData.Value[j] == null) {
                                continue;
                            }

                            if (saveableTask.GetSaveReflectionType(j) != MemberVisibility.None) {
                                // The task is responsible for deserializing the fields.
                                taskData[j] = taskSaveData.Value[j];
                            } else {
                                taskData[j] = taskSaveData.Value[j].DeserializeFields(MemberVisibility.Public, BehaviorTreeData.ValidateDeserializedTypeObject,
                                    (object fieldInfoObj, object task, object value) =>
                                    {
                                        return BehaviorTreeData.ValidateDeserializedObject(fieldInfoObj, task, value, ref variableByNameMap, ref taskReferences);
                                    });
                            }
                        }
                        saveableTask.Load(taskData, behaviorTree.World, behaviorTree.Entity, variableByNameMap, ref taskReferences);
                    }
                }
                if (tasks[i] is Task) {
                    (tasks[i] as Task).Initialize(behaviorTree, (ushort)i);
                }
            }

            // After the tree has been loaded the task references need to be assigned.
            BehaviorTreeData.AssignTaskReferences(behaviorTree.LogicNodes, taskReferences);

            if (active) {
                behaviorTree.StartBehavior();
            }
            if (enableEntity) {
                behaviorTree.World.EntityManager.SetComponentEnabled<EnabledTag>(behaviorTree.Entity, true);
            }
            if (evaluateEntity) {
                behaviorTree.World.EntityManager.SetComponentEnabled<EvaluationComponent>(behaviorTree.Entity, true);
            }

            return true;
        }

        /// <summary>
        /// Loads the variable from the specified file path.
        /// </summary>
        /// <param name="variableContainer">The variable container that should be restored.</param>
        /// <param name="saveData">The save data associated with the variable container.</param>
        /// <returns>True if the variable container was successfully loaded.</returns>
        private static bool Load(ISharedVariableContainer variableContainer, VariableSaveData saveData)
        {
            // There may not be any variables saved.
            if (saveData.UniqueID == 0) {
                return false;
            }

            // The ID must match.
            if (variableContainer.UniqueID != saveData.UniqueID) {
                Debug.LogError($"Error: The variables {variableContainer} cannot be loaded due to being saved in a different version of the variable container.");
                return false;
            }

            var sharedVariables = variableContainer.SharedVariables;
            if (sharedVariables != null) {
                for (int i = 0; i < saveData.Values.Length; ++i) {
                    if (saveData.Values[i] == null) {
                        continue;
                    }

                    var sharedVariableValue = saveData.Values[i].DeserializeFields(MemberVisibility.Public);
                    sharedVariables[i].SetValue(sharedVariableValue);
                }
            }

            return true;
        }
    }
}
#endif
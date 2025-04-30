#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Tasks.Decorators
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Composites;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Provides a UtilityValueComponent implementation that returns a utility value.
    /// </summary>
    public abstract class UtilityEvaluator : DecoratorNode
    {
        [Tooltip("Should an infinite utility value be returned while the child is running?")]
        [SerializeField] protected bool m_BlockDuringExecution = true;

        public bool BlockDuringExecution { get => m_BlockDuringExecution; set => m_BlockDuringExecution = value; }

        /// <summary>
        /// Returns the utility of the decorator. The higher the utility the more likely the task will run next.
        /// </summary>
        /// <returns>The utility of the decorator.</returns>
        public abstract float GetUtilityValue();

        /// <summary>
        /// Adds the task to the behavior tree buffer.
        /// </summary>
        /// <param name="world">The world that the task runs in.</param>
        /// <param name="entity">The entity that the task is connected to.</param>
        /// <param name="behaviorTreeID">The ID of the behavior tree running the task.</param>
        /// <param name="index">The index of the task.</param>
        public override void AddBufferElement(World world, Entity entity, int behaviorTreeID, ushort index)
        {
            base.AddBufferElement(world, entity, behaviorTreeID, index);

            DynamicBuffer<UtilityValueComponent> buffer;
            if (world.EntityManager.HasBuffer<UtilityValueComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UtilityValueComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<UtilityValueComponent>(entity);
            }

            buffer.Add(new UtilityValueComponent()
            {
                Index = index,
            });
            var traversalTaskSystems = world.GetOrCreateSystemManaged<TraversalTaskSystemGroup>();
            traversalTaskSystems.AddSystemToUpdateList(world.GetOrCreateSystem(typeof(UtilityEvaluatorSystem)));
        }

        /// <summary>
        /// Clears all component buffers from the behavior tree buffer.
        /// </summary>
        /// <param name="world">The world that the task runs in.</param>
        /// <param name="entity">The entity that the task is connected to.</param>
        public override void ClearBufferElement(World world, Entity entity)
        {
            base.ClearBufferElement(world, entity);

            DynamicBuffer<UtilityValueComponent> buffer;
            if (world.EntityManager.HasBuffer<UtilityValueComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<UtilityValueComponent>(entity);
                buffer.Clear();
            }
        }

        /// <summary>
        /// Executes the task logic.
        /// </summary>
        /// <returns>The status of the task.</returns>
        public override TaskStatus OnUpdate()
        {
            var taskComponents = m_BehaviorTree.World.EntityManager.GetBuffer<TaskComponent>(m_BehaviorTree.Entity);
            var childStatus = taskComponents[RuntimeIndex + 1].Status; // Index + 1 will always be the task's only child.
            if (childStatus == TaskStatus.Success || childStatus == TaskStatus.Failure) {
                return childStatus;
            }
            return TaskStatus.Running;
        }
    }

    /// <summary>
    /// Sets the UtilityValueComponent value.
    /// </summary>
    [DisableAutoCreation]
    [UpdateBefore(typeof(UtilitySelectorTaskSystem))]
    public partial struct UtilityEvaluatorSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (utilityValueComponents, taskComponents, entity) in
                SystemAPI.Query<DynamicBuffer<UtilityValueComponent>, DynamicBuffer<TaskComponent>>().WithAll<EvaluationComponent>().WithEntityAccess()) {

                for (int i = 0; i < utilityValueComponents.Length; ++i) {
                    var utilityValueComponent = utilityValueComponents[i];
                    var utilityEvaluator = BehaviorTree.GetBehaviorTree(entity).GetTask(utilityValueComponent.Index) as UtilityEvaluator;
                    if (utilityEvaluator == null) {
                        continue;
                    }

                    // If the branch is currently active then it should return an infinite utility value until the branch has completed. This will allow the entire
                    // branch to execute.
                    if (utilityEvaluator.BlockDuringExecution) {
                        var taskComponent = taskComponents[utilityValueComponent.Index + 1];
                        if (taskComponent.Status == TaskStatus.Running) {
                            utilityValueComponent.Value = float.PositiveInfinity;
                        } else {
                            utilityValueComponent.Value = utilityEvaluator.GetUtilityValue();
                        }
                    } else {
                        utilityValueComponent.Value = utilityEvaluator.GetUtilityValue();
                    }
                    var utilityValueComponentBuffer = utilityValueComponents;
                    utilityValueComponentBuffer[i] = utilityValueComponent;
                }
            }
        }
    }
}
#endif
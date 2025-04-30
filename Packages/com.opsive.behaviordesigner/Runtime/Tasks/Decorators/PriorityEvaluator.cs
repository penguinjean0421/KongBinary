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
    using Opsive.BehaviorDesigner.Runtime.Tasks.Composites;
    using Unity.Entities;

    /// <summary>
    /// Provides a PriorityValueComponent implementation that returns a priority value.
    /// </summary>
    public abstract class PriorityEvaluator : DecoratorNode
    {
        /// <summary>
        /// Returns the priority of the decorator. The higher the priority the more likely the task will run next.
        /// </summary>
        /// <returns>The priority of the decorator.</returns>
        public abstract float GetPriorityValue();

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

            DynamicBuffer<PriorityValueComponent> buffer;
            if (world.EntityManager.HasBuffer<PriorityValueComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PriorityValueComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<PriorityValueComponent>(entity);
            }
            buffer.Add(new PriorityValueComponent()
            {
                Index = index,
            });
            var traversalTaskSystems = world.GetOrCreateSystemManaged<TraversalTaskSystemGroup>();
            traversalTaskSystems.AddSystemToUpdateList(world.GetOrCreateSystem(typeof(PriorityEvaluatorSystem)));
        }

        /// <summary>
        /// Clears all component buffers from the behavior tree buffer.
        /// </summary>
        /// <param name="world">The world that the task runs in.</param>
        /// <param name="entity">The entity that the task is connected to.</param>
        public override void ClearBufferElement(World world, Entity entity)
        {
            base.ClearBufferElement(world, entity);

            DynamicBuffer<PriorityValueComponent> buffer;
            if (world.EntityManager.HasBuffer<PriorityValueComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<PriorityValueComponent>(entity);
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
    /// Sets the PriorityValueComponent value.
    /// </summary>
    [DisableAutoCreation]
    [UpdateBefore(typeof(PrioritySelectorTaskSystem))]
    public partial struct PriorityEvaluatorSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        private void OnUpdate(ref SystemState state)
        {
            foreach (var (priorityValueComponents, taskComponents, entity) in
                SystemAPI.Query<DynamicBuffer<PriorityValueComponent>, DynamicBuffer<TaskComponent>>().WithAll<PrioritySelectorTag, EvaluationComponent>().WithEntityAccess()) {

                for (int i = 0; i < priorityValueComponents.Length; ++i) {
                    var priorityValueComponent = priorityValueComponents[i];
                    var priorityEvaluator = BehaviorTree.GetBehaviorTree(entity).GetTask(priorityValueComponent.Index) as PriorityEvaluator;
                    if (priorityEvaluator == null) {
                        continue;
                    }

                    priorityValueComponent.Value = priorityEvaluator.GetPriorityValue();
                    var priorityValueComponentBuffer = priorityValueComponents;
                    priorityValueComponentBuffer[i] = priorityValueComponent;
                }
            }
        }
    }
}
#endif
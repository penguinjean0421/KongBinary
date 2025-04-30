#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Utility
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Unity.Entities;

    /// <summary>
    /// Utility functions that are used throughout the behavior tree execution.
    /// </summary>
    public static class ComponentUtility
    {
        /// <summary>
        /// Adds the components necessary in order to trigger an interrupt.
        /// </summary>
        /// <param name="entityManager">The EntityManager that the entity belongs to.</param>
        /// <param name="entity">The entity that should have the components added..</param>
        public static void AddInterruptComponents(ref EntityManager entityManager, ref Entity entity)
        {
            entityManager.AddComponent<InterruptTag>(entity);
            entityManager.SetComponentEnabled<InterruptTag>(entity, false);
            entityManager.AddComponent<InterruptedTag>(entity);
            entityManager.SetComponentEnabled<InterruptedTag>(entity, false);
        }
    }
}
#endif
#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Components
{
    using Opsive.BehaviorDesigner.Runtime.Groups;
    using System;
    using System.Collections.Generic;
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Indicates that the behavior tree was baked.
    /// </summary>
    public class BakedBehaviorTree : IComponentData
    {
        [Tooltip("The index of the connected start task.")]
        public int StartEventConnectedIndex;
        [Tooltip("Should the behavior tree be started after it has been baked?")]
        public bool StartEvaluation;
        [Tooltip("The indicies of the reevaluate task systems.")]
        public string[] ReevaluateTaskSystems;
        [Tooltip("The indicies of the interrupt task systems.")]
        public string[] InterruptTaskSystems;
        [Tooltip("The indicies of the traversal task systems.")]
        public string[] TraversalTaskSystems;
        [Tooltip("The hashes that correspond to the TaskComponent's ComponentType.")]
        public ulong[] TagStableTypeHashes;
        [Tooltip("The hashes that correspond to the ReevaluateTaskComponent's ComponentType.")]
        public ulong[] ReevaluateTagStableTypeHashes;
    }

    /// <summary>
    /// The behavior tree has been baked. Start the tree using the baked data.
    /// </summary>
    public partial struct StartBakedBehaviorTreeSystem : ISystem
    {
        /// <summary>
        /// Restricts when the system should run.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BakedBehaviorTree>();
        }

        /// <summary>
        /// Starts the baked behavior tree.
        /// </summary>
        /// <param name="state">The current SystemState.</param>
        private void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;

            // The components are baked, but systems are not baked. Create the required systems within the current world.
            var reevaluateTaskSystemGroup = state.World.GetOrCreateSystemManaged<ReevaluateTaskSystemGroup>();
            var interruptTaskSystemGroup = state.World.GetOrCreateSystemManaged<InterruptTaskSystemGroup>();
            var traversalTaskSystemGroup = state.World.GetOrCreateSystemManaged<TraversalTaskSystemGroup>();

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (bakedBehaviorTree, entity) in SystemAPI.Query<BakedBehaviorTree>().WithEntityAccess()) {
                AddSystems(state.World, reevaluateTaskSystemGroup, bakedBehaviorTree.ReevaluateTaskSystems);
                AddSystems(state.World, interruptTaskSystemGroup, bakedBehaviorTree.InterruptTaskSystems);
                AddSystems(state.World, traversalTaskSystemGroup, bakedBehaviorTree.TraversalTaskSystems);

                // ComponentTypes cannot be serialized. Convert the StableTypeHash to a ComponentType.
                var taskComponents = state.World.EntityManager.GetBuffer<TaskComponent>(entity);
                for (int i = 0; i < taskComponents.Length; ++i) {
                    var taskComponent = taskComponents[i];
                    taskComponent.TagComponentType = ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(bakedBehaviorTree.TagStableTypeHashes[i]));
                    taskComponents[i] = taskComponent;
                }

                if (state.World.EntityManager.HasBuffer<ReevaluateTaskComponent>(entity)) {
                    var reevaluateComponents = state.World.EntityManager.GetBuffer<ReevaluateTaskComponent>(entity);
                    for (int i = 0; i < reevaluateComponents.Length; ++i) {
                        var reevaluateComponent = reevaluateComponents[i];
                        reevaluateComponent.ReevaluateTagComponentType = ComponentType.FromTypeIndex(TypeManager.GetTypeIndexFromStableTypeHash(bakedBehaviorTree.ReevaluateTagStableTypeHashes[i]));
                        reevaluateComponents[i] = reevaluateComponent;
                    }
                }

                // All of the systems have been added. Start the behavior tree.
                BehaviorTree.StartBranch(state.World, entity, (ushort)bakedBehaviorTree.StartEventConnectedIndex, bakedBehaviorTree.StartEvaluation);
                ecb.RemoveComponent<BakedBehaviorTree>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        /// <summary>
        /// Adds the systems indicated by the SystemTypeIndex to the specified group.
        /// </summary>
        /// <param name="world">The current World.</param>
        /// <param name="group">The group that the systems should be added to.</param>
        /// <param name="systemTypes">The types of systems that should be added.</param>
        private void AddSystems(World world, ComponentSystemGroup group, string[] systemTypes)
        {
            if (systemTypes == null) { return; }

            for (int i = 0; i < systemTypes.Length; ++i) {
                group.AddSystemToUpdateList(world.GetOrCreateSystem(Shared.Utility.TypeUtility.GetType(systemTypes[i])));
            }
        }
    }
}
#endif
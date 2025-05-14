#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Runtime.Groups
{
    using Opsive.BehaviorDesigner.Runtime.Systems;
    using Unity.Entities;

    /// <summary>
    /// Main group for the systems that are responsible for traversing the behavior tree.
    /// </summary>
    [UpdateInGroup(typeof(BehaviorTreeSystemGroup))]
    [UpdateAfter(typeof(InterruptSystemGroup))]
    public partial class TraversalSystemGroup : ComponentSystemGroup
    {
        private SystemHandle m_EvaluationSystemHandle;
        private SystemHandle m_DetermineEvaluationSystemHandle;

        /// <summary>
        /// The group has been created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            m_EvaluationSystemHandle = World.GetExistingSystem<EvaluationSystem>();
            m_DetermineEvaluationSystemHandle = World.GetExistingSystem<DetermineEvaluationSystem>();
        }

        /// <summary>
        /// Updates the systems. Determines if the systems need to keep being evaluated.
        /// </summary>
        protected override void OnUpdate()
        {
#if UNITY_EDITOR
            var count = 0;
#endif
            bool evaluate;
            do {
                base.OnUpdate();

                var determineEvaluationSystem = EntityManager.WorldUnmanaged.GetUnsafeSystemRef<DetermineEvaluationSystem>(m_DetermineEvaluationSystemHandle);
                determineEvaluationSystem.Complete(EntityManager);
                evaluate = determineEvaluationSystem.Evaluate;

                var evaluationSystem = EntityManager.WorldUnmanaged.GetUnsafeSystemRef<EvaluationSystem>(m_EvaluationSystemHandle);
                evaluationSystem.Complete(EntityManager);

#if UNITY_EDITOR
                if (evaluate) {
                    count++;
                    if (count == ushort.MaxValue / 10) {
                        UnityEngine.Debug.LogWarning("An infinite loop would have been caused by the TraversalSystemGroup. Please email support@opsive.com with steps to reproduce this error.");
                        break;
                    }
                }
#endif
            } while (evaluate);
        }

        /// <summary>
        /// The group has stopped running.
        /// </summary>
        protected override void OnStopRunning()
        {
            base.OnStopRunning();

            var evaluationSystem = EntityManager.WorldUnmanaged.GetUnsafeSystemRef<EvaluationSystem>(m_EvaluationSystemHandle);
            evaluationSystem.Complete(EntityManager, true);
        }
    }
}
#endif
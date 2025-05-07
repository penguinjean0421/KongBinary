/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Components;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.GraphDesigner.Runtime;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;
    using Unity.Transforms;
    using UnityEngine;

    [NodeDescription("Uses DOTS to rotate around the center. This task will always return a status of running.")]
    public struct RotateTowardsEntity : ILogicNode, ITaskComponentData, IAction
    {
        [Tooltip("The index of the node.")]
        [SerializeField] ushort m_Index;
        [Tooltip("The parent index of the node. ushort.MaxValue indicates no parent.")]
        [SerializeField] ushort m_ParentIndex;
        [Tooltip("The sibling index of the node. ushort.MaxValue indicates no sibling.")]
        [SerializeField] ushort m_SiblingIndex;
        [Tooltip("The angular speed of the agent.")]
        [SerializeField] float m_AngularSpeed;

        public ushort Index { get => m_Index; set => m_Index = value; }
        public ushort ParentIndex { get => m_ParentIndex; set => m_ParentIndex = value; }
        public ushort SiblingIndex { get => m_SiblingIndex; set => m_SiblingIndex = value; }
        public ushort RuntimeIndex { get; set; }

        public ComponentType Tag { get => typeof(RotateTowardsEntityTag); }
        public System.Type SystemType { get => typeof(RotateTowardsEntityTaskSystem); }

        /// <summary>
        /// Resets the task to its default values.
        /// </summary>
        public void Reset() { m_AngularSpeed = 2; }

        /// <summary>
        /// Adds the IBufferElementData to the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be assigned to.</param>
        public void AddBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RotateTowardsEntityComponent> buffer;
            if (world.EntityManager.HasBuffer<RotateTowardsEntityComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RotateTowardsEntityComponent>(entity);
            } else {
                buffer = world.EntityManager.AddBuffer<RotateTowardsEntityComponent>(entity);
            }

            buffer.Add(new RotateTowardsEntityComponent()
            {
                Index = RuntimeIndex,
                AngularSpeed = m_AngularSpeed,
            });
        }

        /// <summary>
        /// Clears the IBufferElementData from the entity.
        /// </summary>
        /// <param name="world">The world that the entity exists.</param>
        /// <param name="entity">The entity that the IBufferElementData should be cleared from.</param>
        public void ClearBufferElement(World world, Entity entity)
        {
            DynamicBuffer<RotateTowardsEntityComponent> buffer;
            if (world.EntityManager.HasBuffer<RotateTowardsEntityComponent>(entity)) {
                buffer = world.EntityManager.GetBuffer<RotateTowardsEntityComponent>(entity);
                buffer.Clear();
            }
        }
    }

    /// <summary>
    /// The DOTS data structure for the RotateTowardsEntity struct.
    /// </summary>
    public struct RotateTowardsEntityComponent : IBufferElementData
    {
        [Tooltip("The index of the node.")]
        public ushort Index;
        [Tooltip("The angular speed of the agent.")]
        public float AngularSpeed;
    }

    /// <summary>
    /// A DOTS tag indicating when a RotateTowardsEntity node is active.
    /// </summary>
    public struct RotateTowardsEntityTag : IComponentData, IEnableableComponent { }

    /// <summary>
    /// Runs the RotateTowardsEntity logic.
    /// </summary>
    [DisableAutoCreation]
    public partial struct RotateTowardsEntityTaskSystem : ISystem
    {
        /// <summary>
        /// Updates the logic.
        /// </summary>
        /// <param name="state">The current state of the system.</param>
        [BurstCompile]
        private void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (localTransform, taskComponents, rotateTorwardsTargetComponents) in
                SystemAPI.Query<RefRW<LocalTransform>, DynamicBuffer<TaskComponent>, DynamicBuffer<RotateTowardsEntityComponent>>().WithAll<RotateTowardsEntityTag, EvaluationComponent>()) {
                for (int i = 0; i < rotateTorwardsTargetComponents.Length; ++i) {
                    var rotateTowardsEntityComponent = rotateTorwardsTargetComponents[i];
                    var taskComponent = taskComponents[rotateTowardsEntityComponent.Index];
                    if (taskComponent.Status == TaskStatus.Queued) {
                        taskComponent.Status = TaskStatus.Running;

                        var taskComponentBuffer = taskComponents;
                        taskComponentBuffer[rotateTowardsEntityComponent.Index] = taskComponent;
                    }

                    if (taskComponent.Status != TaskStatus.Running) {
                        continue;
                    }

                    // Find the target. There will only be one entity with the TargetEntityTag.
                    foreach (var (targetTransform, targetEntity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<TargetEntityTag>().WithEntityAccess()) {
                        var target = quaternion.EulerXYZ(0, -GetAngle(targetTransform.ValueRO.Position), 0);
                        localTransform.ValueRW.Rotation = RotateTowards(localTransform.ValueRO.Rotation, target, rotateTowardsEntityComponent.AngularSpeed * deltaTime);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the angle between the target point and the center.
        /// </summary>
        /// <param name="target">The target point.</param>
        /// <returns>The angle between the target point and the center. This will be in the range of 0 - 2PI (radians).</returns>
        [BurstCompile]
        private float GetAngle(float3 target)
        {
            var n = 180 - (math.atan2(-target.x, -target.z)) * 180 / math.PI;
            return (n % 360) * math.TORADIANS;
        }

        /// <summary>
        /// Rotates the specified target.
        /// </summary>
        /// <param name="from">The original rotation.</param>
        /// <param name="to">The target rotation.</param>
        /// <param name="maxDelta">The maximum delta amount.</param>
        /// <returns>The computed rotation.</returns>
        private quaternion RotateTowards(quaternion from, quaternion to, float maxDelta)
        {
            var angle = math.angle(from, to);
            if (angle <= 0) {
                return to;
            }
            return math.slerp(from, to, math.clamp(maxDelta / angle, 0, 1));
        }
    }
}
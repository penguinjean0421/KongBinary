/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Unity.Entities;
    using UnityEngine;

    /// <summary>
    /// Authoring component for the entity that is the part of the turret.
    /// </summary>
    public class TurretIdentifier : MonoBehaviour
    {
        /// <summary>
        /// Specifies the section of the turret.
        /// </summary>
        protected enum TurretObjectType
        {
            Base,   // The base of the turret.
            Mid,    // The mid-section of the turret.
            Weapon  // The turret weapon.
        }
        [Tooltip("Specifies the section of the turret.")]
        [SerializeField] protected TurretObjectType m_ObjectType;
        [Tooltip("The speed of the recoil pullback.")]
        [SerializeField] protected float m_TurretRecoilSpeed = 12f;
        [Tooltip("The target z position of the recoil pullback.")]
        [SerializeField] protected float m_TurretRecoilPosition = -0.7f;

        /// <summary>
        /// Bakes the turret base data.
        /// </summary>
        private class Baker : Baker<TurretIdentifier>
        {
            /// <summary>
            /// Bakes the data.
            /// </summary>
            /// <param name="authoring">The parent authoring component.</param>
            public override void Bake(TurretIdentifier authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.m_ObjectType == TurretObjectType.Base) {
                    AddComponent<TurretBaseTag>(entity, new TurretBaseTag() { });
                } else if (authoring.m_ObjectType == TurretObjectType.Mid) {
                    AddComponent<TurretTag>(entity, new TurretTag() { });
                } else { // Weapon.
                    AddComponent<TurretRecoil>(entity, new TurretRecoil() { 
                        Pullback = true,
                        PullbackSpeed = authoring.m_TurretRecoilSpeed, 
                        PullbackPosition = authoring.m_TurretRecoilPosition });
                    SetComponentEnabled<TurretRecoil>(entity, false);
                }
            }
        }
    }
}
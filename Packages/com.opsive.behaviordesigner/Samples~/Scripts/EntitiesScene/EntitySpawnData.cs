/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// ScriptableObject containing the spawn data for the Entity scene.
    /// </summary>
    public class EntitySpawnData : ScriptableObject
    {
        [Tooltip("The prefab reference object.")]
        [SerializeField] protected GameObject m_Prefab;
        [Tooltip("The spawn count when the scene starts.")]
        [SerializeField] protected int m_InitialSpawnCount;
        [Tooltip("The spawn count when the add button is pressed.")]
        [SerializeField] protected int m_AdditiveSpawnCount;
        [Tooltip("The minimum radius that the entities should be spawned.")]
        [SerializeField] protected int m_MinRadius = 30;
        [Tooltip("The maximum radius that the entities should be spawned.")]
        [SerializeField] protected int m_MaxRadius = 80;
        [Tooltip("The minium speed that the entities should rotate.")]
        [SerializeField] protected Vector3 m_MinRotationSpeed;
        [Tooltip("The maximum speed that the entities should rotate.")]
        [SerializeField] protected Vector3 m_MaxRotationSpeed;

        public GameObject Prefab => m_Prefab;
        public int InitialSpawnCount => m_InitialSpawnCount;
        public int AdditiveSpawnCount => m_AdditiveSpawnCount;
        public int MinRadius => m_MinRadius;
        public int MaxRadius => m_MaxRadius;
        public Vector3 MinRotationSpeed => m_MaxRotationSpeed;
        public Vector3 MaxRotationSpeed => m_MaxRotationSpeed;
    }
}
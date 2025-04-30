/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using UnityEngine;

    /// <summary>
    /// Changes the material color of the muzzle flash.
    /// </summary>
    public class MuzzleFlash : MonoBehaviour
    {
        [Tooltip("The speed at which the muzzle flash should be decreased.")]
        [SerializeField] protected float m_Speed = 2;

        private Material m_Material;

        /// <summary>
        /// Initializes the default colors.
        /// </summary>
        private void Awake()
        {
            m_Material = GetComponent<Renderer>().material;
        }

        /// <summary>
        /// Updates the muzzle flash color.
        /// </summary>
        public void Update()
        {
            var color = m_Material.GetColor("_TintColor");
            color.a = Mathf.MoveTowards(color.a, 0, m_Speed * Time.deltaTime);
            m_Material.SetColor("_TintColor", color);
            if (color.a == 0) {
                Destroy(gameObject);
            }
        }

    }
}
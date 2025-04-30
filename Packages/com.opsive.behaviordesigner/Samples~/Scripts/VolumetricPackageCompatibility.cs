/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.Shared.Utility;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Ensures compatibility with or without the volumetric light package from https://github.com/CristianQiu/Unity-URP-Volumetric-Light.
    /// </summary>
    public class VolumetricPackageCompatibility : MonoBehaviour
    {
        [Tooltip("A reference to the volumetric profile.")]
        [SerializeField] protected ScriptableObject m_VolumetricProfile;
        [Tooltip("The volumetric light anisotropy value.")]
        [SerializeField] protected float m_LightAnisotropy;
        [Tooltip("The volumetric light scattering value.")]
        [SerializeField] protected float m_LightScattering;
        [Tooltip("The volumetric light radius value.")]
        [SerializeField] protected float m_LightRadius; //0 1.72 .469

        /// <summary>
        /// Sets the volumetric light settings if the package exists.
        /// </summary>
        private void Start()
        {
            var additionalLightType = TypeUtility.GetType("VolumetricAdditionalLight");
            if (additionalLightType == null) {
                Destroy(gameObject);
                return;
            }

            var volumeType = TypeUtility.GetType("UnityEngine.Rendering.Volume");
            if (volumeType == null) {
                return;
            }

            var volume = UnityEngine.Object.FindFirstObjectByType(volumeType);
            if (volume == null) {
                return;
            }

            var profileProperty = volumeType.GetProperty("profile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (profileProperty == null) {
                return;
            }

            profileProperty.SetValue(volume, m_VolumetricProfile);

            // All of the behavior tree agents with flashlights were spawned within Awake.
            var volumetricLightObjects = Object.FindObjectsByType<VolumetricFlashlightIdentifier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < volumetricLightObjects.Length; ++i) {
                SetupVolumetricLight(volumetricLightObjects[i].gameObject, additionalLightType);
            }

            Destroy(gameObject);
        }

        /// <summary>
        /// Adds the Volumetric Light component.
        /// </summary>
        /// <param name="lightGameObject">A reference to the light.</param>
        /// <param name="additionalLightType">The component type.</param>
        private void SetupVolumetricLight(GameObject lightGameObject, System.Type additionalLightType)
        {
            var lightComponent = lightGameObject.AddComponent(additionalLightType);
            var property = additionalLightType.GetProperty("Anisotropy", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) {
                property.SetValue(lightComponent, m_LightAnisotropy);
            }
            property = additionalLightType.GetProperty("Scattering", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) {
                property.SetValue(lightComponent, m_LightScattering);
            }
            property = additionalLightType.GetProperty("Radius", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null) {
                property.SetValue(lightComponent, m_LightRadius);
            }
        }
    }
}
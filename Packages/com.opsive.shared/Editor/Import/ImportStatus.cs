/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.Shared.Editor.Import
{
    using UnityEngine;

    /// <summary>
    /// Small ScriptableObject which shows the import window if it has not been shown.
    /// </summary>
    public class ImportStatus : ScriptableObject
    {
        [Tooltip("Has the Character Controller Update Project Settings window been shown?")]
        [SerializeField] protected bool m_CharacterProjectSettingsShown;
        [Tooltip("Has the Behavior Designer Welcome window been shown?")]
        [SerializeField] protected bool m_BehaviorWindowShown;

        public bool CharacterProjectSettingsShown { get { return m_CharacterProjectSettingsShown; } set { m_CharacterProjectSettingsShown = value; } }
        public bool BehaviorWindowShown { get { return m_BehaviorWindowShown; } set { m_BehaviorWindowShown = value; } }
    }
}
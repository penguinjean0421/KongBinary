/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples.SceneManagers
{
    using Opsive.BehaviorDesigner.Runtime;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Switches to the next scene after a random amount of time.
    /// </summary>
    public class PersistentScenesManager : MonoBehaviour
    {
        [Tooltip("A reference to the behavior tree agent prefab.")]
        [SerializeField] protected GameObject m_Agent;
        [Tooltip("The waypoints within the scene.")]
        [SerializeField] protected GameObject[] m_Waypoints;
        [Tooltip("The name of the next scene.")]
        [SerializeField] protected string m_NextSceneName;
        [Tooltip("The minimum seconds for changing to the next scene.")]
        [SerializeField] protected int m_MinimumNextSceneChangeRange = 5;
        [Tooltip("The maximum seconds for changing to the next scene.")]
        [SerializeField] protected int m_MaximumNextSceneChangeRange = 9;
        [Tooltip("A reference to the visual countdown until the scene is changed.")]
        [SerializeField] protected UnityEngine.UI.Text m_CountdownText;

        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public void Awake()
        {
#if UNITY_2022
            var behaviorTree = Object.FindObjectOfType<BehaviorTree>();
#else
            var behaviorTree = Object.FindFirstObjectByType<BehaviorTree>();
#endif
            // The behavior tree will be found if it has already been marked as DontDestroyOnLoad.
            if (behaviorTree == null) {
                var agent = Object.Instantiate(m_Agent);
                behaviorTree = agent.GetComponent<BehaviorTree>();
                DontDestroyOnLoad(agent);
            }

            // Update the waypoints to the new scene. The behavior tree will stay active across scene changes.
            var waypoints = behaviorTree.GetVariable<GameObject[]>("Waypoints");
            if (waypoints == null) {
                Debug.LogError("Error: Unable to find the Waypoints variable.");
                return;
            }
            waypoints.Value = m_Waypoints;

            StartCoroutine(LoadNextScene());
        }

        /// <summary>
        /// Loads the next scene.
        /// </summary>
        private IEnumerator LoadNextScene()
        {
            var duration = Random.Range(m_MinimumNextSceneChangeRange, m_MaximumNextSceneChangeRange);
            var startTime = Time.time;
            while (startTime + duration > Time.time) {
                m_CountdownText.text = $"Changing scenes in {Mathf.RoundToInt(startTime + duration - Time.time)} seconds";
                yield return new WaitForEndOfFrame();
            }
            m_CountdownText.text = "Changing scenes";

            SceneManager.LoadScene(m_NextSceneName);
        }
    }
}
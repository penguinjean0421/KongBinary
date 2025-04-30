/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Samples
{
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.BehaviorDesigner.Runtime.Tasks.Actions;
    using Opsive.BehaviorDesigner.Samples.SceneManagers;
    using UnityEngine;

    /// <summary>
    /// Restarts the Hide and Seek scene.
    /// </summary>
    public class RestartHideSeekScene : Action
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns>Success after the scene has been restarted.</returns>
        public override TaskStatus OnUpdate()
        {
#if UNITY_2022
            var hideSeekSceneManager = Object.FindObjectOfType<HideSeekSceneManager>();
#else
            var hideSeekSceneManager = Object.FindFirstObjectByType<HideSeekSceneManager>();
#endif

            hideSeekSceneManager.Restart();

            return TaskStatus.Success;
        }
    }
}
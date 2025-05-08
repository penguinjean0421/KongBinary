#if GRAPH_DESIGNER
/// ---------------------------------------------
/// Behavior Designer
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------
namespace Opsive.BehaviorDesigner.Editor.Controls.NodeViews
{
    using Opsive.GraphDesigner.Editor;
    using Opsive.BehaviorDesigner.Runtime.Tasks;
    using Opsive.Shared.Editor.UIElements.Controls;
    using UnityEngine.UIElements;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Implements TypeControlBase for the StackedTask type.
    /// </summary>
    [ControlType(typeof(StackedTask))]
    public class StackedTaskNodeViewControl : TaskNodeViewControl
    {
        private const float c_ActiveIconRotationSpeed = 30;

        /// <summary>
        /// Displays information about the nested task within the Stacked Task.
        /// </summary>
        private class TaskView : VisualElement
        {
            private const string c_DarkActiveIconGUID = "1230b934cbd748345b13125468a34720";
            private const string c_LightActiveIconGUID = "e57f179ee476f274dbe537179e67bf04";

            private int m_Index;
            private Image m_ActiveImage;

            /// <summary>
            /// TaskView constructor.
            /// </summary>
            /// <param name="index">The index of the task.</param>
            /// <param name="task">A reference to the task.</param>
            public TaskView(int index, Task task)
            {
                m_Index = index;

                var horizontalLayout = new VisualElement();
                horizontalLayout.AddToClassList("horizontal-layout");
                horizontalLayout.style.height = 18;
                var label = new Label(task.ToString());
                label.style.flexGrow = 1;
                horizontalLayout.Add(label);
                m_ActiveImage = new Image();
                m_ActiveImage.image = Shared.Editor.Utility.EditorUtility.LoadAsset<Texture>(EditorGUIUtility.isProSkin ? c_DarkActiveIconGUID : c_LightActiveIconGUID);
                m_ActiveImage.style.width = 16;
                m_ActiveImage.style.height = 16;
                m_ActiveImage.style.display = DisplayStyle.None;
                horizontalLayout.Add(m_ActiveImage);

                Add(horizontalLayout);
            }

            /// <summary>
            /// Updates the status of the task.
            /// </summary>
            /// <param name="activeIndex">The index that is active.</param>
            public void UpdateStatus(int activeIndex)
            {
                m_ActiveImage.style.display = (m_Index == activeIndex ? DisplayStyle.Flex : DisplayStyle.None);
                if (m_Index == activeIndex) {
                    if (Application.isPlaying) {
                        var eulerAngles = m_ActiveImage.transform.rotation.eulerAngles;
                        m_ActiveImage.transform.rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z + c_ActiveIconRotationSpeed);
                    }
                } else {
                    m_ActiveImage.transform.rotation = Quaternion.identity;
                }
            }
        }

        private StackedTask m_StackedTask;
        private TaskView[] m_TaskViews;

        /// <summary>
        /// Addes the UIElements for the specified runtime node to the editor Node within the graph.
        /// </summary>
        /// <param name="graphWindow">A reference to the GraphWindow.</param>
        /// <param name="parent">The parent UIElement that should contain the node UIElements.</param>
        /// <param name="node">The node that the control represents.</param>
        public override void AddNodeView(GraphWindow graphWindow, VisualElement parent, object node)
        {
            base.AddNodeView(graphWindow, parent, node);

            m_StackedTask = node as StackedTask;
            if (m_StackedTask.Tasks == null) {
                return;
            }

            var tasks = m_StackedTask.Tasks;
            m_TaskViews = new TaskView[tasks.Length];
            for (int i = 0; i < tasks.Length; ++i) {
                var task = m_StackedTask.Tasks[i];
                // The task no longer exists. Replace it.
                if (task == null) {
                    tasks[i] = new UnknownTask();
                    m_StackedTask.Tasks = tasks;
                }
                m_TaskViews[i] = new TaskView(i, m_StackedTask.Tasks[i]);
                parent.Add(m_TaskViews[i]);
            }
        }

        /// <summary>
        /// Internal method which updates the node with the current execution status.
        /// </summary>
        /// <returns>The status of the task.</returns>
        protected override TaskStatus UpdateNodeInternal()
        {
            var activeIndex = -1;
            TaskStatus status;
            if ((status = base.UpdateNodeInternal()) == TaskStatus.Running && m_StackedTask.Tasks.Length > 1) {
                activeIndex = m_StackedTask.ActiveIndex;
                for (int i = 0; i < m_TaskViews.Length; ++i) {
                    m_TaskViews[i].UpdateStatus(activeIndex);
                }
            } else {
                for (int i = 0; i < m_TaskViews.Length; ++i) {
                    m_TaskViews[i].UpdateStatus(activeIndex);
                }
            }
            return status;
        }
    }
}
#endif
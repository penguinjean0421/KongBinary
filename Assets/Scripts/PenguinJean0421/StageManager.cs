using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    public int stageIndex; // 1부터 시작
    public Button stageButton;

    void Start()
    {
        bool unlocked = (stageIndex == 1) || StageData.Instance.IsStageCleared(stageIndex - 1);
        stageButton.interactable = unlocked;
    }

    public void LoadStage()
    {
        StageData.Instance.currentStageIndex = stageIndex;
        SceneManager.LoadScene($"Stage{stageIndex}");
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    public int stageIndex;
    Button stageButton;

    void Awake()
    {
        stageButton = GetComponent<Button>();
    }

    void Start()
    {
        bool unlocked = (stageIndex == 0) || StageData.Instance.IsStageCleared(stageIndex - 1);
        stageButton.interactable = unlocked;
    }

    public void LoadStage()
    {
        StageData.Instance.currentStageIndex = stageIndex;
        SceneManager.LoadScene($"Level{stageIndex}");

        /* 씬 이름 아래 처럼 해주세요 (튜토리얼이 0번)
        SceneManager.LoadScene($"Stage{stageIndex}"); 
        */
    }

    public void ClearTuto()
    {
        StageData.Instance.currentStageIndex = stageIndex;
        StageData.Instance.SetStageCleared(stageIndex);
        StageData.Instance.IsStageCleared(stageIndex);
        SceneManager.LoadScene("ChooseStage");
    }
}
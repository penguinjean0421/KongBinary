using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StageManager : MonoBehaviour
{
    public int stageIndex; // 0(튜토리얼) 부터 시작
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

        /* 아래 처럼 해주세요
        SceneManager.LoadScene($"Stage{stageIndex}"); 
        */
    }

    // 다음 스테이지 로드
    public void LoadNextStage()
    {
        StageData.Instance.currentStageIndex++;
        SceneManager.LoadScene($"Level{StageData.Instance.currentStageIndex}");
    }
}
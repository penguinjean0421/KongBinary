using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public int stageIndex; // 0(튜토리얼) 부터 시작
    public int unlockBossStar = 10; // 보스 해금 별
    int getStars;
    Button stageButton;

    void Awake()
    {
        stageButton = GetComponent<Button>();
    }

    void Start()
    {
        for (int i = 1; i < StageData.Instance.maxStage; i++)
        {
            getStars += PlayerPrefs.GetInt($"Stage{i}'s Star");
        }

        if (stageIndex != 0 && stageIndex % 5 == 0)
        {
            bool isBossUnlocked = StageData.Instance.IsStageCleared(stageIndex - 1) && (getStars > unlockBossStar);
            stageButton.interactable = isBossUnlocked;
        }
        else
        {
            bool isUnlocked = (stageIndex == 0) || StageData.Instance.IsStageCleared(stageIndex - 1);
            stageButton.interactable = isUnlocked;
        }


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
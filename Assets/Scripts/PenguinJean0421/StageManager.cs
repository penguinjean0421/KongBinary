using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    public int stageIndex; // 0(튜토리얼) 부터 시작
    int unlockBossStar = 8; // 보스 해금 별
    int getStars;
    Button stageButton;
    Text starText;

    void Awake()
    {
        stageButton = GetComponent<Button>();
        starText = GameObject.Find($"Stage{stageIndex}Star").GetComponent<Text>();
    }

    void Start()
    {
        for (int i = 1; i < StageData.Instance.maxStage; i++)
        {
            getStars += (int)PlayerPrefs.GetFloat($"Stage{i}'s Star");
        }

        if (stageIndex % 5 == 0)
        {
            bool isBossUnlocked = StageData.Instance.IsStageCleared(stageIndex - 1) && (getStars > unlockBossStar);
            stageButton.interactable = isBossUnlocked;
        }
        else
        {
            bool isUnlocked = (stageIndex == 1) || StageData.Instance.IsStageCleared(stageIndex - 1);
            stageButton.interactable = isUnlocked;
        }

        starText.text = $"{(int)PlayerPrefs.GetFloat($"Stage{stageIndex}'s Star")}/3";
    }

    public void LoadStage()
    {
        StageData.Instance.currentStageIndex = stageIndex;
        SceneManager.LoadScene($"Level_{stageIndex}");
    }
}
using UnityEngine;

public class StageData : MonoBehaviour
{
    public static StageData Instance { get; private set; }
    public int currentStageIndex { get; internal set; }

    void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 현재 진행중인 스테이지를 인덱스에 저장함
    public void SetCurrentStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
    }

    // 특정 스테이지 클리어시 처리
    public bool IsStageCleared(int stageIndex)
    {
        return PlayerPrefs.GetInt($"Stage{stageIndex}Clear", 0) == 1;
    }

    // 모든 클리어 데이터 초기화 (디버깅용).
    public void ResetAllStageData(int maxStage)
    {
        for (int i = 1; i <= maxStage; i++)
        {
            PlayerPrefs.DeleteKey($"Stage{i}Clear");
        }
        PlayerPrefs.Save();
    }
}

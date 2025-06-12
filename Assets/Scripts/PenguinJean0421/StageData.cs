using UnityEngine;

public class StageData : MonoBehaviour
{
    public static StageData Instance { get; private set; }
    public int currentStageIndex { get; internal set; }
    internal int maxStage = 10000; // 스테이지 초기화
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ResetAllStageData(maxStage);
        }
    }

    // 현재 진행 중인 스테이지 인덱스를 설정합니다.
    public void SetCurrentStage(int stageIndex)
    {
        currentStageIndex = stageIndex;
    }

    // 특정 스테이지를 클리어 처리합니다.
    public void SetStageCleared(int stageIndex)
    {
        PlayerPrefs.SetInt($"Stage{stageIndex}Clear", 1);
        PlayerPrefs.Save();
    }


    // 특정 스테이지가 클리어되었는지 확인합니다.
    public bool IsStageCleared(int stageIndex)
    {
        return PlayerPrefs.GetInt($"Stage{stageIndex}Clear", 0) == 1;
    }

    // 모든 클리어 데이터 초기화 (디버깅용).
    public void ResetAllStageData(int maxStage)
    {
        Debug.Log("지워요");
        for (int i = 0; i <= maxStage; i++)
        {
            PlayerPrefs.DeleteKey($"Stage{i}Clear");
        }
        PlayerPrefs.Save();
    }
}

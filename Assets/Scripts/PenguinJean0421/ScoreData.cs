using UnityEditor;
using UnityEngine;

public class ScoreData : MonoBehaviour
{
    public static ScoreData Instance { get; private set; } // 싱글톤
    int maxStage = 10000;

    void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ResetAllStageData(maxStage);
        }
    }

    // 특정 스테이지의 별 갯수 저장
    public void GetStars(int stageIndex, float star)
    {
        PlayerPrefs.SetFloat($"Stage{stageIndex}'s Star", star);
        PlayerPrefs.Save();
    }

    // 모든 클리어 데이터 초기화 (디버깅용).
    public void ResetAllStageData(int maxStage)
    {
        Debug.Log("지워요");
        for (int i = 1; i <= maxStage; i++)
        {
            PlayerPrefs.DeleteKey($"Stage{i}'s Star");
        }
        PlayerPrefs.Save();
    }
}
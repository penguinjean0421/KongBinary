using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤
    StageData instance;
    [SerializeField] private float timeLimit = 240f; // 4분(240초)
    private float timeRemaining;
    [SerializeField] private Text timerText; // 타이머 UI 텍스트

    private bool isGameOver = false;
    private int currentStage = 1; // 현재 스테이지 번호 (예시)

    private float sales;

    private int winning; // 클리어 조건 체크 테스트용

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetTimer(); // 게임 시작 시 타이머 초기화

        winning = Random.Range(0, 100);
    }

    void Update()
    {
        if (isGameOver) return;

        // 타이머 감소
        timeRemaining -= Time.deltaTime;

        // UI 업데이트
        UpdateTimerUI();

        // 시간 종료 처리
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            GameOver();
        }
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over! Time's up!");
        // 게임 오버 처리 (예: 씬 전환)
        // SceneManager.LoadScene("GameOver");

        // 조건 맞춰서 스테이지 클리어 or 실패 판정하게 하면 될거 같습니다.
        if (winning > 50)
        { OnStageClear(); }
        else
        { OnStageFail(); }
    }

    // 타이머 초기화 메서드
    public void ResetTimer()
    {
        timeRemaining = timeLimit;
        isGameOver = false; // 게임 오버 상태도 초기화
        UpdateTimerUI();
    }

    // 다음 스테이지로 이동
    public void GoToNextStage()
    {
        currentStage++;
        ResetTimer(); // 타이머 초기화
        Debug.Log($"Moving to Stage {currentStage}");

        // 다음 스테이지 씬 로드 (예: "Stage2", "Stage3" 등)
        string nextSceneName = $"Stage{currentStage}";
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("No more stages! Game Cleared!");
            // 모든 스테이지 클리어 시 처리
            SceneManager.LoadScene("GameClear");
        }
    }

    // 시간 연장 아이템 등으로 호출 가능
    public void AddTime(float additionalTime)
    {
        timeRemaining += additionalTime;
        if (timeRemaining > timeLimit) timeRemaining = timeLimit;
        UpdateTimerUI();
    }

    // 매출 증가 메서드
    public void AddSales(float amount)
    {
        sales += amount;
        Debug.Log($"매출 증가: {amount}, 총 매출: {sales}");
    }

    #region  스테이지 클리어 체크
    // 스테이지 클리어
    public void OnStageClear()
    {
        int current = StageData.Instance.currentStageIndex;
        StageData.Instance.IsStageCleared(current);
        SceneManager.LoadScene("Score");
    }

    // 클리어 실패
    public void OnStageFail()
    {
        SceneManager.LoadScene("Score");
    }
    #endregion
}
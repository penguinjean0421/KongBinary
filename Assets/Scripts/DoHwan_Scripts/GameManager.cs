using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤
    [SerializeField] private float timeLimit = 240f; // 4분(240초)
    private float timeRemaining;
    [SerializeField] private Text timerText; // 타이머 UI 텍스트

    private bool isGameOver = false;
    private int currentStage = 1; // 현재 스테이지 번호 (예시)

    private float sales;
    [SerializeField] float star1Sale;
    [SerializeField] float star2Sale;
    [SerializeField] float star3Sale;

    [SerializeField] GameObject scoreUI;
    // [SerializeField] GameObject succedUI;
    // [SerializeField] GameObject failUI;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetTimer(); // 게임 시작 시 타이머 초기화
        sales = 0; // 판매액 초기화

        scoreUI.SetActive(false);
        // succedUI.SetActive(false);
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
        scoreUI.SetActive(true);
        // 게임 오버 처리 (예: 씬 전환)
        // SceneManager.LoadScene("GameOver");
        if (sales >= star1Sale)
        {
            if (sales >= star3Sale)
            {
                Debug.Log("별 3개");
            }
            else if (sales >= star2Sale)
            {
                Debug.Log("별 2개");
            }
            else
            {
                Debug.Log("별 1개");
            }
            OnStageClear();
        }
        else
        {
            Debug.Log("실패");
            OnStageFail();
        }
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
        // succedUI.SetActive(true);

        int current = StageData.Instance.currentStageIndex;

        StageData.Instance.SetStageCleared(current);
        StageData.Instance.IsStageCleared(current);
    }

    // 클리어 실패
    public void OnStageFail()
    {
        // fail.SetActive(true);
    }
    #endregion
}
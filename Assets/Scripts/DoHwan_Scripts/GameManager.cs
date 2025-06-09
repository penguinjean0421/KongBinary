using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤
    public float timeLimit = 120f; // 제한 시간
    private float timeRemaining;
    Text timerText; // 타이머 UI 텍스트

    private bool isGameOver = false;
    private int currentStage; // 현재 스테이지 번호

    [SerializeField] internal float sales;
    internal float[] star1Sale = new float[5] { 500, 1000, 1500, 2000, 2500 }; // (배열화 해서 레벨별로 다르게 설정)
    internal float[] star2Sale = new float[5] { 1000, 1500, 2000, 2500, 3000 }; // (배열화 해서 레벨별로 다르게 설정)
    internal float[] star3Sale = new float[5] { 1500, 2000, 2500, 3000, 4500 }; // (배열화 해서 레벨별로 다르게 설정)

    GameObject scoreUI;
    GameObject failUI;
    GameObject succedUI;
    GameObject star1;
    GameObject star2;
    GameObject star3;

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

        GameManager.Instance.currentStage = StageData.Instance.currentStageIndex;
        GameManager.Instance.timerText = GameObject.Find("TimerText").GetComponent<Text>();

        GameManager.Instance.scoreUI = GameObject.Find("ScoreUI");

        // GameManager.Instance.succedUI = GameObject.Find("SuccedUI");
        // GameManager.Instance.failUI = GameObject.Find("FailUI");

        // GameManager.Instance.star1 = GameObject.Find("Star1");
        // GameManager.Instance.star2 = GameObject.Find("Star2");
        // GameManager.Instance.star3 = GameObject.Find("Star3");
    }

    void Start()
    {
        ResetTimer(); // 게임 시작 시 타이머 초기화
        sales = 0; // 판매액 초기화

        GameManager.Instance.scoreUI.SetActive(false);
        Debug.Log($"현재 스테이지는 {GameManager.Instance.currentStage} 입니다.");
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

        if (Input.GetKeyDown(KeyCode.A))
        {
            AddSales(500f);
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
        ScoreUI();


        // scoreUI.SetActive(true);
        // 게임 오버 처리 (예: 씬 전환)
        // SceneManager.LoadScene("GameOver");

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

    #region  ScoreUI

    // 타임 오버 후 출력
    void ScoreUI()
    {
        scoreUI.SetActive(true);

        if (sales < star1Sale[currentStage - 1])
        {
            Debug.Log("실패");
            // failUI.SetActive(true);
        }
        else
        {
            StageData.Instance.SetStageCleared(currentStage);
            StageData.Instance.IsStageCleared(currentStage);

            if (sales >= star1Sale[currentStage - 1]) // 1번째 별
            {
                Debug.Log($"{currentStage} 스테이지 별 1개");
                // star1.SetActive(true);
            }

            if (sales >= star2Sale[currentStage - 1]) // 2번째 별
            {
                Debug.Log($"{currentStage} 스테이지 별 2개");
                // star2.SetActive(true);
            }

            if (sales >= star3Sale[currentStage - 1]) // 3번째 별 
            {
                Debug.Log($"{currentStage} 스테이지 별 3개");
                // star3.SetActive(true);
            }

            // succedUI.SetActive(true);
        }
    }




    // 스테이지 클리어
    public void OnStageClear()
    {

    }

    // 클리어 실패
    public void OnStageFail()
    {
        // fail.SetActive(true);
    }
    #endregion
}
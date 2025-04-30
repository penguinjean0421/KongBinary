using UnityEngine;
using UnityEngine.UI;
public class GameTimer : MonoBehaviour
{
    public Renderer colorObj;
    public Slider timer;

    public float maxTime; // 제한시간
    float time; // 시간

    bool isPause;// 일시정지 여부 체크
    bool isZero = false; // 제한시간이 끝났는지 체크
    void Start()
    {
        time = maxTime;
    }

    void Update()
    {
        // GameTime(Time.deltaTime);
        TestGameTime();
        Reset();
    }

    #region  함수
    void GameTime(float spendTime)
    {
        // 환경설정 이동
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPause)
            {
                isPause = true;
                Time.timeScale = 0f;
                Debug.Log("환경설정 창 켜짐");
            }
            else
            {
                Time.timeScale = 1f;
                isPause = false;
                Debug.Log("환경설정 창 꺼짐");
            }
        }

        if (!isZero)
        {
            time -= spendTime;
            Debug.Log($"게임 타임 : {Mathf.Floor(time * 100f) / 100f}");
            timer.value = time / maxTime;
            if (time <= 0)
            {
                Time.timeScale = 0f;
                isZero = true;
                Debug.Log("타임오버");
                Debug.Log("점수창 출력");
            }
        }
    }
    #endregion

    #region 테스트 코드
    // 게임타임 테스트 코드 (E 누르면 작동)
    void TestGameTime()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPause)
            {
                Time.timeScale = 0f;
                isPause = true;
            }
            else
            {
                Time.timeScale = 1f;
                isPause = false;
            }
        }
        GameTime(Time.deltaTime);

        if (isZero) { colorObj.material.color = Color.blue; }
    }

    // 타이머 초기화(S키 누르기)
    void Reset()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            time = maxTime;
            timer.value = 1f;
            isZero = false;
            colorObj.material.color = Color.white;
        }
    }
    #endregion

}

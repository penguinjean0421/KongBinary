using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{
    public Renderer colorObj;
    public Slider timer;

    public float maxTime; // 제한시간
    float time;

    bool isPause; // 일시정지 여부 체크
    bool isZero = false; // 시간제한 체크
    void Start()
    {
        time = maxTime;
    }

    void Update()
    {
        TestGameTime();
    }

    #region  함수
    void GameTime()
    {
        // 환경설정 이동
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPause)
            {
                isPause = true;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
                isPause = false;
            }
        }

        if (!isZero)
        {
            time -= Time.deltaTime;
            timer.value = time / maxTime;
            if (time <= 0)
            {
                Time.timeScale = 0f;
                isZero = true;
                Debug.Log("타임오버");
                colorObj.material.color = Color.blue;
            }
        }
    }
    #endregion

    #region 테스트 코드
    // 게임타임 테스트 코드 (W 누르면 작동)
    void TestGameTime()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!isPause)
            {
                isPause = true;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
                isPause = false;
            }
        }
        GameTime();
    }
    #endregion

}

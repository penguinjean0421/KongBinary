using UnityEngine;
using UnityEngine.UI;
public class CookingTimer : MonoBehaviour
{
    public Renderer colorObj;
    public Slider timer;

    public string menuName; // 요리 이름
    public float cookingTime; // 요리 소요 시간
    public float cuttingTime; // 손질 소요시간
    public float burned; // 음식이 언제 탔는지 확인할 기준 값
    float time; // 시간

    /* 화구 타이머 */
    bool isCooking; // 요리중인지 확인
    bool isBurned; // 음식이 탔는지 확인

    /* 손질 타이머 */
    bool isCutting; // 손질중인지 혹인
    bool isCut; // 음식이 손질 됬는지 확인

    void Start()
    {
        timer.value = 0f;
    }
    void Update()
    {
        // CookingTime(menuName, Time.unscaledDeltaTime, cookingTime);
        // CuttingTime(menuName, Time.unscaledDeltaTime, cookingTime);

        TestCookingTime();
        TestCuttingTime();
        Reset();
    }

    #region 함수
    // 요리 타이머
    void CookingTime(string menu, float spendTime, float menuTime)
    {
        if (!isCooking) return;

        if (!isBurned)
        {
            Debug.Log($"{menu} 조리중");

            time += spendTime;
            Debug.Log($"조리 타임 : {Mathf.Floor(time * 100f) / 100f}");
            timer.value = time / menuTime;
            if (time >= menuTime * burned)
            {
                isBurned = true;
                Debug.Log("음식이 망했습니다.");
            }
        }
    }
    // 손질 타이머
    void CuttingTime(string menu, float spendTime, float menuTime)
    {
        if (!isCutting) return;

        if (!isCut)
        {
            Debug.Log($"{menu} 손질중");

            time += spendTime;
            Debug.Log($"손질 타임: {Mathf.Floor(time * 100f) / 100f}");
            timer.value = time / menuTime;
            if (time >= cuttingTime)
            {
                isCut = true;
                isCutting = false;
                Debug.Log("손질 완");
            }
        }
    }
    #endregion

    #region 테스트 코드
    // 요리 테스트 코드 (Q키 클릭)
    void TestCookingTime()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isCooking)
            {
                Time.timeScale = 0f;
                Debug.Log($"요리 여부  {isCooking}");
            }

            else
            {
                isCooking = true;
                Debug.Log($"요리 여부  {isCooking}");
            }
        }
        CookingTime(menuName, Time.unscaledDeltaTime, cookingTime);

        if (isBurned) { colorObj.material.color = Color.red; }
    }

    // 손질 테스트 코드 (W키 클릭)
    void TestCuttingTime()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (isCutting)
            {
                isCutting = false;
                Debug.Log($"손질 여부 : {isCutting}");
            }
            else
            {
                isCutting = true;
                Debug.Log($"손질 여부: {isCutting}");
            }
        }
        CuttingTime(menuName, Time.unscaledDeltaTime, cookingTime);

        if (isCut) { colorObj.material.color = Color.green; }
    }
    // 타이머 초기화(A키 누르기)
    void Reset()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            time = 0f;
            timer.value = 0f;
            isBurned = false;
            isCut = false;
            colorObj.material.color = Color.white;
        }
    }
    #endregion
}
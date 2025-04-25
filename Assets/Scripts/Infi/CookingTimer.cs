using UnityEngine;
using UnityEngine.UI;
public class CookingTimer : MonoBehaviour
{
    public Renderer colorObj;
    public Slider timer;
    float time; // 시간

    public float[] cookingTime;
    public string[] menuName;
    int menu; // 메뉴 인덱스 지정

    public float burned; // 음식이 탔는지 확인할때 쓰는 기준 값
    float timeNormal;
    bool isBurned; // 음식이 탔는지 확인하는 bool 값
    bool isCooking; // 요리중인지 확인하는 Bool 값

    void Start()
    {
        menu = Random.Range(0, menuName.Length);
        timer.value = 0f;
    }
    void Update()
    {
        // CookingTime(menuName[a], Time.deltaTime, cookingTime[a]);
        TestCookingTime();
    }

    #region 함수
    void CookingTime(string menu, float spendTime, float menuTime)
    {
        if (!isCooking) return;

        if (!isBurned)
        {
            Debug.Log($"{menu} 조리중");

            time += spendTime;
            timeNormal = time / menuTime;
            timer.value = timeNormal;
            if (timeNormal >= burned)
            {
                isBurned = true;
                Debug.Log("음식이 망했습니다.");
                colorObj.material.color = Color.red;
            }
            Debug.Log(time);
        }
    }
    #endregion

    #region 테스트 코드
    // 쿠킹타임 테스트 코드 (Q키 클릭)
    void TestCookingTime()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isCooking)
            {
                isCooking = false;
                Time.timeScale = 0f;
                Debug.Log($"요리 여부  {isCooking}");
            }

            else
            {
                Time.timeScale = 1f;
                isCooking = true;
                Debug.Log($"요리 여부  {isCooking}");
            }
        }
        CookingTime(menuName[menu], Time.deltaTime, cookingTime[menu]);
    }
    #endregion
}
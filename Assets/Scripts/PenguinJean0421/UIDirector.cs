using UnityEngine;
using UnityEngine.SceneManagement;
public class UIDirector : MonoBehaviour
{
    public GameObject settingTest;
    bool isSetting; // 설정창 실행여부

    void Start()
    {
        settingTest.SetActive(false);
    }

    void Update()
    {
#if UNITY_EDITOR
        TestOnClickSetting();
        TestOnClickStage();
#endif

    }

    public void OnClickSetting(bool setting)
    {
        settingTest.SetActive(setting);
        // 환경설정 창 작동
    }

    public void OnClickStage()
    {
        SceneManager.LoadScene("Timer"); // 스테이지 선택창 생기면 Scene 변경 바람

        /* SetActive
        gameStart.SetActive(false);
        chooseStage.SetActive(true);
        */
    }

    #region 테스트코드
    void TestOnClickSetting()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isSetting = !isSetting;
            Debug.Log($"설정 창 : {isSetting}");
            OnClickSetting(isSetting);
        }
    }

    void TestOnClickStage()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnClickStage();
            Debug.Log("스테이지 선택 ㄱ");
        }
    }
    #endregion

}

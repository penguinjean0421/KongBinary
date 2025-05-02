using UnityEngine;
using UnityEngine.SceneManagement;
public class UIDirector : MonoBehaviour
{
    public GameObject settingTest;
    public bool isPause;

    void Start()
    {
        settingTest.SetActive(false);
    }

    void Update()
    {
        // OnClickSetting(isPause);
#if UNITY_EDITOR
        TestOnClickSetting();
        TestOnClickStage();
#endif

    }

    public void OnClickSetting()
    {
        settingTest.SetActive(isPause);
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
            isPause = !isPause;
            Debug.Log($"설정 창 : {isPause}");
            OnClickSetting();
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

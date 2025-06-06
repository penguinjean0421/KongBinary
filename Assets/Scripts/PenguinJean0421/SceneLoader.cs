using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    // 게임시작 화면
    public void OnClickStart()
    {
        // 튜토 구현 되면 아래 #if ~ #endif 지울것
#if UNITY_EDITOR
        StageData.Instance.SetStageCleared(0);
        StageData.Instance.IsStageCleared(0);
#endif

        if (PlayerPrefs.GetInt($"Stage0Clear") == 0)
        {
            SceneManager.LoadScene("SettingController");
        }

        else
        {
            OnClickStage();
        }
    }

    // 게임 튜토리얼 창 입장
    public void OnClickTutorial()
    {
        SceneManager.LoadScene("Tutorial");
    }

    // 스테이지 선택창 이동
    public void OnClickStage()
    {
        SceneManager.LoadScene("ChooseStage");
    }

    // 종료버튼 누르면
    public void OnClickGameExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    // 돌아가기 버튼
    public void OnClickBack()
    {
        SceneManager.LoadScene("Start");
    }
}

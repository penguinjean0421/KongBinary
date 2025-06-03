using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    // 게임시작 화면
    public void OnClickStart()
    {
        if (PlayerPrefs.GetInt($"Stage0Clear") == 0)
        {
            SceneManager.LoadScene("SettingController");
        }

        else
        {
            OnClickStage();
        }
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

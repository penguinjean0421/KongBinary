using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    #region StartScene
    // 시작 버튼 누르면 스테이지 선택창 이동
    public void OnClickGameStart()
    {
        SceneManager.LoadScene("ChooseStage");
    }

    // 종료버튼 누르면
    public void OnClickGameExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }
    #endregion
    // 돌아가기 버튼
    #region Choose Stage Scene
    public void OnClickBack()
    {
        SceneManager.LoadScene("Start");
    }
    #endregion

    #region Score Scene
    public void OnNext()
    {
        SceneManager.LoadScene("ChooseStage");
    }
    #endregion
}
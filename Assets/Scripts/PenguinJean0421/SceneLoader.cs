using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    internal static SceneLoader Instance;
    // 게임시작 버튼
    public void OnClickStart()
    {
        SceneManager.LoadScene("Player_Select");
    }

    // 게임 튜토리얼 창 입장
    public void OnClickTutorial()
    {
        SceneManager.LoadScene("Stage0");
    }

    // 스테이지 선택창 이동
    public void OnClickStage()
    {
        SceneManager.LoadScene("ChooseStage");
    }

    // 환경설정
    public void OnClickSetting()
    {
        SettingManager.Instance.SettingActive();
    }

    // 종료버튼
    public void OnClickGameExit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    // 시작 화면으로 돌아가기 버튼
    public void OnClickBack()
    {
        SceneManager.LoadScene("Start");
    }

    // 다시하기 버튼
    public void ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 다음 스테이지 로드
    public void LoadNextStage()
    {
        StageData.Instance.currentStageIndex++;
        SceneManager.LoadScene($"Level{StageData.Instance.currentStageIndex}");
    }

    // 튜토리얼 클리어 처리
    public void ClearTuto()
    {
        StageData.Instance.SetStageCleared(0);
        StageData.Instance.IsStageCleared(0);
        OnClickStage();
    }
}

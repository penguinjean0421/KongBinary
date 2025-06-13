using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    // 게임시작 버튼
    public void OnClickStart()
    {
        // 튜토 구현 되면 아래 #if ~ #endif 지울것
#if UNITY_EDITOR
        // StageData.Instance.SetStageCleared(0);
        // StageData.Instance.IsStageCleared(0);
        // StageData.Instance.ResetAllStageData(StageData.Instance.maxStage);
#endif

        if (PlayerPrefs.GetInt($"Stage0Clear") == 0)
        {
            SceneManager.LoadScene("CharPick");
        }

        else
        {
            OnClickStage();
        }
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

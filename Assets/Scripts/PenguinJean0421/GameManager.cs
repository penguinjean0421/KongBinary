using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    void Awake()
    {
        var obj = FindObjectsOfType<GameManager>();
        if (obj.Length == 1)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnClickGameStart()
    {
        SceneManager.LoadScene("Stage");
        Debug.Log("스테이지 선택창 진입");
    }

    public void OnClickStage() // 스테이지 추가할때 복붙후 스테이지 넘버링 
    {
        SceneManager.LoadScene("InGame");
        Debug.Log("인게임 진입");
    }

    public void EndGame()
    {
        SceneManager.LoadScene("Score");
        Debug.Log("점수창");
    }

}

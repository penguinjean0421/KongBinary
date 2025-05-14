using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this as SceneLoader;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void OnSceneChanged(Scene previous, Scene current)
    {
        Debug.Log($"씬 바뀜: {previous.name} > {current.name}");
    }

    public void OnClickGameStart()
    {
        SceneManager.LoadScene("Stage");
    }

    public void OnClickStage() // 스테이지 추가할때 복붙후 스테이지 넘버링 
    {
        SceneManager.LoadScene("InGame");
    }

    public void EndGame()
    {
        SceneManager.LoadScene("Score");
    }
}
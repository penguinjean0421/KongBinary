using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using VFolders.Libs;
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

    private void OnSceneChanged(Scene arg0, Scene arg1)
    {
        // throw new NotImplementedException();
        Debug.Log("씬 바뀜");
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

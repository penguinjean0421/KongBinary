using UnityEngine;
using UnityEngine.UI;
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; } // 싱글톤
    [SerializeField] GameObject settingScene;
    SceneLoader sceneLoader; // 씬로더

    bool isPause;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        sceneLoader = GetComponent<SceneLoader>();
    }

    void Start()
    {
        settingScene.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SettingActive();
        }
    }

    // 환경설정 창 실행
    public void SettingActive()
    {
        isPause = !isPause;
        if (!isPause)  // 창 켜짐
        {
            Time.timeScale = 0f;
            // isPause = true;
            settingScene.SetActive(isPause);
        }
        else // 창 꺼짐
        {
            Time.timeScale = 1f;
            // isPause = false;
            settingScene.SetActive(isPause);
        }
        Debug.Log($"환경설정 창 : {isPause}");
    }
}

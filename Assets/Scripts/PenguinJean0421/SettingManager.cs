using UnityEngine;
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; } // 싱글톤
    [SerializeField] GameObject settingScene;
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
    }

    void Start()
    {
        settingScene.SetActive(false);
    }

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
            Time.timeScale = 1f;
            // isPause = true;
            settingScene.SetActive(isPause);
        }
        else // 창 꺼짐
        {
            Time.timeScale = 0f;
            // isPause = false;
            settingScene.SetActive(isPause);
        }
        Debug.Log($"환경설정 창 : {isPause}");
    }
}

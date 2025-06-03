using UnityEngine;
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; } // 싱글톤
    [SerializeField] GameObject settingScene;
    bool isPlaying;

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
        isPlaying = !isPlaying;
        if (!isPlaying)  // 창 켜짐
        {
            Time.timeScale = 1f;
            // isPlaying = true;
            settingScene.SetActive(isPlaying);
        }
        else // 창 꺼짐
        {
            Time.timeScale = 0f;
            // isPlaying = false;
            settingScene.SetActive(isPlaying);
        }
        Debug.Log($"환경설정 창 : {isPlaying}");
    }
}

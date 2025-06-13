using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; } // 싱글톤
    [SerializeField] GameObject settingScene;


    // 수직동기화
    Toggle vSyncToggle;

    // 사운드
    AudioSource audioSource;
    Slider soundSlider;
    Slider bgmSlider;
    Slider systemSlider;


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
        vSyncToggle = GameObject.Find("VSyncToggle").GetComponent<Toggle>();
        vSyncToggle.isOn = QualitySettings.vSyncCount > 0;

        audioSource = GameObject.Find("Audio").GetComponent<AudioSource>();
        soundSlider = GameObject.Find("SoundSlider").GetComponent<Slider>();
        bgmSlider = GameObject.Find("BgmSlider").GetComponent<Slider>();
        // systemSlider = GameObject.Find("SystemSlider").GetComponent<Slider>();
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

    // 화면 크기 조절

    // 해상도

    // 수직동기화
    public void vSync()
    {
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
        Debug.Log($"수직동기화 {vSyncToggle.isOn}");
    }

    // 전체음량 (슬라이더)
    public void SoundVolume()
    {
        Debug.Log($"전체 음량: {soundSlider.value}");
    }

    // 배경음악 (슬라이더)
    public void BGMVolume()
    {
        audioSource.volume = soundSlider.value * bgmSlider.value;
        Debug.Log($"지금 BGM 볼륨 : {audioSource.volume}");
    }

    // 시스템 (슬라이더)
    public void SystemVolume()
    {
        audioSource.volume = soundSlider.value * bgmSlider.value;
        Debug.Log($"지금 시스템 볼륨 : {audioSource.volume}");
    }
}

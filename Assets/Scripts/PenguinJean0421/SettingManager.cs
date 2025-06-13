using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; } // 싱글톤
    GameObject settingScene;
    public GameObject[] settingPages;
    int pageIndex;

    // 그래픽
    Dropdown screenMode; // 화면모드
    Toggle vSyncToggle; // 수직동기화

    // 사운드 페이지
    AudioSource audioSource;
    Slider soundSlider;  // 전체
    Slider bgmSlider; // BGM
    Slider systemSlider; // 시스템


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
        settingScene = GameObject.Find("SettingScene");

        screenMode = GameObject.Find("ScreenMode").GetComponent<Dropdown>();
        vSyncToggle = GameObject.Find("VSyncToggle").GetComponent<Toggle>();

        // audioSource = GameObject.Find("Audio").GetComponent<AudioSource>();
        soundSlider = GameObject.Find("SoundSlider").GetComponent<Slider>();
        bgmSlider = GameObject.Find("BgmSlider").GetComponent<Slider>();
        systemSlider = GameObject.Find("SystemSlider").GetComponent<Slider>();

        vSyncToggle.isOn = QualitySettings.vSyncCount > 0;
        ResetScreenMode();

        settingScene.SetActive(false);
        settingPages[1].SetActive(false);
    }

    void Update()
    {
        SettingActive();
    }

    // 환경설정 창 실행
    public void SettingActive()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPlaying = !isPlaying;
            if (!isPlaying)  // 창 켜짐
            {
                Time.timeScale = 1f;
                settingScene.SetActive(isPlaying);
            }
            else // 창 꺼짐
            {
                Time.timeScale = 0f;
                settingScene.SetActive(isPlaying);
                pageIndex = 0;
            }
            Debug.Log($"환경설정 창 : {isPlaying}");
        }
    }

    // 화면 크기 조절
    void SetScreenMode(ScreenMode mode)
    {
        switch (mode)
        {
            case ScreenMode.FullScreenWindow:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case ScreenMode.Window:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
        }
    }

    // 수직동기화
    public void SetVSync()
    {
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
        Debug.Log($"수직동기화 {vSyncToggle.isOn}");
    }

    // 전체음량 (슬라이더)
    public void SetSoundVolume()
    {
        Debug.Log($"전체 음량: {soundSlider.value}");
    }

    // 배경음악 (슬라이더)
    public void SetBGMVolume()
    {
        audioSource.volume = soundSlider.value * bgmSlider.value;
        Debug.Log($"지금 BGM 볼륨 : {audioSource.volume}");
    }

    // 시스템 (슬라이더)
    public void SetSystemVolume()
    {
        audioSource.volume = soundSlider.value * bgmSlider.value;
        Debug.Log($"지금 시스템 볼륨 : {audioSource.volume}");
    }

    // 설정창 이동(다음)
    public void OnClickNextPage()
    {
        settingPages[pageIndex % settingPages.Length].SetActive(false);
        pageIndex++;
        settingPages[pageIndex % settingPages.Length].SetActive(true);
    }

    // 설정창 이동(이전)
    public void OnClickLastPage()
    {
        settingPages[pageIndex % settingPages.Length].SetActive(false);
        pageIndex--;
        settingPages[pageIndex % settingPages.Length].SetActive(true);
    }

    // 화면모드 드롭다운 초기화
    void ResetScreenMode()
    {
        List<string> options = new List<string> { "전체화면", "창모드" };

        screenMode.ClearOptions();
        screenMode.AddOptions(options);
        screenMode.onValueChanged.AddListener(index => SetScreenMode((ScreenMode)index));

        switch (screenMode.value)
        {
            case 0:
                Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
                break;
            case 1:
                Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                break;
        }
    }
    public enum ScreenMode
    {
        FullScreenWindow,
        Window
    }
}

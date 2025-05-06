using UnityEngine;
public class UIDirector : MonoBehaviour
{
    public GameObject settingTest;
    public bool isPause;

    void Start()
    {
        settingTest.SetActive(false);
    }

    void Update()
    {
        // OnClickSetting(isPause);
#if UNITY_EDITOR
#endif

    }

    public void OnClickSetting()
    {
        settingTest.SetActive(isPause);
    }
}

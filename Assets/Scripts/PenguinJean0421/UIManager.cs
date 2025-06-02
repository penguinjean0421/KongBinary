using UnityEngine;
using UnityEngine.UI;
public class UIManager : MonoBehaviour
{
  // 환경설정 버튼
  public void OnClickSetting()
  {
    SettingManager.Instance.SettingActive();
  }
}

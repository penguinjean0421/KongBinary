using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    GameObject scoreUI; // 공통 UI
    GameObject succedUI; // Clear 시
    GameObject failUI; // Clear 실패 시

    void Awake()
    {
        scoreUI = GameObject.Find("ScoreUI");
        succedUI = GameObject.Find("SuccedUI");
        failUI = GameObject.Find("FailUI");
    }

    void Start()
    {
        // 점수 UI 게임 시작 할때는 비활성화
        scoreUI.SetActive(false);
        succedUI.SetActive(false);
        failUI.SetActive(false);
    }



}

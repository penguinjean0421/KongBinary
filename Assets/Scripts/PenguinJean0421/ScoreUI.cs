using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    GameObject scoreUI;
    GameObject succedUI;
    GameObject failUI;

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


    // public void ClearUI()
    // {
    //     if (sales >= star1Sale)
    //     {
    //         if (sales >= star3Sale)
    //         {
    //             Debug.Log("별 3개");
    //         }
    //         else if (sales >= star2Sale)
    //         {
    //             Debug.Log("별 2개");
    //         }
    //         else
    //         {
    //             Debug.Log("별 1개");
    //         }
    //         OnStageClear();
    //     }
    //     else
    //     {
    //         Debug.Log("실패");
    //         OnStageFail();
    //     }
    // }
}

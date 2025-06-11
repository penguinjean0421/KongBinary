using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Story : MonoBehaviour
{

    [SerializeField] private Image backgroundImage; // 배경 이미지
    [SerializeField] private Text storyText;        // 스토리 텍스트 (TextMeshProUGUI로 변경 가능)
    [SerializeField] private Sprite[] backgrounds;  // 배경 이미지 배열
    [SerializeField] private string[] storyLines;   // 스토리 텍스트 배열
    private int currentIndex = 0;                   // 현재 스토리 인덱스
    // Start is called before the first frame update
    void Start()
    {
        // 초기 스토리 표시
        UpdateStory();
    }

    void Update()
    {
        // 'E' 키 입력 감지
        if (Input.GetKeyDown(KeyCode.E))
        {
            NextStory();
        }
    }

    private void UpdateStory()
    {
        if (currentIndex < storyLines.Length)
        {
            // 텍스트 업데이트
            storyText.text = storyLines[currentIndex];

            // 배경 이미지 업데이트 (배경 배열이 있으면)
            if (backgrounds != null && currentIndex < backgrounds.Length)
            {
                backgroundImage.sprite = backgrounds[currentIndex];
            }
        }
        else
        {
            // 스토리 끝: 다음 씬으로 이동하거나 종료
            Debug.Log("스토리 종료");
            // 예: SceneManager.LoadScene("NextScene");
        }
    }


    public void NextStory()
    {
        currentIndex++;
        UpdateStory();
    }

    private void ResetStory()
    {
        currentIndex = 0;
        UpdateStory();
        Debug.Log("스토리 초기화");
    }
}
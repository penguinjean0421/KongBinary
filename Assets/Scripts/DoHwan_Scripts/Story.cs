using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Story : MonoBehaviour
{

    [SerializeField] private Image backgroundImage; // ��� �̹���
    [SerializeField] private Text storyText;        // ���丮 �ؽ�Ʈ (TextMeshProUGUI�� ���� ����)
    [SerializeField] private Sprite[] backgrounds;  // ��� �̹��� �迭
    [SerializeField] private string[] storyLines;   // ���丮 �ؽ�Ʈ �迭
    private int currentIndex = 0;                   // ���� ���丮 �ε���
    // Start is called before the first frame update
    void Start()
    {
        // �ʱ� ���丮 ǥ��
        UpdateStory();
    }

    void Update()
    {
        // 'E' Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.E))
        {
            NextStory();
        }
    }

    private void UpdateStory()
    {
        if (currentIndex < storyLines.Length)
        {
            // �ؽ�Ʈ ������Ʈ
            storyText.text = storyLines[currentIndex];

            // ��� �̹��� ������Ʈ (��� �迭�� ������)
            if (backgrounds != null && currentIndex < backgrounds.Length)
            {
                backgroundImage.sprite = backgrounds[currentIndex];
            }
        }
        else
        {
            // ���丮 ��: ���� ������ �̵��ϰų� ����
            Debug.Log("���丮 ����");
            // ��: SceneManager.LoadScene("NextScene");
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
        Debug.Log("���丮 �ʱ�ȭ");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bill : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] public const float maxTime=20;


    public float currentTime;

    void Start()
    {
        if (slider != null)
        {
            slider.maxValue = 1f;
            slider.value = 1f;
            currentTime = maxTime;
            StartCoroutine(DecreaseSlider());
        }
        else
        {
            Debug.LogError("Bill: Slider not assigned!");
        }
    }

    private IEnumerator DecreaseSlider()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            slider.value = currentTime / maxTime; // 20�ʿ��� 0�ʱ��� 1���� 0���� ����
            yield return null;
        }
        slider.value = 0f; // ��Ȯ�� 0���� ����
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Shake : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 0.1f; // ��鸲 ����
    [SerializeField] private float shakeSpeed = 2f;    // ��鸲 ��
    private Vector3 initialPosition;
    private Vector3 targetOffset;
    private float timer;

    void Start()
    {
        initialPosition = transform.position;
        targetOffset = Vector3.zero;
    }

    void Update()
    {
        // ���� �ð� �������� ���ο� ���� offset ����
        timer += Time.deltaTime * shakeSpeed;
        if (timer >= 1f)
        {
            targetOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                0
            );
            timer = 0f;
        }

        // �ε巴�� ���� ��ġ�� ��ǥ offset���� �̵�
        transform.position = Vector3.Lerp(
            transform.position,
            initialPosition + targetOffset,
            Time.deltaTime * shakeSpeed
        );
    }
}

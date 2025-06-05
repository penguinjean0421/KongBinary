using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Shake : MonoBehaviour
{
    [SerializeField] private float shakeAmount = 0.1f; // 흔들림 강도
    [SerializeField] private float shakeSpeed = 2f;    // 흔들림 빈도
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
        // 일정 시간 간격으로 새로운 랜덤 offset 생성
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

        // 부드럽게 현재 위치를 목표 offset으로 이동
        transform.position = Vector3.Lerp(
            transform.position,
            initialPosition + targetOffset,
            Time.deltaTime * shakeSpeed
        );
    }
}

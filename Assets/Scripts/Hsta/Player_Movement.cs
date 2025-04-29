using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float walkSpeed = 5f;    // 걷기 속도
    [SerializeField] private float sprintSpeed = 8f;  // 달리기 속도
    [SerializeField] private float rotationSpeed = 10f;
    private float currentSpeed;  // 현재 속도
    private bool isSprinting;    // 달리기 상태
    private Rigidbody rb;
    private Player_Controller playerController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true; // 회전을 고정하여 물리 충돌로 인한 회전을 방지
        rb.drag = 1f; // 이동 중지 시 자연스러운 감속을 위한 드래그
        
        // Player_Controller 컴포넌트 가져오기
        playerController = GetComponent<Player_Controller>();
        currentSpeed = walkSpeed;  // 초기 속도는 걷기 속도로 설정
    }

    void Update()
    {
        // 상호작용 중일 때는 이동 불가
        if (playerController != null && playerController.isInteracting)
        {
            // 이동 중지
            rb.velocity = Vector3.zero;
            return;
        }

        // 달리기 상태 체크
        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        MoveMent();
    }

    private void MoveMent()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (movement != Vector3.zero)
        {
            // 이동 방향으로 부드럽게 회전
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // currentSpeed를 사용하여 이동
            Vector3 targetVelocity = movement * currentSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 10f);
        }
        else
        {
            // 입력이 없을 때는 속도를 점진적으로 감소
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 10f);
        }
    }
}

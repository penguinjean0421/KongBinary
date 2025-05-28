using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player_Movement : MonoBehaviour
{
    // 플레이어 타입 구분
    public enum PlayerType { Player1, Player2 }
    [SerializeField] public PlayerType playerType = PlayerType.Player1; // 인스펙터에서 설정

    [SerializeField] private float walkSpeed = 5f;    // 걷기 속도
    [SerializeField] private float sprintSpeed = 8f;  // 달리기 속도
    [SerializeField] private float rotationSpeed = 10f;
    private float currentSpeed;  // 현재 속도
    private bool isSprinting;    // 달리기 상태
    private Rigidbody rb;
    private Player_Controller playerController;
    [SerializeField] public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true; // 회전을 고정하여 물리 충돌로 인한 회전 방지
        rb.drag = 1f; // 이동 중지 시 자연스러운 감속을 위한 드래그

        // Player_Controller 컴포넌트 가져오기
        playerController = GetComponent<Player_Controller>();
        currentSpeed = walkSpeed;  // 초기 속도는 걷기 속도로 설정
         // 연결된 게임패드 확인
        Debug.Log("Connected Joysticks: " + string.Join(", ", Input.GetJoystickNames()));
    }

    void Update()
    {
        //애니메이션 
        if (animator != null)
        {

            //Debug.LogWarning(currentSpeed > 0.1f);
            animator.SetBool("IsRun?", rb.velocity.magnitude > 0f);
            animator.SetBool("IsItem?", playerController.isHandObject != null);
            animator.SetBool("IsCutting?", playerController.isInteracting);
            //Debug.LogWarning(animator.angularVelocity);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned in Player_Movement script!");
        }

        // 상호작용 중일 때는 이동 불가
        if (playerController != null && playerController.isInteracting)
        {
            // 이동 중지
            rb.velocity = Vector3.zero;
            return;
        }

        // 플레이어 타입에 따라 입력 처리
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (playerType == PlayerType.Player1)
        {
            // 1P: WASD, Left Shift
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // WASD 입력 직접 처리 (Player1 전용)
            if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;
            if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
            if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

            // 상호작용 입력 처리
            if (Input.GetKeyDown(KeyCode.LeftControl) && playerController != null && playerController.isInTrigger)
            {
                playerController.OnTag();
            }
        }
        else if (playerType == PlayerType.Player2)
        {
            // 2P: 화살표 키, Right Shift 및 게임패드
            isSprinting = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.JoystickButton4);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            // 키보드 입력 (Player2 전용)
            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) verticalInput = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) verticalInput = -1f;

            // 게임패드 입력 (Player2 전용)
            float gamepadHorizontal = Input.GetAxis("Horizontal");
            float gamepadVertical = Input.GetAxis("Vertical");

            // 키보드나 게임패드 중 더 큰 값을 사용
            if (Mathf.Abs(gamepadHorizontal) > 0.1f) horizontalInput = gamepadHorizontal;
            if (Mathf.Abs(gamepadVertical) > 0.1f) verticalInput = gamepadVertical;

            // 상호작용 입력 처리 (키보드 RightControl 또는 게임패드 A버튼)
            if ((Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.JoystickButton0)) && 
                playerController != null && playerController.isInTrigger)
            {
                playerController. OnTag();
            }

            // 디버깅: 입력값 로그 출력 (60프레임마다)
            //if (Time.frameCount % 60 == 0)
            //{
            //    Debug.Log($"Player2 Input - H: {horizontalInput}, V: {verticalInput}, Sprint: {isSprinting}, Gamepad: {Input.GetKey(KeyCode.JoystickButton4)}");
            //}

            //애니메이션파라미터 설정
            //animator.SetBool("IsSprinting", isSprinting);
            //animator.SetBool("HasItem", playerController.isHandObject != null);
            
        }
       
        // 이동 처리
        if (horizontalInput != 0 || verticalInput != 0)
        {
            MoveMent(horizontalInput, verticalInput);
        }
        else
        {
            // 입력이 없을 때는 속도를 0으로
            rb.velocity = Vector3.zero;
        }
    }

    private void MoveMent(float horizontalInput, float verticalInput)
    {
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (movement != Vector3.zero)
        {
            // 이동 방향으로 부드럽게 회전
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // currentSpeed를 사용하여 이동
            //Vector3 targetVelocity = movement * currentSpeed;
            //rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.deltaTime * 10f);

            // currentSpeed를 사용하여 즉시 속도 설정 (Lerp 제거)
            rb.velocity = movement * currentSpeed;
        }
        else
        {
            // 입력이 없을 때는 속도를 점진적으로 감소
            //rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime * 10f);

            // 입력이 없을 때 즉시 속도 0으로 설정
            rb.velocity = Vector3.zero;
        }
    }
}
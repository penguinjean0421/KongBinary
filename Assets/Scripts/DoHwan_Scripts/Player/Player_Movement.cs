using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player_Movement : MonoBehaviour
{
    public enum PlayerType { Player1, Player2 }
    [SerializeField] public PlayerType playerType = PlayerType.Player1;

    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSpeed = 10f;
    private float currentSpeed;
    private bool isSprinting;
    private Rigidbody rb;
    private Player_Controller playerController;
    [SerializeField] public Animator animator;
    [SerializeField] private ParticleSystem dustParticle;
    [SerializeField] private AudioClip footstepSound; // 걸음소리 오디오 클립

    private Vector3 moveDirection; // 이동 방향 저장
    private AudioSource audioSource; // 오디오 소스

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.freezeRotation = true;
        rb.drag = 1f;

        playerController = GetComponent<Player_Controller>();
        currentSpeed = walkSpeed;

        // AudioSource 컴포넌트 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = footstepSound;
        audioSource.loop = true; // 루프 활성화
        audioSource.playOnAwake = false;

        Debug.Log("Connected Joysticks: " + string.Join(", ", Input.GetJoystickNames()));
    }

    void Update()
    {
        // 애니메이션
        if (animator != null)
        {
            animator.SetBool("IsRun?", rb.velocity.magnitude > 0.1f);
            animator.SetBool("IsItem?", playerController.isHandObject != null);
            animator.SetBool("IsCutting?", playerController.isInteracting);
        }
        else
        {
            Debug.LogWarning("Animator is not assigned in Player_Movement script!");
        }

        // 상호작용 중 이동 불가
        if (playerController != null && playerController.isInteracting)
        {
            rb.velocity = Vector3.zero;
            if (dustParticle != null && dustParticle.isPlaying)
            {
                dustParticle.Stop();
            }
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop(); // 상호작용 중 소리 중지
            }
            return;
        }

        // 입력 처리
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (playerType == PlayerType.Player1)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;
            if (Input.GetKey(KeyCode.W)) verticalInput = 1f;
            if (Input.GetKey(KeyCode.S)) verticalInput = -1f;

            if (Input.GetKeyDown(KeyCode.LeftControl) && playerController != null && playerController.isInTrigger)
            {
                playerController.OnTag();
            }
        }
        else if (playerType == PlayerType.Player2)
        {
            isSprinting = Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.JoystickButton4);
            currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

            if (Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;
            if (Input.GetKey(KeyCode.UpArrow)) verticalInput = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) verticalInput = -1f;

            float gamepadHorizontal = Input.GetAxis("Horizontal");
            float gamepadVertical = Input.GetAxis("Vertical");

            if (Mathf.Abs(gamepadHorizontal) > 0.1f) horizontalInput = gamepadHorizontal;
            if (Mathf.Abs(gamepadVertical) > 0.1f) verticalInput = gamepadVertical;

            if ((Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.JoystickButton0)) &&
                playerController != null && playerController.isInTrigger)
            {
                playerController.OnTag();
            }
        }

        // 이동 방향 계산
        moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // 걸음소리 제어
        if (audioSource != null)
        {
            if (rb.velocity.magnitude > 0.1f && !audioSource.isPlaying)
            {
                audioSource.Play(); // 이동 중 시작
            }
            else if (rb.velocity.magnitude <= 0.1f && audioSource.isPlaying)
            {
                audioSource.Stop(); // 이동 멈춤 시 중지
            }
        }
    }

    void FixedUpdate()
    {
        // 이동 처리
        if (moveDirection != Vector3.zero)
        {
            // 충돌 방지 체크
            RaycastHit hit;
            if (Physics.Raycast(transform.position, moveDirection, out hit, 0.1f))
            {
                if (hit.collider.CompareTag("Table"))
                {
                    moveDirection = Vector3.zero; // 벽에 가까우면 이동 중지
                }
            }

            // 부드러운 회전
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // 부드러운 이동
            Vector3 targetVelocity = moveDirection * currentSpeed;
            rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 10f);
        }
        else
        {
            // 입력 없으면 속도 감소
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 10f);
        }

        // 먼지 파티클 제어
        if (dustParticle != null)
        {
            if (rb.velocity.magnitude > 0.1f && !dustParticle.isPlaying)
            {
                dustParticle.Play();
            }
            else if (rb.velocity.magnitude <= 0.1f && dustParticle.isPlaying)
            {
                dustParticle.Stop();
            }
        }
    }
}
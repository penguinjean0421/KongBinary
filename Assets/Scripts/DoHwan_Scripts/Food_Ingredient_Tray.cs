using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food_Ingredient_Tray : MonoBehaviour
{
    public GameObject ingredient;
    public int ingredientCount = 5;
    [SerializeField] private Animator animator;

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Food_Ingredient_Tray: Animator component not found!");
                return;
            }
        }

        // 게임 시작 시 애니메이션 정지 보장
        animator.enabled = false; // 애니메이터 비활성화
        animator.SetBool("IsOpen?", false); // 초기 상태 닫힘으로 설정
    }

    private void OnEnable()
    {
        // 오브젝트 활성화 시에도 정지 상태 유지
        if (animator != null)
        {
            animator.enabled = false;
            animator.SetBool("IsOpen?", false);
        }
    }

    public void AnimationPlayer()
    {
        if (animator != null)
        {
            animator.enabled = true; // 애니메이터 활성화
            StartCoroutine(PlayAnimationsSequentially());
        }
    }

    IEnumerator PlayAnimationsSequentially()
    {
        // 1. Open 애니메이션 재생
        animator.SetBool("IsOpen?", true);

        // Open 애니메이션 길이만큼 대기
        AnimationClip openClip = GetAnimationClip("CoverOpen"); // Open 애니메이션 클립 이름
        if (openClip != null)
        {
            yield return new WaitForSeconds(openClip.length);
        }
        else
        {
            Debug.LogWarning("Open animation clip not found, skipping wait.");
        }

        // 2. Close 애니메이션 재생
        animator.SetBool("IsOpen?", false);

        // Close 애니메이션 길이만큼 대기 (선택 사항)
        AnimationClip closeClip = GetAnimationClip("CoverClose"); // Close 애니메이션 클립 이름
        if (closeClip != null)
        {
            yield return new WaitForSeconds(closeClip.length);
        }
        else
        {
            Debug.LogWarning("Close animation clip not found, skipping wait.");
        }

        // 애니메이션 완료 후 비활성화 (선택 사항)
        animator.enabled = false;
    }

    // 애니메이션 클립을 이름으로 가져오는 헬퍼 메서드
    private AnimationClip GetAnimationClip(string clipName)
    {
        if (animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == clipName)
                {
                    return clip;
                }
            }
        }
        Debug.LogWarning($"Animation clip {clipName} not found!");
        return null;
    }
}
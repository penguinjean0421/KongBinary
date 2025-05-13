using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food_Ingredient_Tray : MonoBehaviour
{
   public GameObject ingredient;
   public int ingredientCount = 5;
   [SerializeField] private Animator animator;
  


    /*
       public GameObject GetIngredient()
       {
           if (ingredientCount > 0)
           {
               ingredientCount--;
               return ingredient;
           }
           Debug.Log("트레이 비었음");
           return null;
       }
    */


    private void OnTriggerEnter(Collider other)
   {
    //fish.SetActive(false);
    //fish_tray.SetActive(true);
   }

    public void AnimationPlayer()
    {
        StartCoroutine(PlayAnimationsSequentially());
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

        // 2. Close 애니메이션 재생
        animator.SetBool("IsOpen?", false);

        // Close 애니메이션 길이만큼 대기 (선택 사항)
        AnimationClip closeClip = GetAnimationClip("CoverClose"); // Close 애니메이션 클립 이름
        if (closeClip != null)
        {
            yield return new WaitForSeconds(closeClip.length);
        }
    }

    // 애니메이션 클립을 이름으로 가져오는 헬퍼 메서드
    private AnimationClip GetAnimationClip(string clipName)
    {
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }
        Debug.LogWarning($"Animation clip {clipName} not found!");
        return null;
    }

    //    public void UsingTray()
    //    {
    //         Ingredient i = ingredient.GetComponent<Ingredient>();
    //         //ingredientComp.CurrentState
    //         if (ingredient==null)
    //         {
    //             Debug.Log("트레이가 비어있음");
    //             return;

    //         }
    //         //Ingredient i = ingredient.GetComponent<Ingredient>();
    //         if (i.CurrentState == IngredientState.Raw)
    //         {
    //             i.Interact();
    //         }
    //         else if (i.CurrentState == IngredientState.Prepared)
    //         {
    //             GameObject playerController = GameObject.FindGameObjectWithTag("Player");
    //             if (playerController != null)
    //             {
    //                 Player_Controller controller = playerController.GetComponent<Player_Controller>();
    //                 if (controller != null && controller.handPosition != null)
    //                 {
    //                     GameObject newIngredient = Instantiate(ingredient, 
    //                         controller.handPosition.transform.position, 
    //                         controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0),
    //                         controller.handPosition.transform);
    //                     controller.isHandObject = newIngredient;
    //                     ingredientCount--;
    //                 }
    //             }
    //         }
    //     }

}

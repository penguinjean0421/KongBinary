using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FryPan : MonoBehaviour
{
    public GameObject ingredient;
    [SerializeField] private GameObject ingredientPos;
    [SerializeField] private float cookingTime = 3f; // 요리에 걸리는 시간
    private bool isCooking = false; // 현재 요리중인지 상태
   
    public void SetIngredient(GameObject newIngredient)
    {
        if (ingredient == null)
        {
            ingredient = newIngredient;
            ingredient.transform.position = ingredientPos.transform.position;
            ingredient.transform.rotation = ingredientPos.transform.rotation;
            ingredient.transform.parent = ingredientPos.transform;
        }
    }

    public void CookingFryPan()
    {
        if (ingredient == null || isCooking)
            return;

        Ingredient i = ingredient.GetComponent<Ingredient>();

        if (i.CurrentState == IngredientState.Prepared)
        {
            StartCoroutine(CookingCoroutine(i));
            //ingredient.Interact(); //코루틴말고 바로 해당스크립트 실행시 요리함
        }
        else if(i.CurrentState == IngredientState.Cooking)
        {
            GameObject playerController = GameObject.FindGameObjectWithTag("Player");
            if (playerController != null)
            {
                Player_Controller controller = playerController.GetComponent<Player_Controller>();
                if (controller != null && controller.handPosition != null)
                {
                    ingredient.transform.SetParent(controller.handPosition.transform);
                    ingredient.transform.position = controller.handPosition.transform.position;
                    ingredient.transform.rotation = controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
                    controller.isHandObject = ingredient;
                    ingredient = null;
                }
            }
        }
    }

    private IEnumerator CookingCoroutine(Ingredient ingredient)
    {
        isCooking = true;
        // 플레이어 상호작용 잠금
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player_Controller playerController = playerObj.GetComponent<Player_Controller>();
        if (playerController != null)
        {
            playerController.isInteracting = true;
        }
        
        yield return new WaitForSeconds(cookingTime);
        
        if (ingredient != null)
        {
            ingredient.Interact();
            if(ingredient.CompareTag("Fish"))
            {
                ingredient.menu = Food_Menu.menu.fishSteak;
            }
            else if(ingredient.CompareTag("Meat"))
            {
                ingredient.menu = Food_Menu.menu.meatSteak;
            }
            
        }
        
        // 플레이어 상호작용 잠금 해제
        if (playerController != null)
        {
            playerController.isInteracting = false;
        }
        isCooking = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutting_Board : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject ingredient;
    [SerializeField] private GameObject ingredientPos;
    [SerializeField] private float cutingTime = 3f; // 요리에 걸리는 시간
    private bool isCuting = false; // 현재 요리중인지 상태
    public void SetIngredient(GameObject newIngredient)
    {
       
        if (ingredient == null)
        {
            
            ingredient = Instantiate(newIngredient, 
                ingredientPos.transform.position, 
                ingredientPos.transform.rotation * Quaternion.Euler(0, 90, 0), 
                ingredientPos.transform);
            
            // Check the tag of the instantiated ingredient
             //Debug.Log(ingredient.CompareTag("Fish"));
          
        }
     
    }
    public void CleanIngredient()
    {
        if (ingredient.CompareTag("Fish"))
        {
            Debug.Log("Fish ingredient placed on cutting board");
        }
        else if (ingredient.CompareTag("Meat"))
        {
            Debug.Log("Meat ingredient placed on cutting board");
        }
        else
        {
            Debug.Log("Other ingredient placed on cutting board: " + ingredient.tag);
        }
    }

    public GameObject PickUpCleanIngredient()
    {
        return ingredient;
    }

     public void UsingCuttingBoard()
   {
        Ingredient i = ingredient.GetComponent<Ingredient>();
        //ingredientComp.CurrentState
        if (ingredient==null||isCuting)
        {
            Debug.Log("트레이가 비어있음");
            return;

        }
        //Ingredient i = ingredient.GetComponent<Ingredient>();
        if (i.CurrentState == IngredientState.Raw)
        {
            StartCoroutine(CutingCoroutine(i));

            //i.Interact();
        }
        else if (i.CurrentState == IngredientState.Prepared)
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
    private IEnumerator CutingCoroutine(Ingredient ingredient)
    {
        isCuting = true;
        // 플레이어 상호작용 잠금
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player_Controller playerController = playerObj.GetComponent<Player_Controller>();
        if (playerController != null)
        {
            playerController.isInteracting = true;
        }
        
        yield return new WaitForSeconds(cutingTime);
        
        if (ingredient != null)
        {
            ingredient.Interact();
        }
        
        // 플레이어 상호작용 잠금 해제
        if (playerController != null)
        {
            playerController.isInteracting = false;
        }
        isCuting = false;
    }

}

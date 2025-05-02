using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FryPan : MonoBehaviour
{
    public GameObject ingredient;
    [SerializeField] private GameObject ingredientPos;
    [SerializeField] private float cookingTime = 3f; // 요리에 걸리는 시간
    [SerializeField] private Slider timerBar; // 타이머바 UI

    private bool isCooking = false; // 현재 요리중인지 상태
   
    void Start()
    {
        // 타이머바 초기 비활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }
    }
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

    public void CookingFryPan(GameObject playerController)
    {
        if (ingredient == null || isCooking)
            return;

        Ingredient i = ingredient.GetComponent<Ingredient>();

        if (i.CurrentState == IngredientState.Prepared)
        {
            Player_Movement playerMovement = playerController.GetComponent<Player_Movement>();
            if (playerMovement != null)
            {
                StartCoroutine(CookingCoroutine(i, playerMovement));
            }
        }
        else if(i.CurrentState == IngredientState.Cooking)
        {
            //GameObject playerController = GameObject.FindGameObjectWithTag("Player");
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

    private IEnumerator CookingCoroutine(Ingredient ingredient, Player_Movement playerMovement)
    {
        isCooking = true;
        // 플레이어 상호작용 잠금
       // GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player_Controller playerController = playerMovement.GetComponent<Player_Controller>();
        if (playerController != null)
        {
            playerController.isInteracting = true;
        }
         // 타이머바 활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(true);
            timerBar.value = 0f;
        }

         // 시간 경과에 따라 타이머바 업데이트
        float elapsedTime = 0f;
        while (elapsedTime < cookingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cookingTime;
            if (timerBar != null)
            {
                timerBar.value = progress; // 0에서 1로 채워짐
            }
            yield return null;
        }

        //yield return new WaitForSeconds(cookingTime);
        
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
        
        // 타이머바 비활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }

        // 플레이어 상호작용 잠금 해제
        if (playerController != null)
        {
            playerController.isInteracting = false;
        }
        isCooking = false;
    }
}

/*
public class FryPan : MonoBehaviour
{
    public GameObject ingredient;
    [SerializeField] private GameObject ingredientPos;
    [SerializeField] private float cookingTime = 3f; // 요리에 걸리는 시간
    [SerializeField] private Slider timerBar; // 타이머바 UI
    private bool isCooking = false; // 현재 요리중인지 상태

    void Start()
    {
        // 타이머바 초기 비활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }
    }

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

    public void CookingFryPan(GameObject playerController)
    {
        if (ingredient == null || isCooking)
            return;

        Ingredient i = ingredient.GetComponent<Ingredient>();

        if (i.CurrentState == IngredientState.Prepared)
        {
            // 플레이어 타입 확인
            Player_Movement playerMovement = playerController.GetComponent<Player_Movement>();
            if (playerMovement != null)
            {
                StartCoroutine(CookingCoroutine(i, playerMovement));
            }
        }
        else if (i.CurrentState == IngredientState.Cooking)
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

    private IEnumerator CookingCoroutine(Ingredient ingredient, Player_Movement playerMovement)
    {
        isCooking = true;
        // 플레이어 상호작용 잠금
        Player_Controller playerController = playerMovement.GetComponent<Player_Controller>();
        if (playerController != null)
        {
            playerController.isInteracting = true;
        }

        // 타이머바 활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(true);
            timerBar.value = 0f;
        }

        // 시간 경과에 따라 타이머바 업데이트
        float elapsedTime = 0f;
        while (elapsedTime < cookingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cookingTime;
            if (timerBar != null)
            {
                timerBar.value = progress; // 0에서 1로 채워짐
            }
            yield return null;
        }

        // 요리 완료
        if (ingredient != null)
        {
            ingredient.Interact();
            if (ingredient.CompareTag("Fish"))
            {
                ingredient.menu = Food_Menu.menu.fishSteak;
            }
            else if (ingredient.CompareTag("Meat"))
            {
                ingredient.menu = Food_Menu.menu.meatSteak;
            }
        }

        // 타이머바 비활성화
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }

        // 플레이어 상호작용 잠금 해제
        if (playerController != null)
        {
            playerController.isInteracting = false;
        }
        isCooking = false;
    }
}
*/
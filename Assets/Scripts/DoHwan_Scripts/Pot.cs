using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Pot : MonoBehaviour
{
    public GameObject ingredient_1;
    public GameObject ingredient_2;

    [SerializeField] private GameObject ingredientPos_1;
    [SerializeField] private GameObject ingredientPos_2;
    [SerializeField] private float cookingTime = 3f;
    [SerializeField] private Slider timerBar;
    [SerializeField] private GameObject[] menuObject;

    private bool isCooking = false;

    void Start()
    {
        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }
    }

    public void SetIngredient(GameObject newIngredient)
    {
        if (newIngredient == null)
        {
            Debug.LogWarning("SetIngredient: newIngredient is null!");
            return;
        }

        Ingredient ingredient = newIngredient.GetComponent<Ingredient>();
        if (ingredient == null)
        {
            Debug.LogWarning($"SetIngredient: {newIngredient.name} does not have Ingredient component");
            return;
        }

        if (ingredient_1 == null)
        {
            ingredient_1 = newIngredient;
            ingredient_1.transform.position = ingredientPos_1.transform.position;
            ingredient_1.transform.rotation = ingredientPos_1.transform.rotation;
            ingredient_1.transform.parent = ingredientPos_1.transform;
            Debug.Log($"SetIngredient: ingredient_1 set as {ingredient_1.name}");
        }
        else if (ingredient_2 == null)
        {
            ingredient_2 = newIngredient;
            ingredient_2.transform.position = ingredientPos_2.transform.position;
            ingredient_2.transform.rotation = ingredientPos_2.transform.rotation;
            ingredient_2.transform.parent = ingredientPos_2.transform;
            Debug.Log($"SetIngredient: ingredient_2 set as {ingredient_2.name}");
        }
    }

    public void CookingPot(GameObject playerController)
    {
        if (ingredient_1 == null || isCooking || ingredient_2 == null)
            return;
        //Debug.Log("test1111");
        if (ingredient_1.CompareTag("Food"))
        {
            Debug.Log("여기서 요리 손으로");
            //GameObject playerController = GameObject.FindGameObjectWithTag("Player");
            if (playerController != null)
            {
                Player_Controller controller = playerController.GetComponent<Player_Controller>();
                if (controller != null && controller.handPosition != null)
                {
                    ingredient_1.transform.SetParent(controller.handPosition.transform);
                    ingredient_1.transform.position = controller.handPosition.transform.position;
                    ingredient_1.transform.rotation = controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
                    controller.isHandObject = ingredient_1;
                    Debug.Log($"CookingPot: Picked up cooked ingredient {ingredient_1.name}");
                    ingredient_1 = null;
                    ingredient_2 = null;
                    return;
                }
            }
        }
        //if (ingredient_1 == null || ingredient_2 == null || isCooking)
        if (isCooking)
        {
            Debug.LogWarning($"CookingPot: Cannot cook. ingredient_1: {(ingredient_1 == null ? "null" : "not null")}, ingredient_2: {(ingredient_2 == null ? "null" : "not null")}, isCooking: {isCooking}");
            return;
        }

        Ingredient i1 = ingredient_1.GetComponent<Ingredient>();
        Ingredient i2 = ingredient_2.GetComponent<Ingredient>();

        if (i1 == null || i2 == null)
        {
            Debug.LogError($"CookingPot: Ingredient component missing. i1: {i1}, i2: {i2}");
            return;
        }

        if (i1.CurrentState == IngredientState.Prepared && i2.CurrentState == IngredientState.Prepared)
        {
            StartCoroutine(CookingCoroutine(i1, i2));
        }
        /*
        else if (ingredient_1.CompareTag("Food"))
        {
            Debug.Log("여기서 요리 손으로");
            //GameObject playerController = GameObject.FindGameObjectWithTag("Player");
            if (playerController != null)
            {
                Player_Controller controller = playerController.GetComponent<Player_Controller>();
                if (controller != null && controller.handPosition != null)
                {
                    ingredient_1.transform.SetParent(controller.handPosition.transform);
                    ingredient_1.transform.position = controller.handPosition.transform.position;
                    ingredient_1.transform.rotation = controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
                    controller.isHandObject = ingredient_1;
                    Debug.Log($"CookingPot: Picked up cooked ingredient {ingredient_1.name}");
                    ingredient_1 = null;
                    ingredient_2 = null;
                }
            }
        }
        */
    }

    private IEnumerator CookingCoroutine(Ingredient i1, Ingredient i2)
    {
        isCooking = true;

        GameObject playerController = GameObject.FindGameObjectWithTag("Player");
        Player_Controller controller = playerController?.GetComponent<Player_Controller>();
        if (controller != null) //상호작용시 이동 재한하고 싶으면 isInteracting부분 주석해제
        {
            //controller.isInteracting = true;
        }

        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(true);
            timerBar.value = 0f;
        }

        float elapsedTime = 0f;
        while (elapsedTime < cookingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / cookingTime;
            if (timerBar != null)
            {
                timerBar.value = progress;
            }
            yield return null;
        }

        if (i1 != null && i2 != null)
        {
            i1.Interact();
            i2.Interact();

            //Food_Menu.menu newMenu = Food_Menu.menu.none;
            GameObject newFoodObject = null;

            if ((i1.ingredient == global::ingredient.Meat && i2.ingredient == global::ingredient.Carrot) ||
               (i1.ingredient == global::ingredient.Carrot && i2.ingredient == global::ingredient.Meat))
            {
                foreach (GameObject menuItem in menuObject)
                {
                    Food_State foodState = menuItem.GetComponent<Food_State>();
                    if (foodState != null && foodState.foodMenu == FoodMenu.braisedRibs)
                    {
                        newFoodObject = Instantiate(menuItem);
                        //foodState = foodState.foodMenu.braisedRibs;
                        break;
                    }
                }
            }


            // 그 외의 경우 기본 쓰레기 음식 생성
            else
            {
                newFoodObject = Instantiate(menuObject[0]);
                Debug.Log("CookingPot: Default trash food created");
            }

            Destroy(ingredient_1);
            Destroy(ingredient_2);
            ingredient_1 = null;
            ingredient_2 = null;

            if (newFoodObject != null)
            {
                ingredient_1 = newFoodObject;
                ingredient_1.transform.position = ingredientPos_1.transform.position;
                ingredient_1.transform.rotation = ingredientPos_1.transform.rotation;
                ingredient_1.transform.parent = ingredientPos_1.transform;

                Ingredient newIngredient = newFoodObject.GetComponent<Ingredient>();
                if (newIngredient != null)
                {
                    newIngredient.CurrentState = IngredientState.Cooking;
                    //newIngredient.menu = newMenu;
                    Debug.Log($"CookingPot: New ingredient {newFoodObject.name} created with menu ");
                }
                else
                {
                    Debug.LogWarning($"CookingPot: New ingredient {newFoodObject.name} does not have Ingredient component");
                }
            }
            else
            {
                Debug.LogWarning("CookingPot: Could not find matching FoodMenu in menuObject array");
            }
        }

        if (timerBar != null)
        {
            timerBar.gameObject.SetActive(false);
            timerBar.value = 0f;
        }

        if (controller != null)
        {
            //controller.isInteracting = false;
        }
        isCooking = false;
    }
}




/*
public class Pot : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ingredient_1;
    public GameObject ingredient_2;

    [SerializeField] private GameObject ingredientPos_1;
    [SerializeField] private GameObject ingredientPos_2;
    public void SetIngredient(GameObject newIngredient)
    {
        if (ingredient_1 == null)
        {
            ingredient_1 = newIngredient;
            ingredient_1.transform.position = ingredientPos_1.transform.position;
            ingredient_1.transform.rotation = ingredientPos_1.transform.rotation;
            ingredient_1.transform.parent = ingredientPos_1.transform;
        }
        else if(ingredient_2 == null)
        {
            ingredient_2 = newIngredient;
            ingredient_2.transform.position = ingredientPos_2.transform.position;
            ingredient_2.transform.rotation = ingredientPos_2.transform.rotation;
            ingredient_2.transform.parent = ingredientPos_2.transform;
        }
    }
    public void CookingPot()
    {
        Debug.Log("냄비요리");
        //재료 둘다 있어야만 요리리
         if (ingredient_1==null||ingredient_2==null)
            return;
        
        Ingredient i_1 = ingredient_1.GetComponent<Ingredient>();
        Ingredient i_2 = ingredient_2.GetComponent<Ingredient>();
        //여기에 요리로 변하는거 들어가야됨

    }
}
*/
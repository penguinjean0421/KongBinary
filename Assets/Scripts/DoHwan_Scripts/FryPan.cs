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
    [SerializeField] private GameObject[] menuObject;
    [SerializeField] private GameObject fireEffect;

    private bool isCooking = false; // 현재 요리중인지 상태
   
    void Start()
    {
        fireEffect.SetActive(false);
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

        if (ingredient.CompareTag("Ingredient"))
        {
            if (i.CurrentState == IngredientState.Prepared)//if(ingredient.CompareTag("Food"))
            {
                Player_Movement playerMovement = playerController.GetComponent<Player_Movement>();
                if (playerMovement != null)
                {
                    StartCoroutine(CookingCoroutine(i, playerMovement));
                }
            }
        }
        //else if(i.CurrentState == IngredientState.Cooking)
        else if (ingredient.CompareTag("Food"))
        {
            //GameObject playerController = GameObject.FindGameObjectWithTag("Player");
            //Debug.Log("test1");
            if (playerController != null)
            {
               // Debug.Log("test2");
                Player_Controller controller = playerController.GetComponent<Player_Controller>();
                if (controller != null && controller.handPosition != null && controller.isHandObject != null && controller.isHandObject.CompareTag("Dish"))
                {

                    /*
                    //Debug.Log("test3");
                    Destroy(controller.isHandObject); // "Dish" 태그 오브젝트 제거
                    controller.isHandObject = null; // 초기화

                    ingredient.transform.SetParent(controller.handPosition.transform);
                    ingredient.transform.position = controller.handPosition.transform.position;
                    ingredient.transform.rotation = controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
                    controller.isHandObject = ingredient;
                    ingredient = null;
                    */

                    //FindChildRecursive(controller.isHandObject.transform,"Pos").SetParent(controller.handPosition.transform);
                    //FindChildRecursive(controller.isHandObject.transform, "Pos").position = controller.handPosition.transform.position;
                    //FindChildRecursive(controller.isHandObject.transform, "Pos").rotation = controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
                    //controller.isHandObject = ingredient;
                    //ingredient = null;

                    ingredient.transform.SetParent(FindChildRecursive(controller.isHandObject.transform, "Pos"));
                    ingredient.transform.position = FindChildRecursive(controller.isHandObject.transform, "Pos").position;
                    ingredient.transform.rotation = FindChildRecursive(controller.isHandObject.transform, "Pos").rotation * Quaternion.Euler(0, 90, 0);
                    //controller.isHandObject = ingredient;
                    ingredient = null;
                }
            }
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        // 부모 오브젝트의 이름이 찾으려는 이름과 일치하면 해당 transform 반환
        if (parent.name == childName)
        {
            return parent;
        }

        // 부모의 모든 자식 오브젝트를 순회하며 재귀적으로 탐색
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, childName);
            // 재귀 호출 결과 찾은 자식이 있다면 바로 반환
            if (result != null)
            {
                return result;
            }
        }

        // 모든 계층을 탐색했으나 찾지 못할 경우 null 반환
        return null;
    }

    private IEnumerator CookingCoroutine(Ingredient _ingredient, Player_Movement playerMovement)
    {
        
        isCooking = true;
        fireEffect.SetActive(true);
        // 플레이어 상호작용 잠금
        // GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player_Controller playerController = playerMovement.GetComponent<Player_Controller>();
        if (playerController != null)
        {
            //playerController.isInteracting = true;
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

       
        if (_ingredient != null)
        {
            // Interact 호출 및 menu 설정
            _ingredient.Interact();
            /*
            if (_ingredient.CompareTag("Fish"))
            {
                _ingredient.menu = Food_Menu.menu.fishSteak;
            }
            else if (_ingredient.CompareTag("Meat"))
            {
                _ingredient.menu = Food_Menu.menu.meatSteak;
            }
            */

            // 기존 ingredient 삭제
            //Destroy(_ingredient.gameObject);
            //ingredient = null;

            // menuObject 배열에서 새로운 푸드 오브젝트 생성
            GameObject newFoodObject = null;
            if (_ingredient.ingredient == global::ingredient.Meat)
            {
                // menuObject에서 FoodMenu가 meatSteak인 오브젝트 찾기
                foreach (GameObject menuItem in menuObject)
                {
                    Food_State foodState = menuItem.GetComponent<Food_State>();
                    if (foodState != null && foodState.foodMenu == FoodMenu.meatSteak)
                    {
                        newFoodObject = Instantiate(menuItem);
                        break;
                    }
                }
            }
            else if (_ingredient.ingredient == global::ingredient.Fish)
            {
                // menuObject에서 FoodMenu가 fishSteak인 오브젝트 찾기
                foreach (GameObject menuItem in menuObject)
                {
                    Food_State foodState = menuItem.GetComponent<Food_State>();
                    if (foodState != null && foodState.foodMenu == FoodMenu.fishSteak)
                    {
                        newFoodObject = Instantiate(menuItem);
                        break;
                    }
                }
            }
            else
            {
                newFoodObject = Instantiate(menuObject[0]);//0번에는 항상 쓰래기음식 있음
            }

            // 새로운 오브젝트를 프라이팬 위치에 배치
            if (newFoodObject != null)
            {
                Destroy(ingredient.gameObject);
                ingredient = null;
                SetIngredient(newFoodObject);
                // 새로운 ingredient의 상태를 Cooking으로 설정
                //Ingredient newIngredient = newFoodObject.GetComponent<Ingredient>();
                //if (newIngredient != null)
                //{
                //    newIngredient.CurrentState = IngredientState.Cooking;
                //    newIngredient.menu = _ingredient.menu; // 기존 ingredient의 메뉴 정보 유지
                //}
            }
            else
            {
                Debug.LogWarning("해당 FoodMenu에 맞는 오브젝트를 menuObject 배열에서 찾을 수 없습니다.");
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
            //playerController.isInteracting = false;
        }
        isCooking = false;
        fireEffect.SetActive(false);
    }
}


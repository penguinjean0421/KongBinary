using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [SerializeField] public GameObject handPosition;

    private Food_Item food_Item = null;
    public GameObject isHandObject = null;

    public bool isInTrigger = false;
    private List<Collider> currentTriggers = new List<Collider>(); // 여러 트리거 저장
    private Collider currentTrigger = null; // 가장 가까운 트리거
    private Rigidbody rb;

    public bool isInteracting = false; // 플레이어가 요리 등의 행동 중인지 확인

    private Player_Movement playerMovement;

    private void Start()
    {
        playerMovement = this.GetComponent<Player_Movement>();
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // 입력 처리는 Player_Movement로 이동됨
        // 여기서는 상호작용 관련 상태만 관리
        // UpdateNearestTrigger 호출 제거
    }

    void FixedUpdate()
    {
        // FixedUpdate에서 가장 가까운 트리거 갱신
        UpdateNearestTrigger();
    }

    public void OnTag()
    {
        if (currentTrigger == null) return;

        if (currentTrigger.gameObject.CompareTag("ingredientTable"))
        {
            IngredientTableInteraction();
        }
        else if (currentTrigger.gameObject.CompareTag("CuttingBoard"))
        {
            CuttingBoardInteraction();
        }
        else if (currentTrigger.gameObject.CompareTag("WasteBasket"))
        {
            ClearHandObject();
        }
        else if (currentTrigger.gameObject.CompareTag("FryPan"))
        {
            UsingFryPan();
        }
        else if (currentTrigger.gameObject.CompareTag("Pot"))
        {
            UsingPot();
        }
        else if (currentTrigger.gameObject.CompareTag("FinishedTable"))
        {
            FinishedTable();
        }
        else if (currentTrigger.gameObject.CompareTag("Table"))
        {
            Table table = currentTrigger.gameObject.GetComponent<Table>();
            if (table != null)
            {
                table.Interact(this); // Player_Controller 인스턴스 전달
            }
        }
        if (isInteracting)
        {
            if (currentTrigger != null)
            {
                // currentTrigger의 위치를 바라보게 회전
                Vector3 lookPos = currentTrigger.transform.position - transform.position;
                lookPos.y = 0; // y축 회전만 하도록(고개를 숙이거나 들지 않게)
                if (lookPos != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookPos);
                }
            }
        }
    }

    #region Triggers

    private void OnTriggerEnter(Collider other)
    {
        // 트리거에 들어갈 때 상태 설정
        isInTrigger = true;
        if (!currentTriggers.Contains(other))
        {
            currentTriggers.Add(other);
        }
        UpdateNearestTrigger();
    }

    private void OnTriggerStay(Collider other)
    {
        // 트리거 안에 있는 동안 상태 유지
        if (!currentTriggers.Contains(other))
        {
            currentTriggers.Add(other);
        }
        UpdateNearestTrigger();
    }

    private void OnTriggerExit(Collider other)
    {
        // 트리거에서 나가면 상태 초기화
        currentTriggers.Remove(other);
        if (currentTriggers.Count == 0)
        {
            isInTrigger = false;
            currentTrigger = null;
        }
        else
        {
            UpdateNearestTrigger();
        }
    }

    private void UpdateNearestTrigger()
    {
        if (currentTriggers.Count == 0)
        {
            currentTrigger = null;
            return;
        }
        
        float shortestDistance = float.MaxValue;
        Collider nearestTrigger = null;

        foreach (Collider trigger in currentTriggers)
        {
            if (trigger != null)
            {
                float distance = (trigger.transform.position - transform.position).sqrMagnitude; // 성능 최적화
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestTrigger = trigger;
                }
            }
        }

        currentTrigger = nearestTrigger;
    }
    #endregion

    #region Interaction

    private void IngredientTableInteraction()
    {
        Food_Ingredient_Tray food_Ingredient_Try = currentTrigger.gameObject.GetComponent<Food_Ingredient_Tray>();
        if (isHandObject != null || food_Ingredient_Try.ingredientCount < 1)
            return;

        isHandObject = Instantiate(food_Ingredient_Try.ingredient,
                    handPosition.transform.position,
                    handPosition.transform.rotation * Quaternion.Euler(0, 90, 0),
                    handPosition.transform);
        food_Ingredient_Try.ingredientCount--;
        food_Ingredient_Try.AnimationPlayer();
    }

    private void CuttingBoardInteraction()
    {
        Cutting_Board cuttingBoard = currentTrigger.gameObject.GetComponent<Cutting_Board>();

        if (isHandObject != null && cuttingBoard.ingredient == null)
        {
            if (isHandObject.CompareTag("Food"))
                return;
            Ingredient i = isHandObject.GetComponent<Ingredient>();
            if (i.CurrentState == IngredientState.Raw)
            {
                cuttingBoard.SetIngredient(isHandObject);
                Destroy(isHandObject);
                isHandObject = null;
            }
        }
        else if (isHandObject == null && cuttingBoard.ingredient != null)
        {
            cuttingBoard.UsingCuttingBoard(this.gameObject);
        }
    }

    private void ClearHandObject()
    {
        Destroy(isHandObject);
        isHandObject = null;
    }

    private void UsingFryPan()
    {
        FryPan fryPan = currentTrigger.gameObject.GetComponent<FryPan>();
        if (fryPan == null) return;

        if (isHandObject != null)
        {
            if (isHandObject.CompareTag("Food"))
                return;
            else if (isHandObject.CompareTag("Ingredient"))
            {
                Ingredient ingredient = isHandObject.GetComponent<Ingredient>();
                if (ingredient != null && ingredient.CurrentState == IngredientState.Prepared)
                {
                    //FryPan fryPan = currentTrigger.gameObject.GetComponent<FryPan>();
                    if (fryPan != null && fryPan.ingredient == null)
                    {
                        fryPan.SetIngredient(isHandObject);
                        isHandObject.transform.parent = null;
                        isHandObject = null;
                    }
                }
            }
            else if (isHandObject.CompareTag("Dish"))
            {
                fryPan.CookingFryPan(this.gameObject);
            }
        }
        else if (isHandObject == null)
        {
            //FryPan fryPan = currentTrigger.gameObject.GetComponent<FryPan>();
            if (fryPan != null && fryPan.ingredient != null)
            {
                fryPan.CookingFryPan(this.gameObject);
            }
        }
    }

    private void UsingPot()
    {
        Pot pot = currentTrigger.gameObject.GetComponent<Pot>();
        if (pot == null) return;

        if (isHandObject != null)
        {
            if (isHandObject.CompareTag("Food"))
                return;
            else if (isHandObject.CompareTag("Ingredient"))
            {
                Ingredient ingredient = isHandObject.GetComponent<Ingredient>();
                if (ingredient != null && ingredient.CurrentState == IngredientState.Prepared)
                {
                   
                    if (pot != null && (pot.ingredient_1 == null || pot.ingredient_2 == null))
                    {
                        pot.SetIngredient(isHandObject);
                        isHandObject.transform.parent = null;
                        isHandObject = null;
                    }
                }
            }
            else if (isHandObject.CompareTag("Dish"))
            {
                pot.CookingPot(this.gameObject);
            }
        }
        else if (isHandObject == null )
        {
          
            if (pot != null)
            {
                pot.CookingPot(this.gameObject);
            }
        }
    }

    private void FinishedTable()
    {
        /*
        if (isHandObject != null)
        {
            Finished_Table finishedTable = currentTrigger.gameObject.GetComponent<Finished_Table>();
            if (finishedTable != null)
            {
                if (isHandObject.CompareTag("Food"))
                {
                    finishedTable.Finished(isHandObject);
                    isHandObject = null;
                }
              
            }
        }
        */
        if (isHandObject != null)
        {
            Finished_Table finishedTable = currentTrigger.gameObject.GetComponent<Finished_Table>();
            if (finishedTable != null)
            {
                if (isHandObject.CompareTag("Dish"))
                {
                    //finishedTable.Finished(isHandObject);
                    //isHandObject = null;
                    GameObject food = FindChildWithTag(isHandObject.transform, "Food");
                    if (food != null) 
                    {
                        finishedTable.Finished(food);
                        Destroy(isHandObject);
                        isHandObject = null;
                        
                    }
                }

            }
        }

    }

    public GameObject FindChildWithTag(Transform parent, string tagToFind)
    {
        // 부모 오브젝트 자체가 지정한 태그를 가지고 있는지 확인합니다.
        if (parent.CompareTag(tagToFind))
        {
            return parent.gameObject;
        }

        // 부모의 모든 자식 오브젝트를 순회합니다.
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            // 현재 자식 오브젝트가 지정한 태그를 가지고 있는지 확인합니다.
            if (child.CompareTag(tagToFind))
            {
                return child.gameObject;
            }

            // 자식 오브젝트의 자식들을 재귀적으로 탐색합니다.
            GameObject found = FindChildWithTag(child, tagToFind);
            if (found != null)
            {
                return found;
            }
        }

        // 지정한 태그를 가진 오브젝트가 없다면 null을 반환합니다.
        return null;
    }

    #endregion
}
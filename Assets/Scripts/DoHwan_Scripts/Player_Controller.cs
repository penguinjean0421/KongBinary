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

    // 강조 효과 관련 변수
    [SerializeField] private Color highlightColor = Color.yellow; // 강조 색상
    [SerializeField] private float highlightIntensity = 1f; // 발광 강도
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>(); // 원본 재질 저장

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
    }

    void FixedUpdate()
    {
        
        // FixedUpdate에서 가장 가까운 트리거 갱신 및 강조 효과 적용
        UpdateNearestTrigger();
        UpdateHighlight();
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
                lookPos.y = 0; // y축 회전만 하도록
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
        if (other != null && !currentTriggers.Contains(other))
        {
            currentTriggers.Add(other);
            isInTrigger = true;
            UpdateNearestTrigger();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other != null && !currentTriggers.Contains(other))
        {
            currentTriggers.Add(other);
        }
        UpdateNearestTrigger();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            currentTriggers.Remove(other);
            RemoveHighlight(other.gameObject); // 트리거 나갈 때 즉시 강조 제거
        }
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
        // 유효하지 않은 트리거 제거
        currentTriggers.RemoveAll(trigger => trigger == null || !trigger.gameObject.activeInHierarchy);

        if (currentTriggers.Count == 0)
        {
            currentTrigger = null;
            return;
        }

        float shortestDistance = float.MaxValue;
        Collider nearestTrigger = null;

        foreach (Collider trigger in currentTriggers)
        {
            if (trigger != null && trigger.gameObject.activeInHierarchy)
            {
                float distance = (trigger.transform.position - transform.position).sqrMagnitude;
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

    #region Highlight

    private void UpdateHighlight()
    {
        // 모든 트리거의 강조 효과 제거
        foreach (Collider trigger in currentTriggers.ToArray()) // 복사본 사용
        {
            if (trigger != null && trigger != currentTrigger && trigger.gameObject != null)
            {
                RemoveHighlight(trigger.gameObject);
            }
        }

        // 가장 가까운 트리거에 강조 효과 적용
        if (currentTrigger != null && currentTrigger.gameObject != null)
        {
            ApplyHighlight(currentTrigger.gameObject);
        }
    }

    private void ApplyHighlight(GameObject target)
    {
        // 자식 포함 모든 Renderer 가져오기 (비활성화된 오브젝트 포함)
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            Debug.LogWarning($"No renderers found in {target.name}");
            return;
        }

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            // 원본 재질 저장 (최초 강조 시)
            if (!originalMaterials.ContainsKey(renderer))
            {
                originalMaterials[renderer] = new Material(renderer.material); // 원본 재질 복사
                Debug.Log($"Stored original material for {renderer.gameObject.name}");
            }

            Material material = renderer.material;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }
    }

    private void RemoveHighlight(GameObject target)
    {
        // 자식 포함 모든 Renderer 가져오기 (비활성화된 오브젝트 포함)
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0) return;

        foreach (Renderer renderer in renderers)
        {
            if (renderer != null && originalMaterials.ContainsKey(renderer))
            {
                // 원본 재질로 복구
                renderer.material = new Material(originalMaterials[renderer]);
                Debug.Log($"Restored material for {renderer.gameObject.name}");
            }
        }
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
            if (isHandObject.CompareTag("Ingredient"))
            {
                Ingredient i = isHandObject.GetComponent<Ingredient>();
                if (i.CurrentState == IngredientState.Raw)
                {
                    cuttingBoard.SetIngredient(isHandObject);
                    Destroy(isHandObject);
                    isHandObject = null;
                }
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
        else if (isHandObject == null)
        {
            if (pot != null)
            {
                pot.CookingPot(this.gameObject);
            }
        }
    }

    private void FinishedTable()
    {
        if (isHandObject != null)
        {
            Finished_Table finishedTable = currentTrigger.gameObject.GetComponent<Finished_Table>();
            if (finishedTable != null)
            {
                if (isHandObject.CompareTag("Dish"))
                {
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
        if (parent.CompareTag(tagToFind))
        {
            return parent.gameObject;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.CompareTag(tagToFind))
            {
                return child.gameObject;
            }
            GameObject found = FindChildWithTag(child, tagToFind);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    #endregion
}
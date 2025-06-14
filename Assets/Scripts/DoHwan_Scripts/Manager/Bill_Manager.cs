using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Collections;



public class Bill_Manager : MonoBehaviour
{
    [System.Serializable]
    public class BillStack
    {
        public FoodMenu menu;
        public int stackCount;
    }

    [SerializeField] private GameObject[] billPrefabs; // 빌지 프리팹 배열
    [SerializeField] private Transform billParent; // 빌지가 생성될 부모 Canvas
    [SerializeField] private float totalGameTime = 120f; // 총 게임 시간 (2분)
    [SerializeField] private BillStack[] initialBillStacks; // 인스펙터에서 설정할 초기 스택
    private Dictionary<FoodMenu, int> billStacks = new Dictionary<FoodMenu, int>(); // 각 메뉴의 남은 스택
    private List<GameObject> bills = new List<GameObject>();
    private float gameTime;
    private float timer;

    [SerializeField] private float xOffset = 110f;
    [SerializeField] private float yOffset = 950f;
    [SerializeField] private float spacing = 180f; // 빌지 간 간격 (픽셀 단위)
    [SerializeField] private float moveDuration = 1f; // 이동 지속 시간 (초)

    [SerializeField] private int maxBills=5;

    [SerializeField] private Slider slider;
    //private float decreaseRate = 0.03f;

    void Start()
    {
        if (billPrefabs == null || billPrefabs.Length == 0)
        {
            Debug.LogError("BillManager: billPrefabs array is empty or not assigned!");
            return;
        }
        for (int i = 0; i < billPrefabs.Length; i++)
        {
            if (billPrefabs[i] == null)
            {
                Debug.LogError($"BillManager: billPrefabs[{i}] is null!");
            }
        }

        // 인스펙터에서 설정한 초기 스택으로 초기화
        foreach (var stack in initialBillStacks)
        {
            if (stack.menu != FoodMenu.None && stack.stackCount > 0)
            {
                billStacks[stack.menu] = stack.stackCount;
            }
        }

        if (slider != null)
        {
            // 슬라이더 초기값 설정 (최대값 1로 꽉 채움)
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
        }

        gameTime = totalGameTime;
        timer = 0f;

        // 초기 2개 랜덤 생성 (2:00 ~ 1:50)
        SpawnInitialBills();
    }

    void Update()
    {
        gameTime -= Time.deltaTime;
        timer -= Time.deltaTime;

        // 시간대별 빌지 생성
        if (gameTime > 60f) // [2:00 ~ 1:50] (10초)
        {
            if (gameTime <= 110f && timer <= 0 && CanSpawnBill())
            {
                SpawnRandomBill();
                timer = 5f; // 5초 간격으로 2개 생성 (10초 내 2번)
            }
        }
        else if (gameTime > 30f) // [1:50 ~ 0:30] (80초)
        {
            if (timer <= 0 && CanSpawnBill())
            {
                SpawnRandomBill();
                timer = 10f; // 10초당 1개 (총 8개)
            }
        }
        else // [0:30 ~ 0:00] (30초)
        {
            if (timer <= 0 && CanSpawnBill())
            {
                SpawnRandomBill();
                timer = 5f; // 5초당 1개 (총 6개)
            }
        }

        if (timer <= 0)
        {
            timer = 0f;
        }
    }

    void SpawnInitialBills()
    {
        for (int i = 0; i < 2; i++)
        {
            if (CanSpawnBill())
            {
                SpawnRandomBill();
            }
        }
    }

    void SpawnRandomBill()
    {
        if (!CanSpawnBill()) return;

        // 남은 스택이 있는 메뉴만 대상으로 랜덤 선택
        List<FoodMenu> availableMenus = new List<FoodMenu>();
        foreach (var kvp in billStacks)
        {
            if (kvp.Value > 0)
            {
                availableMenus.Add(kvp.Key);
            }
        }

        if (availableMenus.Count == 0) return;

        FoodMenu randomMenu = availableMenus[Random.Range(0, availableMenus.Count)];
        int randomIndex = -1;
        for (int i = 0; i < billPrefabs.Length; i++)
        {
            Food_State foodState = billPrefabs[i].GetComponent<Food_State>();
            if (foodState != null && foodState.foodMenu == randomMenu)
            {
                randomIndex = i;
                break;
            }
        }

        if (randomIndex == -1) return;

        GameObject billPrefab = billPrefabs[randomIndex];
        GameObject newBill = Instantiate(billPrefab, billParent);
        bills.Add(newBill);

        billStacks[randomMenu]--; // 스택 감소
        Debug.Log($"Spawned bill: {randomMenu}, Remaining stacks - {randomMenu}: {billStacks[randomMenu]}");

        // 오른쪽에서 이동 시작
        StartCoroutine(MoveBill(newBill));
    }

    IEnumerator MoveBill(GameObject bill)
    {
        RectTransform rectTransform = bill.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;

        // 초기 위치 (오른쪽 밖)
        float startX = 2500F;
        rectTransform.anchoredPosition = new Vector2(startX, yOffset);

        // 목표 위치 계산 (가장 오른쪽에 추가)
        float targetX = xOffset + (bills.Count - 1) * spacing;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(startX, yOffset), new Vector2(targetX, yOffset), t);
            yield return null;
        }

        // 이동 완료 후 정렬
        StartCoroutine(SmoothAlignBills());
    }

    IEnumerator SmoothAlignBills()
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            for (int i = 0; i < bills.Count; i++)
            {
                RectTransform rectTransform = bills[i].GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float targetX = xOffset + i * spacing;
                    Vector2 startPos = rectTransform.anchoredPosition;
                    rectTransform.anchoredPosition = Vector2.Lerp(startPos, new Vector2(targetX, yOffset), t);
                }
            }

            yield return null;
        }

        // 최종 정렬
        UpdateBillPositions();
    }

    bool CanSpawnBill()
    {
        return bills.Count < maxBills && billStacks.Values.Sum() > 0;
    }

    void UpdateBillPositions()
    {
        for (int i = 0; i < bills.Count; i++)
        {
            RectTransform rectTransform = bills[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(xOffset + i * spacing, yOffset);
            }
        }
    }

    public void CompleteBill(FoodMenu menuType)
    {

        for (int i = 0; i < bills.Count; i++)
        {
            Food_State foodState = bills[i].GetComponent<Food_State>();
            //float f = bills[i].GetComponent<Bill>().currentTime;
            if (foodState != null && foodState.foodMenu == menuType)
            {
               
                GameManager.Instance.AddSales(foodState.price);
               

                Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");

                Destroy(bills[i]);
                bills.RemoveAt(i);
                StartCoroutine(SmoothAlignBills());
                Debug.Log($"BillManager: Removed bill with FoodMenu {menuType}");

                // 빌지가 1개 남았을 때 1개 랜덤 생성
                if (bills.Count == 0 && billStacks.Values.Sum() > 0)
                {
                    SpawnRandomBill();
                }
                return; // 첫 번째 일치 항목만 제거
            }
        }
        BossHp(0.03f);
        Debug.LogWarning($"BillManager: No bill found with FoodMenu {menuType}"); // 잘못 제출
    }
    public void FailBill(FoodMenu menuType)
    {
        for (int i = 0; i < bills.Count; i++)
        {
            Food_State foodState = bills[i].GetComponent<Food_State>();
            //float f = bills[i].GetComponent<Bill>().currentTime;
            if (foodState != null && foodState.foodMenu == menuType)
            {
                BossHp(0.05f);

                //if(f>0)
                GameManager.Instance.AddSales(-50f);
                // else if(f<=0)
                //{
                //    GameManager.Instance.AddSales(foodState.price*(2f/5f));
                //}
                //Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");

                Destroy(bills[i]);
                bills.RemoveAt(i);
                StartCoroutine(SmoothAlignBills());
                Debug.Log($"BillManager: Removed bill with FoodMenu {menuType}");

                // 빌지가 1개 남았을 때 1개 랜덤 생성
                if (bills.Count == 0 && billStacks.Values.Sum() > 0)
                {
                    SpawnRandomBill();
                }
                return; // 첫 번째 일치 항목만 제거
            }
        }
        Debug.LogWarning($"BillManager: No bill found with FoodMenu {menuType}"); // 잘못 제출
    }
    private void BossHp(float f)
    {
        if (slider != null)
        {
            slider.value = Mathf.Max(0f, slider.value - f);
            if (slider.value <= 0f)
            {
                //게임오버
            }
        }
       
    }
}
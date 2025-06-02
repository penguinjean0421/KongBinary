using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Bill_Manager : MonoBehaviour
{
    [SerializeField] private GameObject[] billPrefabs; // 빌지 프리팹 배열
    [SerializeField] private Transform billParent; // 빌지가 생성될 부모 Canvas
    [SerializeField] private float spawnInterval = 10f; // 빌지 생성 간격 (초)
    [SerializeField] private int MAX_BILLS = 8; // 최대 빌지 개수
    private List<GameObject> bills = new List<GameObject>();
    private float timer;

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
        timer = spawnInterval;
        InvokeRepeating("SpawnBill", spawnInterval, spawnInterval);//일정시간마다 실행
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = spawnInterval;
        }

        // 빌지 위치 업데이트 (왼쪽 위에서 오른쪽으로 쌓임)
        UpdateBillPositions();
    }

    void SpawnBill()
    {
        if (bills.Count >= MAX_BILLS)
        {
            return;
            // 최대 개수 초과 시 가장 오래된 빌지 제거
            Destroy(bills[0]);
            bills.RemoveAt(0);
        }

        // 배열에서 랜덤으로 빌지 선택
        int randomIndex = Random.Range(0, billPrefabs.Length);
        GameObject billPrefab = billPrefabs[randomIndex];

        // 새로운 빌지 생성
        GameObject newBill = Instantiate(billPrefab, billParent);
        bills.Add(newBill);

        // 빌지 내용 예시 (프리팹에 따라 다를 수 있음)
        //Text billText = newBill.GetComponent<Text>();
        //if (billText != null)
        //{
        //    billText.text = $"Bill {bills.Count} - {GetRandomOrder()}";
        //}

        UpdateBillPositions();
    }

    //string GetRandomOrder()
    //{
    //    string[] orders = { "Pizza", "Steak", "Soup", "Salad" }; // 예시 주문 목록
    //    return orders[Random.Range(0, orders.Length)];
    //}

    void UpdateBillPositions()
    {
        float xOffset = 110f;
        float yOffset = 950f;
        float spacing = 180f; // 빌지 간 간격 (픽셀 단위)

        for (int i = 0; i < bills.Count; i++)
        {
            RectTransform rectTransform = bills[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
                xOffset += spacing; // 오른쪽으로 쌓임

                //if (xOffset > 700f) // 화면 너비 초과 시 줄 바꿈 (가정: 800px 화면)
                //{
                //    xOffset = 0f;
                //    yOffset -= spacing;
                //}
            }
        }
    }

    // 빌지 처리 (예: 완료 시 제거)
    //public void CompleteBill(int index)
    //{
    //    if (index >= 0 && index < bills.Count)
    //    {
    //        Destroy(bills[index]);
    //        bills.RemoveAt(index);
    //        UpdateBillPositions();
    //    }
    //}

    public void CompleteBill(FoodMenu menuType)
    {
        for (int i = 0; i < bills.Count; i++)
        {
            Food_State foodState = bills[i].GetComponent<Food_State>();
            if (foodState != null && foodState.foodMenu == menuType)
            {
                GameManager.Instance.AddSales(foodState.price);
                Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");

                Destroy(bills[i]);
                bills.RemoveAt(i);
                UpdateBillPositions();
                Debug.Log($"BillManager: Removed bill with FoodMenu {menuType}");
                return; // 첫 번째 일치 항목만 제거 (모두 제거하려면 return 제거)
            }
        }
        Debug.LogWarning($"BillManager: No bill found with FoodMenu {menuType}");//잘못제출
    }

}

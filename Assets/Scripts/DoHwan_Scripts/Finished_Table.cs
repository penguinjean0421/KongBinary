using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finished_Table : MonoBehaviour
{
    public Bill_Manager billManager; // BillManager 참조

    void Start()
    {
        // BillManager가 Inspector에서 할당되지 않은 경우 씬에서 찾기
        if (billManager == null)
        {
            billManager = FindObjectOfType<Bill_Manager>();
            if (billManager == null)
            {
                Debug.LogError("Finished_Table: BillManager not found in the scene!");
            }
        }
    }

    void Update()
    {
    }

    // 음식 제출 시 호출되는 메서드
    public void Finished(GameObject food)
    {
        //Food_State foodState = food.GetComponent<Food_State>();
        //if (foodState != null)
        //{
        //    // GameManager에 판매 금액 추가
        //    GameManager.Instance.AddSales(foodState.price);
        //    Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");

        //    // BillManager에서 해당 FoodMenu 빌지 제거
        //    if (billManager != null)
        //    {
        //        billManager.CompleteBill(foodState.foodMenu);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("Finished_Table: Cannot remove bill because BillManager is not assigned!");
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning("Finished_Table: Food_State component not found on the submitted food!");
        //}

        //Destroy(food); // 음식 오브젝트 제거

        if (food == null)
        {
            Debug.LogWarning("Finished_Table: Submitted food is null!");
            return;
        }
        Food_State foodState = food.GetComponent<Food_State>();
        if (foodState == null)
        {
            Debug.LogWarning("Finished_Table: Food_State component not found on the submitted food!");
            return;
        }
        if (billManager == null)
        {
            Debug.LogWarning("Finished_Table: billManager is not assigned!");
            return;
        }

        //GameManager.Instance.AddSales(foodState.price);
       //Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");
        billManager.CompleteBill(foodState.foodMenu);
        Destroy(food);
    }
}

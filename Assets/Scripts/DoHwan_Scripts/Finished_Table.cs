using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finished_Table : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /*
    public void Finished(Food_Menu.menu menu)
    {
        //Debug.Log("test finished");
        
            
        
        switch(menu)
        {
           case Food_Menu.menu.fishSteak:
           Debug.Log("생선스테이크 재출출");
           break;
            case Food_Menu.menu.meatSteak:
           Debug.Log("소고기스테이크 재출");
           break;
        }
    }
    */

    // 음식 제출 시 호출되는 메서드
    public void Finished(GameObject food)
    {
        Food_State foodState = food.GetComponent<Food_State>();
        if (foodState != null)
        {
            GameManager.Instance.AddSales(foodState.price);
            Debug.Log($"{foodState.foodMenu} 제출, 가격: {foodState.price}");
        }
        Destroy(food); // 음식 오브젝트 제거
    }
}

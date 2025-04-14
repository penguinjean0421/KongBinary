using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    [SerializeField] public GameObject handPosition;
   

    //private GameObject isHandObject = null;
    private Food_Item food_Item = null;
    public GameObject isHandObject = null;

    private bool isInTrigger = false;
    private Collider currentTrigger = null;
    private Rigidbody rb;

    public bool isInteracting = false; // 플레이어가 요리 등의 행동 중인지 확인

    private void Start()
    {
        
    }

  
    private void Update()
    {
        // 상호작용 중일 때는 새로운 상호작용을 막음
        if (isInteracting)
            return;

        if (Input.GetKeyDown(KeyCode.Space) && isInTrigger)
        {
            //Food_Ingredient_Tray food_Ingredient_Try = currentTrigger.gameObject.GetComponent<Food_Ingredient_Tray>();
           

            if(currentTrigger.gameObject.CompareTag("ingredientTable"))
            {
                IngredientTableInteraction();
            }
            if(currentTrigger.gameObject.CompareTag("CuttingBoard"))
            {
                CuttingBoardInteraction();
            }
            if (currentTrigger.gameObject.CompareTag("WasteBasket"))
            {
                ClearHandObject();
            }
            if (currentTrigger.gameObject.CompareTag("FryPan"))
            {
                UsingFryPan();
            }
            if (currentTrigger.gameObject.CompareTag("Pot"))
            {
                UsingPot();
            }
            if (currentTrigger.gameObject.CompareTag("FinishedTable"))
            {
                FinishedTable();
            }
            
        }

        // Q 키를 눌렀을 때 손에 있는 아이템을 비우기
     
    }
    

    #region Triggers

    private void OnTriggerEnter(Collider other)
    {
        // 트리거에 들어갈 때 상태 설정
        isInTrigger = true;
        currentTrigger = other;
    }


    private void OnTriggerStay(Collider other)
    {
        // 트리거 안에 있는 동안 상태 유지 (필요 시 디버깅용)
        isInTrigger = true;
        currentTrigger = other;
    }

    private void OnTriggerExit(Collider other)
    {
        // 트리거에서 나가면 상태 초기화
        isInTrigger = false;
        currentTrigger = null;
    }
    #endregion

    #region Interaction

    private void IngredientTableInteraction()
    {
        Food_Ingredient_Tray food_Ingredient_Try = currentTrigger.gameObject.GetComponent<Food_Ingredient_Tray>();
        if(isHandObject!=null||food_Ingredient_Try.ingredientCount<1)
                    return;
                //Debug.Log("Test");
                //food_Item = currentTrigger.gameObject.GetComponent<>
                //GameObject ingredient = Instantiate(food_ingredient.ingredient, handPosition.position, handPosition.rotation);
        //if(isHandObject!=null)
        isHandObject = Instantiate(food_Ingredient_Try.ingredient, 
                    handPosition.transform.position, 
                    handPosition.transform.rotation * Quaternion.Euler(0, 90, 0), 
                    handPosition.transform);
        food_Ingredient_Try.ingredientCount--;
        

        // if(isHandObject == null&&food_Ingredient_Try.ingredient!=null)
        // {
            
        // }

    }

    private void CuttingBoardInteraction()
    {
        Cutting_Board cuttingBoard = currentTrigger.gameObject.GetComponent<Cutting_Board>();
       

        if (isHandObject != null && cuttingBoard.ingredient == null)
        {
            Ingredient i = isHandObject.GetComponent<Ingredient>();
            if(i.CurrentState==IngredientState.Raw)
            {
                 //Cutting_Board cuttingBoard = currentTrigger.gameObject.GetComponent<Cutting_Board>();
                cuttingBoard.SetIngredient(isHandObject);
                Destroy(isHandObject);
                isHandObject = null;
            }
          
        }
        //else if()

        else if(isHandObject == null && cuttingBoard.ingredient != null)
        {
            cuttingBoard.CleanIngredient();
            cuttingBoard.UsingCuttingBoard();
        }
       
    }

    private void ClearHandObject()
    {
        //if (isHandObject == null)
            //return;
           
        
        Destroy(isHandObject);
        isHandObject = null;
    }

    private void UsingFryPan()
    {
        if (isHandObject != null)
        {
            Ingredient ingredient = isHandObject.GetComponent<Ingredient>();
            if (ingredient != null && ingredient.CurrentState == IngredientState.Prepared)
            {
                FryPan fryPan = currentTrigger.gameObject.GetComponent<FryPan>();
                if (fryPan != null && fryPan.ingredient == null)
                {
                    fryPan.SetIngredient(isHandObject);
                    isHandObject.transform.parent = null;
                    isHandObject = null;
                }
            }
        }
        else if(isHandObject == null)
        {
            FryPan fryPan = currentTrigger.gameObject.GetComponent<FryPan>();
            if (fryPan != null && fryPan.ingredient != null)
            {
                fryPan.CookingFryPan();
            }
        }
        
    }

    private void UsingPot()
    {
         if (isHandObject != null)
        {
            Ingredient ingredient = isHandObject.GetComponent<Ingredient>();
            if (ingredient != null && ingredient.CurrentState == IngredientState.Prepared)
            {
                Pot pot = currentTrigger.gameObject.GetComponent<Pot>();
                if (pot != null && (pot.ingredient_1 == null||pot.ingredient_2==null))
                {
                    pot.SetIngredient(isHandObject);
                    isHandObject.transform.parent = null;
                    isHandObject = null;
                }
            }
        }
        else if(isHandObject == null)
        {
            Pot pot = currentTrigger.gameObject.GetComponent<Pot>();
            if (pot != null && pot.ingredient_1 != null &&pot.ingredient_2!=null )
            {
                pot.CookingPot();
            }
        }
    }

    private void FinishedTable()
    {
        Ingredient i = isHandObject.GetComponent<Ingredient>();
        if(isHandObject!=null&&i.menu!=Food_Menu.menu.none)
        {
            currentTrigger.gameObject.GetComponent<Finished_Table>().Finished(i.menu);
            Destroy(isHandObject);
            isHandObject = null;
        }
         //   currentTrigger.gameObject.GetComponent<Finished_Table>().Finished(isHandObject);
    }

    #endregion

   
    
    

}

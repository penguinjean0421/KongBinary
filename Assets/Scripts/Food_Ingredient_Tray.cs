using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food_Ingredient_Tray : MonoBehaviour
{
   public GameObject ingredient;
   public int ingredientCount = 5;


/*
   public GameObject GetIngredient()
   {
       if (ingredientCount > 0)
       {
           ingredientCount--;
           return ingredient;
       }
       Debug.Log("트레이 비었음");
       return null;
   }
*/


   private void OnTriggerEnter(Collider other)
   {
    //fish.SetActive(false);
    //fish_tray.SetActive(true);
   }

//    public void UsingTray()
//    {
//         Ingredient i = ingredient.GetComponent<Ingredient>();
//         //ingredientComp.CurrentState
//         if (ingredient==null)
//         {
//             Debug.Log("트레이가 비어있음");
//             return;

//         }
//         //Ingredient i = ingredient.GetComponent<Ingredient>();
//         if (i.CurrentState == IngredientState.Raw)
//         {
//             i.Interact();
//         }
//         else if (i.CurrentState == IngredientState.Prepared)
//         {
//             GameObject playerController = GameObject.FindGameObjectWithTag("Player");
//             if (playerController != null)
//             {
//                 Player_Controller controller = playerController.GetComponent<Player_Controller>();
//                 if (controller != null && controller.handPosition != null)
//                 {
//                     GameObject newIngredient = Instantiate(ingredient, 
//                         controller.handPosition.transform.position, 
//                         controller.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0),
//                         controller.handPosition.transform);
//                     controller.isHandObject = newIngredient;
//                     ingredientCount--;
//                 }
//             }
//         }
//     }
   
}

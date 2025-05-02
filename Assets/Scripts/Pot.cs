using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

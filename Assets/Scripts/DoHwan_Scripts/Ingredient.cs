using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum IngredientState { Raw, Prepared, Cooking }
public enum ingredient { None,Meat, Fish, Carrot }
public class Ingredient : MonoBehaviour
{
    public IngredientState CurrentState { get;  set; } = IngredientState.Raw;
    public ingredient ingredient;
    [SerializeField] private Mesh rawIngredientMesh;
    [SerializeField] private Mesh preparedIngredientMesh;
    [SerializeField] private Mesh fryPanFoodMesh;
    
    //public Food_Menu.menu menu;
    

    private MeshFilter meshFilter;
    // Start is called before the first frame update
    void Awake()
    {

        meshFilter = GetComponent<MeshFilter>();
        //Debug.Log(CurrentState);
        //Interact();
    }

    public void Interact()
    {
        //Debug.Log(CurrentState);
        if (CurrentState == IngredientState.Raw)
        {
            CurrentState = IngredientState.Prepared;
            meshFilter.mesh = preparedIngredientMesh;
            // 필요 시 애니메이션 재생, 효과음 추가 등
        }
        else if (CurrentState == IngredientState.Prepared)
        {
            CurrentState = IngredientState.Cooking;
            meshFilter.mesh = fryPanFoodMesh;
            // 필요 시 애니메이션 재생, 효과음 추가 등
        }
      
        
       
    }
  


}

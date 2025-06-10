using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum IngredientState { Raw, Prepared, Cooking }
public enum ingredient { None,Meat, Fish, Carrot,Onion }
public class Ingredient : MonoBehaviour
{
    public IngredientState CurrentState { get;  set; } = IngredientState.Raw;
    public ingredient ingredient;
    [SerializeField] private Mesh rawIngredientMesh;
    [SerializeField] private Mesh preparedIngredientMesh;
    [SerializeField] private Mesh fryPanFoodMesh;

    [SerializeField] private Material rawMaterial;        // Raw 상태 매터리얼
    [SerializeField] private Material preparedMaterial;  // Prepared 상태 매터리얼
    [SerializeField] private Material cookingMaterial;   // Cooking 상태 매터리얼

    [SerializeField] public Sprite sprite;

    //public Food_Menu.menu menu;


    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshFilter == null)
        {
            Debug.LogError("Ingredient: MeshFilter component not found!");
        }
        if (meshRenderer == null)
        {
            Debug.LogError("Ingredient: MeshRenderer component not found!");
        }
        UpdateMeshAndMaterial(); // 초기 상태에 맞게 설정
    }

    public void Interact()
    {
        if (CurrentState == IngredientState.Raw)
        {
            CurrentState = IngredientState.Prepared;
        }
        else if (CurrentState == IngredientState.Prepared)
        {
            CurrentState = IngredientState.Cooking;
        }
        UpdateMeshAndMaterial(); // 상태 변경 후 메쉬와 매터리얼 업데이트
    }

    private void UpdateMeshAndMaterial()
    {
        switch (CurrentState)
        {
            case IngredientState.Raw:
                if (rawIngredientMesh != null) meshFilter.mesh = rawIngredientMesh;
                if (rawMaterial != null) meshRenderer.material = rawMaterial;
                break;
            case IngredientState.Prepared:
                if (preparedIngredientMesh != null) meshFilter.mesh = preparedIngredientMesh;
                if (preparedMaterial != null) meshRenderer.material = preparedMaterial;
                break;
            case IngredientState.Cooking:
                if (fryPanFoodMesh != null) meshFilter.mesh = fryPanFoodMesh;
                if (cookingMaterial != null) meshRenderer.material = cookingMaterial;
                break;
            default:
                Debug.LogWarning("Ingredient: Unknown CurrentState!");
                break;
        }
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FoodMenu { None,meatSteak,fishSteak,trashFood, braisedRibs ,fishStew}
public class Food_State : MonoBehaviour
{
    public FoodMenu foodMenu;
    public float price;
    [SerializeField] public Sprite sprite;
}

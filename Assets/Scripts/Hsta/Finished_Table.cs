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
}

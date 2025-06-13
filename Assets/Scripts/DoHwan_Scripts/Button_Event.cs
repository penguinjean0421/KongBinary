using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button_Event : MonoBehaviour
{
    [SerializeField] private Button addPlayerB;
    //[SerializeField] private Button startB;
    [SerializeField] private Image player2;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.playerIndex = 1;
        
    }

  

    public void StartButton()
    {


    }
    public void AddPlayer()
    {
        GameManager.Instance.playerIndex = 2;
    }
}

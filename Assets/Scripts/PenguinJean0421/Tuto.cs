using UnityEngine;

public class Tuto : MonoBehaviour
{
    GameObject tutoClearButton;
    GameObject chooseStageButton;

    void Start()
    {
        tutoClearButton = GameObject.Find("TutoClearButton");
        chooseStageButton = GameObject.Find("ChooseStageButton");
    }
    public void TutoClear()
    {

    }
}
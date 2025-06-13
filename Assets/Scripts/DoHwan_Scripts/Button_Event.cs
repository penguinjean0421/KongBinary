using UnityEngine;
using UnityEngine.UI;

public class Button_Event : MonoBehaviour
{
    [SerializeField] private Button addPlayerB;
    [SerializeField] private Image player2;

    void Start()
    {
        GameManager.Instance.playerIndex = 1;
        if (player2 != null)
        {
            player2.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Button_Event: player2 is not assigned!");
        }
    }

    public void StartButton()
    {
        SceneLoader.Instance.OnClickStage();
    }

    public void AddPlayer()
    {
        GameManager.Instance.playerIndex = 2;
        if (addPlayerB != null)
        {
            //addPlayerB.interactable = false;
            addPlayerB.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Button_Event: addPlayerB is not assigned!");
        }
        if (player2 != null)
        {
            player2.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Button_Event: player2 is not assigned!");
        }
    }
}
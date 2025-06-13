using UnityEngine;

public class SingletoneStart : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.ResetGameStart();
    }
}

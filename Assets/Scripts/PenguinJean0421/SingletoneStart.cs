using UnityEngine;

public class SingletoneStart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.ResetGameStart();
    }
}

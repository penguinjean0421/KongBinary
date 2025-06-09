using UnityEngine;

public class ScoreData : MonoBehaviour
{
    public static ScoreData Instance { get; private set; } // 싱글톤

    void Awake()
    {
        if (Instance != null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }


}

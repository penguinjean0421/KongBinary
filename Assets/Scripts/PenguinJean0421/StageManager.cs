using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance; // 싱글톤

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

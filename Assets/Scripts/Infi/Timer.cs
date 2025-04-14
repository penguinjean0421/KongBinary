using UnityEngine;
using UnityEngine.UI;
public class Timer : MonoBehaviour
{
    Slider timer;
    public float maxTime;
    float time;
    void Awake()
    {

        timer = GetComponent<Slider>();
    }

    void Start()
    {
        time = maxTime;
    }

    void Update()
    {
        time -= Time.deltaTime;
        timer.value = time / maxTime;
        if (timer.value > 0)
        {
            Debug.Log($"Time : {string.Format("{0:N2}", time)}");
        }
        else
        {
            Debug.Log("Time Over");
        }
    }
}

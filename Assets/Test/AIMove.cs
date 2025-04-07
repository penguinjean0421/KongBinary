using UnityEngine;
using UnityEngine.AI;
public class AIMove : MonoBehaviour
{
    public Transform[] target;
    NavMeshAgent nmAgent;

    void Awake()
    {
        nmAgent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        nmAgent.SetDestination(target[Random.Range(0, target.Length)].position);
        Debug.Log($"{target[Random.Range(0, target.Length)].position}로 가는중");
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Table : MonoBehaviour
{
    [SerializeField] private GameObject setPosition;
    private GameObject currentObject; // 테이블에 놓인 현재 오브젝트

    public void Interact(Player_Controller player)
    {
        if (player == null)
        {
            Debug.LogWarning("Table.Interact: Player is null!");
            return;
        }

        // 1. 플레이어 손에 오브젝트가 있고 테이블이 비어 있으면 테이블로 이동
        if (player.isHandObject != null && currentObject == null)
        {
            currentObject = player.isHandObject;
            currentObject.transform.SetParent(setPosition.transform);
            currentObject.transform.position = setPosition.transform.position;
            currentObject.transform.rotation = setPosition.transform.rotation;
            player.isHandObject = null;
            Debug.Log($"Table: Moved {currentObject.name} from hand to table");
        }
        // 2. 플레이어 손이 비어 있고 테이블에 오브젝트가 있으면 손으로 이동
        else if (player.isHandObject == null && currentObject != null)
        {
            player.isHandObject = currentObject;
            player.isHandObject.transform.SetParent(player.handPosition.transform);
            player.isHandObject.transform.position = player.handPosition.transform.position;
            player.isHandObject.transform.rotation = player.handPosition.transform.rotation * Quaternion.Euler(0, 90, 0);
            currentObject = null;
            Debug.Log($"Table: Moved {player.isHandObject.name} from table to hand");
        }
        else
        {
            Debug.LogWarning("Table.Interact: No valid interaction (hand or table full)");
        }
    }
}
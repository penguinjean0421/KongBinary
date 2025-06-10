using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Table : MonoBehaviour
{
    [SerializeField] private GameObject setPosition;
    private GameObject currentObject; // ���̺� ���� ���� ������Ʈ

    public void Interact(Player_Controller player)
    {
        if (player == null)
        {
            Debug.LogWarning("Table.Interact: Player is null!");
            return;
        }

        // 1. �÷��̾� �տ� ������Ʈ�� �ְ� ���̺��� ��� ������ ���̺�� �̵�
        if (player.isHandObject != null && currentObject == null)
        {
            currentObject = player.isHandObject;
            currentObject.transform.SetParent(setPosition.transform);
            currentObject.transform.position = setPosition.transform.position;
            currentObject.transform.rotation = setPosition.transform.rotation;
            player.isHandObject = null;
            Debug.Log($"Table: Moved {currentObject.name} from hand to table");
        }
        // 2. �÷��̾� ���� ��� �ְ� ���̺� ������Ʈ�� ������ ������ �̵�
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
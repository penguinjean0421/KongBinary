using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates; // Ÿ�� ��ǥ (x, z)
    public bool canMove = true; // �̵� ���� ����

  
    private void Awake()
    {
        // Ÿ���� ���� ��ǥ�� �׸��� ��ǥ�� ���� (�ʿ� �� ����)
        coordinates = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        //Debug.Log($"Tile initialized at {coordinates}, canMove: {canMove}, position: {transform.position}");
    }
}
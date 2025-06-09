using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates; // 타일 좌표 (x, z)
    public bool canMove = true; // 이동 가능 여부

  
    private void Awake()
    {
        // 타일의 월드 좌표를 그리드 좌표로 설정 (필요 시 조정)
        coordinates = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        //Debug.Log($"Tile initialized at {coordinates}, canMove: {canMove}, position: {transform.position}");
    }
}
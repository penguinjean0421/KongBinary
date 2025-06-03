using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance;
    private Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();

    [SerializeField] private bool debugMode = false; // Inspector에 표시될 변수 추가

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // 루트 오브젝트로 설정
            DontDestroyOnLoad(gameObject); // 씬 변경 시 유지
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Tile[] tiles = FindObjectsOfType<Tile>();
        if (tiles.Length == 0)
        {
            Debug.LogError("TileManager: No Tile objects found in the scene!");
        }
        else
        {
            foreach (Tile tile in tiles)
            {
                if (tile == null)
                {
                    Debug.LogWarning("TileManager: Null Tile reference encountered, skipping.");
                    continue;
                }
                if (tileMap.ContainsKey(tile.coordinates))
                {
                    Debug.LogWarning($"TileManager: Duplicate tile at {tile.coordinates}, overwriting!");
                }
                tileMap[tile.coordinates] = tile;
                if (debugMode)
                {
                    Debug.Log($"TileManager: Registered tile at {tile.coordinates}, canMove: {tile.canMove}, world pos: {tile.transform.position}");
                }
            }
            Debug.Log($"TileManager: Total tiles registered: {tileMap.Count}");
            if (debugMode && tileMap.Count > 0)
            {
                Debug.Log($"TileManager: Registered tiles: {string.Join(", ", tileMap.Keys)}");
            }
        }
    }

    public Tile GetTile(Vector2Int coordinates)
    {
        if (tileMap.ContainsKey(coordinates))
        {
            return tileMap[coordinates];
        }
        if (debugMode)
        {
            Debug.LogWarning($"TileManager: Tile not found at {coordinates}");
        }
        return null;
    }

    public bool CanMove(Vector2Int coordinates)
    {
        Tile tile = GetTile(coordinates);
        bool canMove = tile != null && tile.canMove;
        if (debugMode)
        {
            Debug.Log($"TileManager: CanMove check at {coordinates}, result: {canMove}");
        }
        return canMove;
    }
}
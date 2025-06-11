using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Waiter : MonoBehaviour
{
    [SerializeField] private GameObject startTileObject;
    [SerializeField] private GameObject[] targetTileObjects;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 10f; // 회전 속도

    private Vector2Int startTile;
    private Vector2Int currentTarget;
    private bool movingToTarget = true;
    private List<Vector2Int> path = new List<Vector2Int>(); // 경로 저장

    void Start()
    {
        StartCoroutine(DelayedExecution());
    }

    IEnumerator DelayedExecution()
    {
        // 한 프레임 대기 (Start가 끝난 후 실행)
        yield return null;
        // 원하는 로직 작성
        Debug.Log("Start 이후 실행됨!");
        if (startTileObject != null)
        {
            startTile = new Vector2Int(Mathf.RoundToInt(startTileObject.transform.position.x), Mathf.RoundToInt(startTileObject.transform.position.z));
            transform.position = new Vector3(startTile.x, transform.position.y, startTile.y);
            Debug.Log($"NPC_Waiter: startTileObject position: {startTileObject.transform.position}, converted to startTile: {startTile}");
        }
        else
        {
            Debug.LogError("NPC_Waiter: Start tile object not assigned!");
            yield return null;
        }

        if (targetTileObjects != null && targetTileObjects.Length > 0)
        {
            int randomIndex = Random.Range(0, targetTileObjects.Length);
            GameObject initialTargetObject = targetTileObjects[randomIndex];
            currentTarget = new Vector2Int(Mathf.RoundToInt(initialTargetObject.transform.position.x), Mathf.RoundToInt(initialTargetObject.transform.position.z));
        }
        else
        {
            Debug.LogError("NPC_Waiter: No target tile objects assigned!");
            yield return null;
        }

        Tile start = TileManager.Instance.GetTile(startTile);
        Tile target = TileManager.Instance.GetTile(currentTarget);
        if (start == null)
        {
            Debug.LogError($"NPC_Waiter: Start tile {startTile} not found! Please add a tile at this position.");
            yield return null;
        }
        if (target == null)
        {
            Debug.LogError($"NPC_Waiter: Target tile {currentTarget} not found! Please add a tile at this position.");
            yield return null;
        }

        Debug.Log($"NPC_Waiter: Starting at {startTile}, moving to {currentTarget}");
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            Debug.Log($"NPC_Waiter: Current position: {currentPos}, Target: {currentTarget}");

            if (currentPos == currentTarget)
            {
                movingToTarget = !movingToTarget;
                if (movingToTarget)
                {
                    int randomIndex;
                    Vector2Int newTarget;
                    do
                    {
                        randomIndex = Random.Range(0, targetTileObjects.Length);
                        newTarget = new Vector2Int(Mathf.RoundToInt(targetTileObjects[randomIndex].transform.position.x), Mathf.RoundToInt(targetTileObjects[randomIndex].transform.position.z));
                    } while (targetTileObjects.Length > 1 && newTarget == currentTarget);
                    currentTarget = newTarget;
                }
                else
                {
                    currentTarget = startTile;
                }
                //Debug.Log($"NPC_Waiter: Reached {currentPos}, now moving to {currentTarget}");
            }

            // 경로 계산
            path = FindPath(currentPos, currentTarget);
            if (path.Count == 0)
            {
                Debug.LogWarning($"NPC_Waiter: No path found to {currentTarget}! Waiting for 1 second.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            // 경로를 따라 이동
            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int nextPos = path[i];
                Vector3 targetWorldPos = new Vector3(nextPos.x, transform.position.y, nextPos.y);
                // 이동 방향으로 회전
                Vector3 moveDirection = (targetWorldPos - transform.position).normalized;
                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                //Debug.Log($"NPC_Waiter: Moving to {nextPos}");
                while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
                    // 이동 중에도 회전 업데이트
                    if (moveDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                    yield return null;
                }
                transform.position = targetWorldPos;
                yield return null;
            }
        }
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int target)
    {
        //Debug.Log($"NPC_Waiter: Finding path from {start} to {target}");

        // A* 노드
        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { { start, 0 } };
        var fScore = new Dictionary<Vector2Int, float> { { start, ManhattanDistance(start, target) } };

        while (openSet.Count > 0)
        {
            // fScore가 가장 낮은 노드 선택
            Vector2Int current = openSet[0];
            float currentFScore = fScore[current];
            for (int i = 1; i < openSet.Count; i++)
            {
                Vector2Int pos = openSet[i];
                if (fScore[pos] < currentFScore)
                {
                    current = pos;
                    currentFScore = fScore[pos];
                }
            }

            if (current == target)
            {
                // 경로 재구성
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int curr = target;
                while (curr != start)
                {
                    path.Add(curr);
                    curr = cameFrom[curr];
                }
                path.Reverse();
                //Debug.Log($"NPC_Waiter: Path found: {string.Join(" -> ", path)}");
                return path;
            }

            openSet.Remove(current);

            // 4방향 탐색
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 상
                new Vector2Int(0, -1),  // 하
                new Vector2Int(-1, 0),  // 좌
                new Vector2Int(1, 0)    // 우
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;
                if (!TileManager.Instance.CanMove(neighbor)) continue;

                float tentativeGScore = gScore[current] + 1;
                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + ManhattanDistance(neighbor, target);
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        Debug.LogWarning($"NPC_Waiter: No path exists from {start} to {target}!");
        return new List<Vector2Int>(); // 경로 없음
    }

    private float ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NPC_Waiter : MonoBehaviour
{
    [SerializeField] private Vector2Int startTile;
    [SerializeField] private Vector2Int targetTile;
    [SerializeField] private float moveSpeed = 2f;

    private Vector2Int currentTarget;
    private bool movingToTarget = true;

    void Start()
    {
        transform.position = new Vector3(startTile.x, transform.position.y, startTile.y);
        currentTarget = targetTile;

        // 타일 확인
        Tile start = TileManager.Instance.GetTile(startTile);
        Tile target = TileManager.Instance.GetTile(targetTile);
        if (start == null)
        {
            Debug.LogError($"NPC_Waiter: Start tile {startTile} not found! Please add a tile at this position.");
            return;
        }
        if (target == null)
        {
            Debug.LogError($"NPC_Waiter: Target tile {targetTile} not found! Please add a tile at this position.");
            return;
        }

        Debug.Log($"NPC_Waiter: Starting at {startTile}, moving to {targetTile}");
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            Vector2Int currentPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
            //Debug.Log($"NPC_Waiter: Current position: {currentPos}, Target: {currentTarget}");

            if (currentPos == currentTarget)
            {
                movingToTarget = !movingToTarget;
                currentTarget = movingToTarget ? targetTile : startTile;
                //Debug.Log($"NPC_Waiter: Reached {currentPos}, now moving to {currentTarget}");
            }

            Vector2Int nextPos = GetNextPosition(currentPos, currentTarget);
            if (nextPos == currentPos)
            {
                Debug.LogWarning($"NPC_Waiter: Cannot move to {currentTarget}, path blocked or invalid!");
                yield return new WaitForSeconds(1f);
                continue;
            }

            Vector3 targetWorldPos = new Vector3(nextPos.x, transform.position.y, nextPos.y);
            //Debug.Log($"NPC_Waiter: Moving to {nextPos}");
            while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetWorldPos;
            yield return null;
        }
    }

    private Vector2Int GetNextPosition(Vector2Int current, Vector2Int target)
    {
        //Debug.Log($"NPC_Waiter: Calculating next position from {current} to {target}");

        if (current.x != target.x)
        {
            int direction = target.x > current.x ? 1 : -1;
            Vector2Int next = new Vector2Int(current.x + direction, current.y);
            if (TileManager.Instance.CanMove(next))
            {
                //Debug.Log($"NPC_Waiter: Next position (x direction): {next}");
                return next;
            }
            Debug.Log($"NPC_Waiter: Path blocked in x direction at {next}");
        }
        else if (current.y != target.y)
        {
            int direction = target.y > current.y ? 1 : -1;
            Vector2Int next = new Vector2Int(current.x, current.y + direction);
            if (TileManager.Instance.CanMove(next))
            {
                //Debug.Log($"NPC_Waiter: Next position (z direction): {next}");
                return next;
            }
            Debug.Log($"NPC_Waiter: Path blocked in z direction at {next}");
        }

        Debug.Log($"NPC_Waiter: No valid next position found!");
        return current;
    }
}
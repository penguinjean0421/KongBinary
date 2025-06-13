using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Spwan : MonoBehaviour
{
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;

    [SerializeField] private int playerIndex;

    [SerializeField] private Transform player1Pos;
    [SerializeField] private Transform player2Pos;

    void Start()
    {
        playerIndex = GameManager.Instance.playerIndex;
        if (playerIndex == 1)
        {
            if (player1 != null && player1Pos != null)
            {
                Instantiate(player1, player1Pos.position, player1Pos.rotation);
            }
            else
            {
                Debug.LogError("Player_Spwan: player1 or player1Pos is not assigned!");
            }
        }
        else if (playerIndex == 2)
        {
            if (player1 != null && player1Pos != null)
            {
                Instantiate(player1, player1Pos.position, player1Pos.rotation);
            }
            else
            {
                Debug.LogError("Player_Spwan: player1 or player1Pos is not assigned!");
            }

            if (player2 != null && player2Pos != null)
            {
                Instantiate(player2, player2Pos.position, player2Pos.rotation);
            }
            else
            {
                Debug.LogError("Player_Spwan: player2 or player2Pos is not assigned!");
            }
        }
        else
        {
            Debug.LogWarning("Player_Spwan: Invalid playerIndex! Use 1 or 2.");
        }
    }
}
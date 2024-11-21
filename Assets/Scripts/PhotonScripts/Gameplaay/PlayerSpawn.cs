using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Assign the player prefab in the Inspector
    [SerializeField] private Vector2[] spawnPoints; // Assign spawn points in the Inspector

    public static Vector2[] SharedSpawnPoints { get; private set; } // Shared spawn points for other scripts

    private void Awake()
    {
        // Share spawn points for access in other scripts
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in PlayerSpawn!");
            return;
        }
        SharedSpawnPoints = spawnPoints;
    }

    private void Start()
    {
        // Ensure the playerPrefab is assigned
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned in PlayerSpawn!");
            return;
        }

        // Get the local player actor number
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1, so subtract 1 for zero-based index

        // Get the corresponding spawn point
        if (playerIndex < 0 || playerIndex >= spawnPoints.Length)
        {
            Debug.LogError("Player index is out of range of the spawn points array!");
            return;
        }
        Vector2 spawnPosition = spawnPoints[playerIndex];

        // Instantiate the player at the chosen spawn position
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        Debug.Log($"Player spawned at {spawnPosition}");
    }
}

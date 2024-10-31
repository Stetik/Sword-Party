using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector2[] spawnPoints;

    private static int currentSpawnIndex = 0;
    private void Start()
    {
        // Get the local player actor number
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // ActorNumber starts at 1, so subtract 1 for zero-based index

        // Use the player index to get the corresponding spawn point
        Vector2 spawnPosition = spawnPoints[playerIndex % spawnPoints.Length];

        // Instantiate the player at the chosen spawn position
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
    }
}

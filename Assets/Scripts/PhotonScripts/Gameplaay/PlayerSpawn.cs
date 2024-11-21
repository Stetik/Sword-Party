using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab; // Player prefab assigned in the Inspector
    [SerializeField] private Vector2[] spawnPoints; // Spawn points assigned in the Inspector

    public static Vector2[] SharedSpawnPoints { get; private set; } // Shared spawn points for other scripts

    private bool hasSpawned; // Ensure player spawns only once

    private void Awake()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in PlayerSpawn!");
            return;
        }

        SharedSpawnPoints = spawnPoints;
    }

    private void Start()
    {
        // Attempt to spawn the player if already in the room
        if (PhotonNetwork.InRoom && !hasSpawned)
        {
            SpawnPlayer();
        }
    }

    public override void OnJoinedRoom()
    {
        // Ensure the player spawns when joining/rejoining a room
        if (!hasSpawned)
        {
            SpawnPlayer();
        }

        SyncSceneState();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab is not assigned in PlayerSpawn!");
            return;
        }

        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1; // Zero-based index for spawn points

        if (playerIndex < 0 || playerIndex >= spawnPoints.Length)
        {
            Debug.LogError("Player index is out of range for spawn points!");
            return;
        }

        Vector2 spawnPosition = spawnPoints[playerIndex];

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);
        hasSpawned = true;
        Debug.Log($"Player spawned at {spawnPosition}");
    }

    private void SyncSceneState()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null. Cannot synchronize scene state.");
            return;
        }

        GameObject lobbyObject = GameObject.Find("Lobby"); // Replace with actual lobby object name
        GameObject mapObject = GameObject.Find("Map"); // Replace with actual map object name

        bool isLobby = GameManager.Instance.CurrentState == GameManager.GameState.Lobby;

        if (lobbyObject != null)
        {
            lobbyObject.SetActive(isLobby);
        }

        if (mapObject != null)
        {
            mapObject.SetActive(!isLobby);
        }

        Debug.Log("Scene state synchronized for rejoining player.");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered the room.");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room.");
    }
}

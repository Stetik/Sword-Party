using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartGameTrigger : MonoBehaviourPun
{
    private Dictionary<Photon.Realtime.Player, float> playersInZone = new Dictionary<Photon.Realtime.Player, float>();
    [SerializeField] private GameObject map; // Assign the specific map in the Inspector
    [SerializeField] private GameObject lobbyObject; // Assign the lobby object in the Inspector
    private float startGameTimer = 0f;
    private const float requiredTimeInZone = 3f; // Time players need to stay in the zone

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView playerView = collision.GetComponent<PhotonView>();
        if (playerView != null)
        {
            Photon.Realtime.Player player = playerView.Owner;

            if (player != null && !playersInZone.ContainsKey(player))
            {
                playersInZone[player] = Time.time;
                Debug.Log("Player entered the zone: " + player.NickName);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PhotonView playerView = collision.GetComponent<PhotonView>();
        if (playerView != null)
        {
            Photon.Realtime.Player player = playerView.Owner;

            if (player != null && playersInZone.ContainsKey(player))
            {
                playersInZone.Remove(player);
                Debug.Log("Player left the zone: " + player.NickName);
                startGameTimer = 0f; // Reset the timer
            }
        }
    }

    private void Update()
    {
        if (map == null || lobbyObject == null)
        {
            Debug.LogError("Map or Lobby object not assigned in the Inspector!");
            return;
        }

        if (playersInZone.Count < PhotonNetwork.CurrentRoom.PlayerCount) // Require all players to be in the zone
        {
            startGameTimer = 0f;
            return;
        }

        // Check if all players have been in the zone for the required time
        bool allPlayersReady = true;

        foreach (var kvp in playersInZone)
        {
            Photon.Realtime.Player player = kvp.Key;

            if (player == null || !PhotonNetwork.CurrentRoom.Players.ContainsValue(player))
            {
                allPlayersReady = false;
                break;
            }

            float entryTime = kvp.Value;

            if (Time.time - entryTime < requiredTimeInZone)
            {
                allPlayersReady = false;
                break;
            }
        }

        if (allPlayersReady)
        {
            startGameTimer += Time.deltaTime;

            if (startGameTimer >= requiredTimeInZone)
            {
                photonView.RPC("ActivateMapAndRespawnPlayers", RpcTarget.AllBuffered); // Trigger map activation and respawn players
                startGameTimer = 0f; // Reset timer to prevent duplicate calls
            }
        }
    }

    [PunRPC]
    private void ActivateMapAndRespawnPlayers()
    {
        if (map == null || lobbyObject == null || PlayerSpawn.SharedSpawnPoints == null || PlayerSpawn.SharedSpawnPoints.Length == 0)
        {
            Debug.LogError("Map, Lobby, or Spawn Points are not properly assigned!");
            return;
        }

        // Deactivate the lobby
        lobbyObject.SetActive(false);
        Debug.Log("Lobby deactivated.");

        // Activate the selected map
        map.SetActive(true);
        Debug.Log("Map activated: " + map.name);

        // Respawn players at their original spawn points
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            int playerIndex = player.ActorNumber - 1; // ActorNumber starts at 1
            Vector2[] spawnPoints = PlayerSpawn.SharedSpawnPoints;

            if (playerIndex < spawnPoints.Length)
            {
                PhotonView playerView = FindPlayerPhotonView(player.ActorNumber);
                if (playerView != null && playerView.IsMine) // Ensure this is the local player
                {
                    playerView.transform.position = spawnPoints[playerIndex]; // Move the player
                    Debug.Log($"Player {player.NickName} respawned at {spawnPoints[playerIndex]}");
                }
            }
        }

        // Start the gameplay state
        GameManager.Instance.StartGameplay();
    }

    private PhotonView FindPlayerPhotonView(int actorNumber)
    {
        foreach (GameObject playerObject in GameObject.FindGameObjectsWithTag("Player")) // Ensure your player prefab has a "Player" tag
        {
            PhotonView pv = playerObject.GetComponent<PhotonView>();
            if (pv != null && pv.Owner.ActorNumber == actorNumber)
            {
                return pv;
            }
        }

        Debug.LogError($"No PhotonView found for player with ActorNumber {actorNumber}");
        return null;
    }
}

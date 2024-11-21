using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StartGameTrigger : MonoBehaviourPun
{
    private readonly Dictionary<Photon.Realtime.Player, float> playersInZone = new();
    [SerializeField] private GameObject map; // Assigned map object
    [SerializeField] private GameObject lobbyObject; // Assigned lobby object
    private float startGameTimer = 0f;
    private const float RequiredTimeInZone = 3f; // Time players need to stay in the zone

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playerView = collision.GetComponent<PhotonView>();
        if (playerView == null) return;

        var player = playerView.Owner;
        if (player != null && !playersInZone.ContainsKey(player))
        {
            playersInZone[player] = Time.time;
            Debug.Log($"Player entered the zone");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var playerView = collision.GetComponent<PhotonView>();
        if (playerView == null) return;

        var player = playerView.Owner;
        if (player != null && playersInZone.Remove(player))
        {
            Debug.Log($"Player left the zone: {player.NickName}");
            startGameTimer = 0f; // Reset the timer
        }
    }

    private void Update()
    {
        if (!ValidateAssignedObjects()) return;

        if (playersInZone.Count < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            startGameTimer = 0f; // Reset timer if not all players are in the zone
            return;
        }

        if (AreAllPlayersReady())
        {
            startGameTimer += Time.deltaTime;

            if (startGameTimer >= RequiredTimeInZone)
            {
                photonView.RPC(nameof(ActivateMapAndRespawnPlayers), RpcTarget.AllBuffered);
                startGameTimer = 0f; // Prevent duplicate calls
            }
        }
    }

    private bool ValidateAssignedObjects()
    {
        if (map == null || lobbyObject == null)
        {
            Debug.LogError("Map or Lobby object not assigned in the Inspector!");
            return false;
        }
        return true;
    }

    private bool AreAllPlayersReady()
    {
        foreach (var kvp in playersInZone)
        {
            var player = kvp.Key;
            if (player == null || !PhotonNetwork.CurrentRoom.Players.ContainsValue(player))
                return false;

            if (Time.time - kvp.Value < RequiredTimeInZone)
                return false;
        }

        return true;
    }

    [PunRPC]
    private void ActivateMapAndRespawnPlayers()
    {
        if (!ValidateActivationRequirements()) return;

        lobbyObject.SetActive(false);
        map.SetActive(true);

        Debug.Log("Lobby deactivated and map activated.");

        RespawnPlayers();

        GameManager.Instance.StartGameplay();
    }

    private bool ValidateActivationRequirements()
    {
        if (map == null || lobbyObject == null || PlayerSpawn.SharedSpawnPoints == null || PlayerSpawn.SharedSpawnPoints.Length == 0)
        {
            Debug.LogError("Map, Lobby, or Spawn Points are not properly assigned!");
            return false;
        }
        return true;
    }

    private void RespawnPlayers()
    {
        var spawnPoints = PlayerSpawn.SharedSpawnPoints;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int playerIndex = player.ActorNumber - 1; // ActorNumber starts at 1

            if (playerIndex < spawnPoints.Length)
            {
                var playerView = FindPlayerPhotonView(player.ActorNumber);
                if (playerView != null && playerView.IsMine)
                {
                    playerView.transform.position = spawnPoints[playerIndex];
                    Debug.Log($"Player {player.NickName} respawned at {spawnPoints[playerIndex]}");
                }
            }
        }
    }

    private PhotonView FindPlayerPhotonView(int actorNumber)
    {
        foreach (var playerObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            var photonView = playerObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.Owner.ActorNumber == actorNumber)
            {
                return photonView;
            }
        }

        Debug.LogError($"No PhotonView found for player with ActorNumber {actorNumber}");
        return null;
    }
}

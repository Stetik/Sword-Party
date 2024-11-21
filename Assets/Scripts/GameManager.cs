using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance;

    public enum GameState { Lobby, Gameplay }
    private GameState currentState = GameState.Lobby;

    public delegate void OnGameStateChanged(GameState newState);
    public static event OnGameStateChanged GameStateChanged;

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState != value)
            {
                currentState = value;
                Debug.Log($"Game state changed to: {currentState}");
                GameStateChanged?.Invoke(currentState);
            }
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGameplay()
    {
        if (photonView == null)
        {
            Debug.LogError("PhotonView is null in GameManager! Ensure this script has a PhotonView component.");
            return;
        }

        Debug.Log("Transitioning to Gameplay...");
        photonView.RPC("SyncGameState", RpcTarget.AllBuffered, GameState.Gameplay);
    }

    public void StartLobby()
    {
        if (photonView == null)
        {
            Debug.LogError("PhotonView is null in GameManager! Ensure this script has a PhotonView component.");
            return;
        }

        Debug.Log("Transitioning to Lobby...");
        photonView.RPC("SyncGameState", RpcTarget.AllBuffered, GameState.Lobby);
    }

    [PunRPC]
    private void SyncGameState(GameState newState)
    {
        Debug.Log($"Synchronizing Game State to: {newState}");
        CurrentState = newState;
    }
}

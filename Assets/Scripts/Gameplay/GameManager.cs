using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Lobby, Gameplay }
    private GameState currentState = GameState.Lobby;

    public GameState CurrentState
    {
        get => currentState;
        private set
        {
            if (currentState == value) return;

            currentState = value;
            Debug.Log($"Game state changed to: {currentState}");
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
        ChangeGameState(GameState.Gameplay);
    }

    public void StartLobby()
    {
        ChangeGameState(GameState.Lobby);
    }

    private void ChangeGameState(GameState newState)
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(SyncGameState), RpcTarget.AllBuffered, newState);
        }
        else
        {
            SyncGameState(newState); // Fallback for offline mode
        }
    }

    [PunRPC]
    private void SyncGameState(GameState newState)
    {
        Debug.Log($"Synchronizing Game State to: {newState}");
        CurrentState = newState;
    }
}

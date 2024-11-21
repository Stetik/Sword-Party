using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class MainMenuTrigger : MonoBehaviourPunCallbacks
{
    private bool isInsideTrigger = false; // Tracks if the player is inside the trigger
    private float timeInsideTrigger = 0f; // Timer for how long the player has stayed inside
    [SerializeField] private float requiredTimeInTrigger = 3f; // Time required to trigger transition

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var playerView = collision.GetComponent<PhotonView>();
        if (playerView != null && playerView.IsMine)
        {
            isInsideTrigger = true; // Mark player as inside the trigger
            Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} entered the trigger zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var playerView = collision.GetComponent<PhotonView>();
        if (playerView != null && playerView.IsMine)
        {
            isInsideTrigger = false; // Mark player as outside the trigger
            timeInsideTrigger = 0f; // Reset the timer
            Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} left the trigger zone. Timer reset.");
        }
    }

    private void Update()
    {
        if (isInsideTrigger)
        {
            timeInsideTrigger += Time.deltaTime; // Increment timer while inside the trigger

            if (timeInsideTrigger >= requiredTimeInTrigger)
            {
                Debug.Log($"Player {PhotonNetwork.LocalPlayer.NickName} has stayed in the trigger zone for {requiredTimeInTrigger} seconds. Transitioning...");
                StartCoroutine(LeaveRoomAndLoadMainMenu());
            }
        }
    }

    private System.Collections.IEnumerator LeaveRoomAndLoadMainMenu()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("Leaving current room...");
            PhotonNetwork.LeaveRoom();
        }

        // Wait until the player has left the room
        while (PhotonNetwork.InRoom)
        {
            yield return null;
        }

        Debug.Log("Loading MainMenu scene...");
        SceneManager.LoadScene("MainMenu");
    }

    public override void OnLeftRoom()
    {
        // Ensure reconnection to MasterServer
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Reconnecting to MasterServer...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player has successfully left the room and is connected.");
        }
    }
}

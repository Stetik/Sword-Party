using UnityEngine;
using Photon.Pun;

public class KillZone : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player entered the collider
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine) // Ensure only the local player triggers it
        {
            // Call the Die method via RPC
            player.photonView.RPC(nameof(PlayerController.Die), RpcTarget.AllBuffered);
            Debug.Log($"Player {player.photonView.ViewID} touched the KillZone and died.");
        }
    }
}

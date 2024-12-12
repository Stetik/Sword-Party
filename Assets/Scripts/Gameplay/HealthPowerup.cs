using UnityEngine;
using Photon.Pun;

public class HealthPowerup : MonoBehaviourPun
{
    [SerializeField] private int healthBonus = 2; // The amount of health to restore

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collides with the power-up
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && PhotonNetwork.IsConnected)
        {
            // Synchronize the health bonus and destroy the power-up across all players
            photonView.RPC("ApplyPowerup", RpcTarget.All, player.photonView.ViewID);
        }
    }

    [PunRPC]
    private void ApplyPowerup(int playerViewID)
    {
        // Find the player using the ViewID
        PhotonView playerView = PhotonView.Find(playerViewID);
        if (playerView != null)
        {
            PlayerController player = playerView.GetComponent<PlayerController>();
            if (player != null)
            {
                // Add health to the player
                player.AddHealth(healthBonus);
            }
        }

        // Destroy the power-up on all clients
        Destroy(gameObject);
        Debug.Log("Power-up applied and destroyed.");
    }
}

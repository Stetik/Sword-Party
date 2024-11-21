using UnityEngine;
using Photon.Pun;

public class HealthPowerup : MonoBehaviourPun
{
    [SerializeField] private int healthBonus = 2; // The amount of health to restore

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player collides with the power-up
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine) // Ensure this is the local player
        {
            // Add health to the player
            player.photonView.RPC("AddHealth", RpcTarget.AllBuffered, healthBonus);

            
            PhotonNetwork.Destroy(gameObject);

            Debug.Log($"Player healed for {healthBonus} points.");
        }
    }
}

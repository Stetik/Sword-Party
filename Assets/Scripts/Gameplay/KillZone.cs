using UnityEngine;
using Photon.Pun;

public class KillZone : MonoBehaviourPun
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.photonView.IsMine) 
        {
            
            player.photonView.RPC(nameof(PlayerController.Die), RpcTarget.AllBuffered);
            Debug.Log($"Player touched the KillZone");
        }
    }
}

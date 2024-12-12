using UnityEngine;
using Photon.Pun;

public class PhotonConfig : MonoBehaviour
{
    private void Awake()
    {
        PhotonNetwork.SendRate = 60; // Más datos enviados por segundo
        PhotonNetwork.SerializationRate = 30; // Más actualizaciones sincronizadas

    }
}

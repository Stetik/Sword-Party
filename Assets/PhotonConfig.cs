using UnityEngine;
using Photon.Pun;

public class PhotonConfig : MonoBehaviour
{
    private void Awake()
    {
        PhotonNetwork.SendRate = 60; // M�s datos enviados por segundo
        PhotonNetwork.SerializationRate = 30; // M�s actualizaciones sincronizadas

    }
}

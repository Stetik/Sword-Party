using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(Random.Range(-6, 6), Random.Range(-6, 6)), Quaternion.identity);
    }
}

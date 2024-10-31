using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    public int speed;

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (pv.IsMine) 
        {
            if (Input.GetKey(KeyCode.W)) 
            {
                transform.position += Vector3.up * 5 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.S)) 
            {
                transform.position += -Vector3.up * 5 * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position += -Vector3.right * speed * Time.deltaTime;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right * speed * Time.deltaTime;
            }

        }
    }
}

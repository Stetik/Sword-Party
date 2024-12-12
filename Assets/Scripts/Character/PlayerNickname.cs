using UnityEngine;
using Photon.Pun;
using TMPro; // Importa el namespace para TextMeshPro.

public class PlayerNameTag : MonoBehaviourPun
{
    [SerializeField] private TMP_Text playerNameText; // Arrastra el TMP_Text desde el Canvas.
    [SerializeField] private Transform nameTagTransform; // Arrastra el Transform del Canvas para posicionarlo correctamente.

    private void Start()
    {
        if (photonView.IsMine)
        {
            // Muestra tu propio nombre.
            playerNameText.text = PhotonNetwork.NickName;
        }
        else
        {
            // Muestra el nombre del propietario del objeto.
            playerNameText.text = photonView.Owner.NickName;
        }
    }

    private void LateUpdate()
    {
        // Mantén el texto orientado hacia la cámara.
        if (nameTagTransform != null)
        {
            nameTagTransform.rotation = Camera.main.transform.rotation;
        }
    }
}

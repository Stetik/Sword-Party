using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;



public class CreateRoom : MonoBehaviourPunCallbacks
{

    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMPro.TMP_InputField createInput;
    [SerializeField] private TMPro.TMP_InputField joinInput;
    private void Awake()
    {
        createButton.onClick.AddListener(CreateNewRoom);
        joinButton.onClick.AddListener(JoinNewRoom);
    }

    private void OnDestroy()
    {
        createButton.onClick.RemoveAllListeners();
        joinButton.onClick.RemoveAllListeners();
    }
    private void CreateNewRoom()
    {
        RoomOptions roomconfiguration = new RoomOptions();
        roomconfiguration.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(createInput.text, roomconfiguration);
    }
    private void JoinNewRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }

}

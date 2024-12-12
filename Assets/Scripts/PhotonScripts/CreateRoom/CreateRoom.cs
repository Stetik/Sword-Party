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
    [SerializeField] private TMPro.TMP_InputField nicknameImput;
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
        roomconfiguration.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(createInput.text, roomconfiguration);
        PhotonNetwork.NickName = nicknameImput.text;
    }
    private void JoinNewRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
        PhotonNetwork.NickName = nicknameImput.text;
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Gameplay");
    }

}

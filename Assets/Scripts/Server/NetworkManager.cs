using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance = null;

    private void Awake() {
        if(instance == null){
            instance = this;
        }
        else if(instance != null){
            Destroy(this.gameObject);
            Destroy(this);
        }
        DontDestroyOnLoad(this);

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private readonly string gameVersion = "1";

    [SerializeField] private Button joinButton;
    [SerializeField] private Text connectionInfoText;
    
    private bool isConnectedToGame = false;

    private void Start() {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        joinButton.interactable = false;
        connectionInfoText.text = "Connecting to Master Server...";
    }

    public override void OnConnectedToMaster()
    {
        joinButton.interactable = true;
        connectionInfoText.text = "Online : Connected to Master Server";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        joinButton.interactable = false;
        connectionInfoText.text = $"Offline : Connection Disabled {cause.ToString()}";

        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect(){
        joinButton.interactable = false;

        if(PhotonNetwork.IsConnected){
            connectionInfoText.text = "Connecting to Random Room...";

            PhotonNetwork.JoinRandomRoom();
        }
        else{
            connectionInfoText.text = "Offline : Connection Disabled - Try Reconnecting...";

            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnJoinedRoom()
    {
        connectionInfoText.text = "Connected with Room.";
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "There is no empty room, Creating new Room";
        PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 2});
    }

    private void Update() {
        if(PhotonNetwork.IsConnected && PhotonNetwork.InRoom && !isConnectedToGame){
            if(PhotonNetwork.CurrentRoom.PlayerCount == 2){
                isConnectedToGame = true;
                PhotonNetwork.LoadLevel("InGame_Multi");
            }
        }
    }
}

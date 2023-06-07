using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    public string playerName = "Player";
    public int maxPlayers = 4;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();    
    }

    public override void OnConnectedToMaster()
    {

        Debug.Log("Connected to master");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Joining random room failed, creating new room");
        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers
        });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room");
        playerName = "Player " + PhotonNetwork.CurrentRoom.PlayerCount;
        PhotonNetwork.NickName = playerName;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player joined the room: " + newPlayer.NickName);
    }
}

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public string gameSceneName = "SampleScene"; // Replace with your actual game scene name

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Connect to Photon
        Debug.Log("Connecting to Photon...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server. Joining room...");
        PhotonNetwork.JoinRandomRoom(); // Try to join a random room
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No room found. Creating a new one...");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 }); // Create a new room
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room. Loading game scene...");
        PhotonNetwork.LoadLevel(gameSceneName); // Load your actual game scene
    }
}

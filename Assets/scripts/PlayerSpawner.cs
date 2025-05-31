using Photon.Pun;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefabMover;
    public GameObject playerPrefabGuider;
    public Transform spawnPointMover; // Optional spawn point for Mover
    public Transform spawnPointGuider; // Optional spawn point for Guider

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Vector3 spawnPos = spawnPointMover != null ? spawnPointMover.position : Vector3.zero;
                PhotonNetwork.Instantiate(playerPrefabMover.name, spawnPos, Quaternion.identity);
            }
            else
            {
                Vector3 spawnPos = spawnPointGuider != null ? spawnPointGuider.position : Vector3.zero;
                PhotonNetwork.Instantiate(playerPrefabGuider.name, spawnPos, Quaternion.identity);
            }
        }
    }
}
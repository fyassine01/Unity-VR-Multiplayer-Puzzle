using UnityEngine;
using Photon.Pun;
using Photon.Realtime; // For Player type

public class CategoryZone : MonoBehaviour // MonoBehaviourPun is not strictly needed unless it has its own PhotonView
{
    public string zoneCategory;
    public int score = 0; // This score is local to the MasterClient's instance of this zone

    private void OnTriggerEnter(Collider other)
    {
        CubeMetadata cube = other.GetComponent<CubeMetadata>();
        if (cube == null) return;

        // ALL Game Logic for placement happens on the Master Client
        if (PhotonNetwork.IsMasterClient)
        {
            if (cube.category == zoneCategory)
            {
                Debug.Log($"CategoryZone (MasterClient): Correct cube '{other.name}' for category '{zoneCategory}' entered.");

                PhotonView cubePv = cube.GetComponent<PhotonView>();
                Player cubeOwner = cubePv != null ? cubePv.Owner : null;

                // If the cube is owned by a player (the Mover) and that player is not the MasterClient,
                // tell that Mover to drop it via RPC.
                if (cubeOwner != null && !cubeOwner.IsMasterClient)
                {
                    FPSController moverController = FindFPSControllerForPlayer(cubeOwner);
                    if (moverController != null)
                    {
                        Debug.Log($"CategoryZone (MasterClient): Sending RPC_ServerForceDrop to Mover {cubeOwner.NickName} for cube {cube.name}.");
                        moverController.photonView.RPC("RPC_ServerForceDrop", cubeOwner);
                    }
                    else
                    {
                        Debug.LogWarning($"CategoryZone (MasterClient): Could not find FPSController for Mover {cubeOwner.NickName} to send RPC_ServerForceDrop.");
                    }
                }
                // If the cube is owned by the MasterClient (meaning the Mover is the MasterClient)
                else if (cubeOwner != null && cubeOwner.IsMasterClient)
                {
                    FPSController masterMoverController = FindFPSControllerForPlayer(PhotonNetwork.MasterClient); // Find MC's own FPSController
                    if (masterMoverController != null && masterMoverController.IsHoldingObject(cube.gameObject))
                    {
                        Debug.Log($"CategoryZone (MasterClient): MasterClient Mover placed {cube.name}. Forcing local drop.");
                        // Forcing MasterClient's Mover to drop its object locally before destroy.
                        // The RPC_ServerForceDrop calls LocalDrop, so this should be equivalent for MC.
                        masterMoverController.RPC_ServerForceDrop(); // Call the RPC on itself, which calls LocalDrop
                    }
                }

                // Notify GameManager about the correct placement
                if (GameManager.Instance != null && GameManager.Instance.GetComponent<PhotonView>() != null)
                {
                    GameManager.Instance.GetComponent<PhotonView>().RPC("RPC_Master_CubePlacedCorrectly", RpcTarget.MasterClient);
                }
                else
                {
                    Debug.LogError("CategoryZone (MasterClient): GameManager instance or its PhotonView not found for RPC.");
                }

                // Spawn the next cube (MasterClient responsibility)
                CubeSpawner spawner = FindObjectOfType<CubeSpawner>(); // Assumes only one CubeSpawner
                if (spawner != null)
                {
                    spawner.SpawnNewCube();
                }
                else
                {
                    Debug.LogError("CategoryZone (MasterClient): CubeSpawner not found!");
                }

                // Destroy the placed cube (MasterClient responsibility)
                // Request ownership if MasterClient doesn't own it, then destroy.
                if (cubePv != null && !cubePv.IsMine) // IsMine checks if this PhotonView is controlled by the local client (MasterClient here)
                {
                    Debug.Log($"CategoryZone (MasterClient): Cube {cube.name} not owned by MasterClient ({PhotonNetwork.LocalPlayer.NickName}). Requesting ownership before destroy.");
                    cubePv.RequestOwnership();
                    // Destruction will proceed; Mover's OnOwnershipTransfered should handle its state.
                }
                // It's generally safer to wait a frame for ownership transfer or use a callback,
                // but for simplicity, we proceed. PhotonNetwork.Destroy can be called by MasterClient on any object.
                Debug.Log($"CategoryZone (MasterClient): Destroying cube {cube.name}.");
                PhotonNetwork.Destroy(cube.gameObject);

                score++; // Increment MasterClient's local score for this zone
            }
        }
    }

    // Helper to find FPSController for a specific Photon Player
    private FPSController FindFPSControllerForPlayer(Player targetPlayer)
    {
        if (targetPlayer == null) return null;

        FPSController[] allControllers = FindObjectsOfType<FPSController>();
        foreach (FPSController controller in allControllers)
        {
            if (controller.photonView != null && controller.photonView.Owner == targetPlayer)
            {
                return controller;
            }
        }
        return null;
    }
}
using UnityEngine;
using Photon.Pun;

public class PlayerCameraEnabler : MonoBehaviourPun
{
    public Camera playerCamera;

    void Start()
    {
        if (photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(true);
        }
        else
        {
            playerCamera.gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using TMPro;
using Photon.Pun; // Add this namespace

public class CubeMetadata : MonoBehaviour, IPunObservable // Implement the interface
{
    public string category;
    public Color color;
    public TextMeshPro textLabel;

    void Start()
    {
        GetComponent<Renderer>().material.color = color;
        if (textLabel != null)
        {
            textLabel.text = category;
        }
    }

    void Update()
    {
        if (textLabel != null)
        {
            textLabel.transform.position = transform.position + Vector3.up * 1.5f;
            if (Camera.main != null)
            {
                textLabel.transform.LookAt(Camera.main.transform);
                textLabel.transform.Rotate(0, 180, 0);
            }
        }
    }

    // This method will be called by Photon to serialize and deserialize data
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(category);
        }
        else
        {
            // Network player, receive data
            this.category = (string)stream.ReceiveNext();
            // Optionally update the text label when receiving the category
            if (textLabel != null)
            {
                textLabel.text = this.category;
            }
        }
    }
}
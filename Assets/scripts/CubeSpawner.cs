using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Linq;

public class CubeSpawner : MonoBehaviourPunCallbacks // Changed to MonoBehaviourPunCallbacks if you need Photon Callbacks
{
    public GameObject[] cubePrefabs;
    public Transform spawnPoint;
    public string[] categories = { "Sales", "Marketing", "HR" };

    private List<GameObject> availableCubes = new List<GameObject>();
    // private GameObject currentCube; // This was used for distance check, less critical with GameManager

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            availableCubes.AddRange(cubePrefabs);
            int totalCubesForGame = cubePrefabs.Length; // Total unique prefabs to be spawned

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Master_RegisterTotalCubes(totalCubesForGame);
            }
            else
            {
                Debug.LogError("CubeSpawner (MasterClient): GameManager instance not found to register total cubes.");
            }

            SpawnNewCube(); // Spawn the first cube
        }
    }

    // Update logic for distance check was removed as GameManager now tracks placed cubes.
    // If you still want to respawn a cube if it's moved too far from spawn *before* placement,
    // you would need to re-implement that part carefully, considering currentCube is MasterClient only.
    // For now, assuming cubes are only respawned after correct placement.

    public void SpawnNewCube()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (availableCubes.Count > 0)
            {
                int randomIndex = Random.Range(0, availableCubes.Count);
                GameObject prefabToSpawn = availableCubes[randomIndex];
                GameObject newCube = PhotonNetwork.Instantiate(prefabToSpawn.name, spawnPoint.position, Quaternion.identity);
                availableCubes.RemoveAt(randomIndex); // Remove the spawned type from available list

                CubeMetadata metadata = newCube.GetComponent<CubeMetadata>();
                if (metadata != null)
                {
                    metadata.category = categories[Random.Range(0, categories.Length)];
                    // The CubeMetadata's OnPhotonSerializeView or an RPC should handle syncing this category
                    // and updating the text label for relevant players.
                    // Forcing an update here on MasterClient for the text label might be redundant if CubeMetadata handles it.
                    // if (metadata.textLabel != null)
                    // {
                    //     metadata.textLabel.text = metadata.category;
                    // }
                }
                Debug.Log($"CubeSpawner (MasterClient): Spawned new cube '{newCube.name}' with category '{metadata?.category}'. Cubes remaining in spawner list: {availableCubes.Count}");
            }
            else
            {
                Debug.Log("CubeSpawner (MasterClient): All unique cube types have been spawned and (presumably) placed.");
                // GameManager now handles the win condition when cubesCorrectlyPlaced == totalCubesToPlace
                // No direct EndGame call from here.
            }
        }
    }
}
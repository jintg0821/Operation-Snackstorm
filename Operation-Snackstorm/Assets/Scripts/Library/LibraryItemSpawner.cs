using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(BoxCollider))]
public class LibraryItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs;

    public int minSpawnCount = 10;
    public int maxSpawnCount = 20;

    private BoxCollider spawnArea;

    void Awake()
    {
        spawnArea = GetComponent<BoxCollider>();
    }

    public void SpawnItems()
    {

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefabToSpawn = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            Bounds bounds = spawnArea.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = bounds.center.y;
            float randomZ = Random.Range(bounds.min.z, bounds.max.z);
            Vector3 spawnPosition = new Vector3(randomX, randomY, randomZ);

            PhotonNetwork.Instantiate("Prefabs/Items/" + prefabToSpawn.name, spawnPosition, Quaternion.identity);
        }
    }
}
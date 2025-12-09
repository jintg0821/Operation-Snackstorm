using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviourPun
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public int coinCount = 2;

    [Header("Random Offset Range (around each platform)")]
    public float randomMinOffsetX = -3f;
    public float randomMaxOffsetX = 3f;
    public float randomMinOffsetZ = -3f;
    public float randomMaxOffsetZ = 3f;
    public float raycastHeight = 10f;
    public float offsetY = 0.2f;

    [Header("Platform Areas")]
    public Transform[] platforms;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private bool hasSpawnedAfterStart = false;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!GameManager.Instance.gameStart) return;

        if (!hasSpawnedAfterStart)
        {
            SpawnCoins();
            hasSpawnedAfterStart = true;
        }
    }

    void SpawnCoins()
    {
        foreach (Transform platform in platforms)
        {
            for (int i = 0; i < coinCount; i++)
            {
                Vector3 spawnPos = GetValidSpawnPosition(platform);
                if (spawnPos != Vector3.zero)
                {
                    PhotonNetwork.Instantiate("Prefabs/Coin", spawnPos, Quaternion.identity);
                }
            }
        }
    }

    Vector3 GetValidSpawnPosition(Transform platform)
    {
        const int maxAttempts = 20;

        Collider platformCollider = platform.GetComponent<Collider>();
        if (platformCollider == null)
        {
            return Vector3.zero;
        }

        Bounds bounds = platformCollider.bounds;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y + raycastHeight,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 groundPos = hit.point;

                if (!Physics.CheckSphere(groundPos, 0.3f, obstacleLayer))
                {
                    return groundPos + Vector3.up * offsetY;
                }
            }
        }

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (platforms == null) return;

        Gizmos.color = Color.yellow;
        foreach (Transform platform in platforms)
        {
            Gizmos.DrawWireCube(platform.position, new Vector3(
                Mathf.Abs(randomMaxOffsetX - randomMinOffsetX),
                0.1f,
                Mathf.Abs(randomMaxOffsetZ - randomMinOffsetZ)
            ));
        }
    }
}

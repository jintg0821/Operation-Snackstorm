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

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_SpawnCoin", RpcTarget.All);
            Debug.Log("afds");
        }
    }

    [PunRPC]
    void RPC_SpawnCoin()
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
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(randomMinOffsetX, randomMaxOffsetX),
                0,
                Random.Range(randomMinOffsetZ, randomMaxOffsetZ)
            );

            Vector3 startPos = platform.position + randomOffset + Vector3.up * raycastHeight;

            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 potentialPos = hit.point;

                float checkRadius = 0.5f;
                if (!Physics.CheckSphere(potentialPos, checkRadius, obstacleLayer))
                {
                    return potentialPos + Vector3.up * offsetY;
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

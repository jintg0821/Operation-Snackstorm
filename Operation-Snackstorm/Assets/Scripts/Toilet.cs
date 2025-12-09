using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toilet : MonoBehaviourPun
{
    public int spawnCountPerArea = 3;

    public float spawnInterval = 10f;
    private float timer = 0f;

    [Header("Random Offset Range")]
    public float minOffsetX = -4f;
    public float maxOffsetX = 4f;
    public float minOffsetZ = -4f;
    public float maxOffsetZ = 4f;

    [Header("Raycast Settings")]
    public float raycastHeight = 10f;
    public float spawnYOffset = 0.05f;

    [Header("Spawn Areas (복도 구역들)")]
    public Transform[] platforms;

    [Header("Layer Settings")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            SpawnDirtyObjects();
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnDirtyObjects();
        }
    }


    void SpawnDirtyObjects()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_SpawnDirtyObjects", RpcTarget.All);
    }

    [PunRPC]
    void RPC_SpawnDirtyObjects()
    {
        foreach (Transform area in platforms)
        {
            int randomCount = Random.Range(0, spawnCountPerArea + 1);

            for (int i = 0; i < randomCount; i++)
            {
                Vector3 spawnPos = GetValidSpawnPosition(area);
                if (spawnPos != Vector3.zero)
                {
                    PhotonNetwork.Instantiate("Prefabs/Dirty", spawnPos, Quaternion.identity);
                }
            }
        }
    }
    Vector3 GetValidSpawnPosition(Transform area)
    {
        const int maxAttempts = 20;

        Collider platformCollider = area.GetComponent<Collider>();
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
                    return groundPos + Vector3.up * spawnYOffset;
                }
            }
        }

        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        if (platforms == null) return;

        Gizmos.color = Color.green;
        foreach (Transform area in platforms)
        {
            Gizmos.DrawWireCube(area.position, new Vector3(
                Mathf.Abs(maxOffsetX - minOffsetX),
                0.1f,
                Mathf.Abs(maxOffsetZ - minOffsetZ)
            ));
        }
    }
}

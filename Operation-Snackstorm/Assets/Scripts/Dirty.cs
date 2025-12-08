using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirty : MonoBehaviourPun
{
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    public float spawnRadius = 1f;
    public float raycastHeight = 2f;
    public float spawnYOffset = 0.05f;

    private bool canSpawn = true;
    private float spawnCooldown = 3f;

    [SerializeField] private bool isSpawnedDirty = false;

    [PunRPC]
    public void RPC_RequestDirtyDestroy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    void RPC_InstantiateDirty()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!canSpawn) return;
        if (isSpawnedDirty) return;

        StartCoroutine(SpawnCooldownRoutine());

        Vector3 basePos = transform.position;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Vector3 startRayPos = basePos + randomOffset + Vector3.up * raycastHeight;

            if (Physics.Raycast(startRayPos, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundLayer))
            {
                Vector3 groundPos = hit.point;

                if (!Physics.CheckSphere(groundPos, 0.3f, obstacleLayer))
                {
                    GameObject newDirty = PhotonNetwork.Instantiate("Prefabs/Dirty", groundPos + Vector3.up * spawnYOffset, Quaternion.identity);
                    newDirty.GetComponent<Dirty>().isSpawnedDirty = true;
                    return;
                }
            }
        }
    }

    IEnumerator SpawnCooldownRoutine()
    {
        canSpawn = false;
        yield return new WaitForSeconds(spawnCooldown);
        canSpawn = true;
    }
}

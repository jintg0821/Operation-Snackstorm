using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPrefabs;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            int playerPrefabNum = Random.Range(0, playerPrefabs.Length);
            PhotonNetwork.Instantiate(
                playerPrefabs[playerPrefabNum].name,
                GameManager.Instance.spawnPoint.position,
                Quaternion.identity
            );
        }
    }
}

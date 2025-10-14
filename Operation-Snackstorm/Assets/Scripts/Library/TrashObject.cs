using UnityEngine;
using Photon.Pun;

public class TrashObject : MonoBehaviourPun
{
    public void Interact()
    {
        TrashCleanupMission.Instance.OnTrashCollected();
        photonView.RPC("DestroyTrash", RpcTarget.All);
    }

    [PunRPC]
    private void DestroyTrash()
    {
        Destroy(gameObject);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ReportTarget : MonoBehaviourPun
{
    [Header("건의 대상 정보")]
    public string displayName;  
    public int id; 

    [Header("AI 컴포넌트들")]
    public AIController aiController;          
    public TeachersController teachersController; 
    public AIAnimationController animController;  
    public NavMeshAgent agent;

    [Header("정지 설정")]
    [SerializeField] private float stunDuration = 10f; 

    private bool isStunned = false;

    void Awake()
    {
        if (aiController == null) aiController = GetComponent<AIController>();
        if (teachersController == null) teachersController = GetComponent<TeachersController>();
        if (animController == null) animController = GetComponent<AIAnimationController>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    public void Report()
    {
        if (isStunned) return;
        isStunned = true;

        photonView.RPC(nameof(RPC_Stun), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_Stun(PhotonMessageInfo info)
    {
        Debug.Log($"{displayName} 건의됨 → {stunDuration}초 정지!");

        if (aiController != null) aiController.enabled = false;
        if (teachersController != null) teachersController.enabled = false;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        if (animController != null) animController.SetSpeed(0f);

        if (PhotonNetwork.LocalPlayer.ActorNumber == info.Sender.ActorNumber)
        {
            SuggestionBox box = FindObjectOfType<SuggestionBox>();
            if (box != null)
            {
                box.ShowResultMessage($"{displayName}\n{stunDuration}초간 행동 정지!");
            }
        }

        StartCoroutine(ReleaseStun());
    }

    IEnumerator ReleaseStun()
    {
        yield return new WaitForSeconds(stunDuration);

        // 다시 움직이게
        if (aiController != null) aiController.enabled = true;
        if (teachersController != null) teachersController.enabled = true;
        if (agent != null) agent.isStopped = false;
        if (animController != null) animController.SetSpeed(1f);

        isStunned = false; 
    }
}

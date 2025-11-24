using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class ReportTarget : MonoBehaviourPun
{
    [Header("건의 대상 정보")]
    public string displayName;   // UI에 보여줄 이름 
    public int id;               // 0 ~ 17 같은 고유 번호

    [Header("AI 컴포넌트들")]
    public AIController aiController;          
    public TeachersController teachersController; 
    public AIAnimationController animController;  
    public NavMeshAgent agent;                 

    bool reported = false; 

    void Awake()
    {
        if (aiController == null) aiController = GetComponent<AIController>();
        if (teachersController == null) teachersController = GetComponent<TeachersController>();
        if (animController == null) animController = GetComponent<AIAnimationController>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// 건의함에 이 NPC를 넣었을 때 호출되는 함수
    /// </summary>
    public void Report()
    {
        if (reported) return;
        reported = true;

        photonView.RPC(nameof(RPC_Freeze), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void RPC_Freeze()
    {
        Debug.Log($"{displayName} 건의됨 → AI 정지");

        if (aiController != null)
            aiController.enabled = false;

        if (teachersController != null)
            teachersController.enabled = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animController != null)
        {
            animController.SetSpeed(0f);
        }
    }
}

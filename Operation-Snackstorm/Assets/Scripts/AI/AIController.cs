using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public enum AIState
{
    Idle,
    Patrol,
    Chase
}

public enum AIAnimationState
{
    Idle,
    Walk,
    Run
}

public enum PatrolType
{
    Route,
    RandomPoint
}

public class AIController : MonoBehaviourPun
{
    [Header("FOV")]
    public float viewAngle;
    public float viewRadius;

    public float restrictedViewAngle;
    public float restrictedViewRadius;

    private float defaultViewAngle;
    private float defaultViewRadius;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public bool isSightRestricted = false;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    [SerializeField] private int currentIndex = -1;
    public float pointReachThreshold = 1f;
    [SerializeField] private float patrolSpeed = 6f;

    [Header("Chase")]
    public Transform target;
    [SerializeField] private float losetargetDistance = 15f;
    [SerializeField] private float chaseSpeed = 9f;

    public PatrolType patrolType;

    [SerializeField] private AIState currentState;
    private NavMeshAgent agent;
    private AIAnimationController animationController;

    void Start()
    {
        GameManager.Instance.aiList.Add(this.gameObject);
        agent = GetComponent<NavMeshAgent>();
        animationController = GetComponent<AIAnimationController>();

        defaultViewAngle = viewAngle;
        defaultViewRadius = viewRadius;

        if (patrolPoints.Length > 0)
        {
            currentIndex = 0;
            if (PhotonNetwork.IsMasterClient)
            {
                agent.SetDestination(patrolPoints[currentIndex].position);

                if (Vector3.Distance(transform.position, patrolPoints[currentIndex].position) <= pointReachThreshold)
                {
                    currentIndex = (currentIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[currentIndex].position);
                }
            }
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (isSightRestricted)
        {
            viewAngle = restrictedViewAngle;
            viewRadius = restrictedViewRadius;
        }
        else
        {
            viewAngle = defaultViewAngle;
            viewRadius = defaultViewRadius;
        }

        CheckSight();

        float blendSpeed = 0f;
        switch (currentState)
        {
            case AIState.Idle:
                agent.speed = 0f;
                blendSpeed = 0f;
                break;

            case AIState.Patrol:
                agent.speed = patrolSpeed;
                blendSpeed = patrolSpeed;
                Patrol();
                break;

            case AIState.Chase:
                agent.speed = chaseSpeed;
                blendSpeed = chaseSpeed;
                ChaseTarget();
                break;
        }
        animationController.SetSpeed(blendSpeed);
    }

    void CheckSight()
    {
        if (isSightRestricted)
        {
            target = null;
            if (currentState == AIState.Chase) currentState = AIState.Patrol;
            return;
        }

        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        bool seeTarget = false;

        foreach (Collider targetCol in targets)
        {
            PlayerController player = targetCol.GetComponent<PlayerController>();
            if (player != null && !player.isCatchable) continue;

            Transform targetTransform = targetCol.transform;
            Vector3 dirToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2f)
            {
                float distance = Vector3.Distance(transform.position, targetTransform.position);
                if (!Physics.Raycast(transform.position, dirToTarget, distance, obstacleMask))
                {
                    seeTarget = true;
                    target = targetCol.transform;
                    break;
                }
            }
        }

        if (seeTarget)  // 시야에 플레이어가 있다면
        {
            currentState = AIState.Chase;   // 추적 상태
        }
        else
        {
            if (currentState != AIState.Chase)  //시야에 플레이어가 없으며 추적 상태가 아니라면
            {
                currentState = AIState.Patrol;  // 순찰 상태
            }
        }
    }

    void Patrol()
    {
        switch (patrolType)
        {
            case PatrolType.Route:
                PatrolRoute();
                break;

            case PatrolType.RandomPoint:
                PatrolRandom();
                break;
        }
    }

    void PatrolRoute()
    {
        if (patrolPoints.Length == 0) return;

        if (currentIndex == -1)
        {
            currentIndex = 0;
            agent.SetDestination(patrolPoints[currentIndex].position);
        }
        else if (!agent.pathPending && agent.remainingDistance <= pointReachThreshold && !float.IsNaN(agent.remainingDistance))
        {
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentIndex].position);
        }
    }

    void PatrolRandom()
    {
        if (patrolPoints.Length == 0) return;

        if (currentIndex == -1 || (agent.remainingDistance <= pointReachThreshold && agent.hasPath && !float.IsNaN(agent.remainingDistance)))
        {
            int randomIndex = Random.Range(0, patrolPoints.Length);
            if (randomIndex == currentIndex && patrolPoints.Length > 1)
            {
                randomIndex = (randomIndex + 1) % patrolPoints.Length;
            }
            currentIndex = randomIndex;
            agent.SetDestination(patrolPoints[currentIndex].position);
        }
    }

    void ChaseTarget()
    {
        if (target == null)                 // 타겟이 없으면
        {
            currentState = AIState.Patrol;  // 순찰 상태
            return;
        }

        PlayerController player = target.GetComponent<PlayerController>();
        if (player == null || !player.isCatchable)  // 플레이어를 잡을 수 없는 상태라면
        {
            target = null;                          // 타겟을 null 로 바꾼 후
            currentState = AIState.Patrol;          // 순찰 상태
            agent.SetDestination(patrolPoints[currentIndex].position);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > losetargetDistance)  // 타겟 플레이어와의 거리가 멀어지면
        {
            target = null;                  // 타겟을 null로 바꾼 후
            currentState = AIState.Patrol;  //순찰 상태
            return;
        }

        agent.SetDestination(target.position);
    }

    public virtual void OnCatchTarget(PlayerController player)
    {
        player.characterController.enabled = false;
        player.gameObject.transform.position = GameManager.Instance.spawnPoint.position;
        player.characterController.enabled = true;
    }

    [PunRPC]
    private void RPC_HandleCatch(int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView playerPV = PhotonView.Find(playerViewID);
        if (playerPV != null)
        {
            PlayerController player = playerPV.GetComponent<PlayerController>();
            if (player != null && player.isCatchable)
            {
                switch (this)
                {
                    case TeachersController teachers:
                        teachers.OnCatchTarget(player);
                        break;

                    default:
                        OnCatchTarget(player);
                        break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            DoorController door = other.GetComponent<DoorController>();
            if (door != null)
            {
                door.AgentEntered();
                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(AIDoor(door));
                }
            }
        }

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                photonView.RPC("RPC_HandleCatch", RpcTarget.MasterClient, player.photonView.ViewID);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            DoorController door = other.GetComponent<DoorController>();
            if (door != null)
            {
                door.AgentExited();
            }
        }
    }

    IEnumerator AIDoor(DoorController door)
    {
        agent.isStopped = true;

        yield return new WaitForSeconds(1f);

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentIndex].position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }

    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(agent.velocity);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            agent.velocity = (Vector3)stream.ReceiveNext();
            agent.nextPosition = transform.position;
        }
    }
}
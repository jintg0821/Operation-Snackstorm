using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

public class AIController : MonoBehaviour
{
    [Header("FOV")]
    public float viewAngle;
    public float viewRadius;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    [SerializeField] private int currentIndex = -1;
    public float pointReachThreshold = 0.5f; //도착 판정 거리

    [Header("Chase")]
    public Transform target;
    [SerializeField] private float losetargetDistance = 15f;

    public PatrolType patrolType;

    [SerializeField] private AIState currentState;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        CheckSight();
        switch (currentState)
        {
            case AIState.Idle:
                break;

            case AIState.Patrol:
                Patrol();
                break;

            case AIState.Chase:
                ChaseTarget();
                break;
        }
    }

    void CheckSight()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        bool seeTarget = false;

        foreach (Collider targetCol in targets)
        {
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

        if (seeTarget)
        {
            currentState = AIState.Chase;
        }
        else
        {
            if (currentState != AIState.Chase)
            {
                currentState = AIState.Patrol;
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

        if (currentIndex == -1 || agent.remainingDistance <= pointReachThreshold)
        {
            currentIndex = (currentIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentIndex].position);
        }
    }

    void PatrolRandom()
    {
        if (patrolPoints.Length == 0) return;

        if (currentIndex == -1 || agent.remainingDistance <= pointReachThreshold)
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
        if (target == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > losetargetDistance)
        {
            target = null;
            currentState = AIState.Patrol;
            return;
        }

        agent.SetDestination(target.position);
    }


    void OnDrawGizmosSelected()
    {
        // 시야 범위 원
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        // 시야 각도 방향선 계산
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2, false);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2, false);

        // 시야 각도 선
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        // 타겟 방향선 (감지된 경우만)
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }

    // 각도 → 방향 벡터 변환
    private Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

}
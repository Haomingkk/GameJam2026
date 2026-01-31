using UnityEngine;
using UnityEngine.AI;
using GameJam26;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform player;
    
    [Header("Detection Settings")]
    [SerializeField] float detectionRange = 10f;
    [SerializeField] LayerMask detectionLayer;
    [SerializeField] LayerMask obstacleLayers;
    
    [Header("Movement Settings")]
    [SerializeField] float chaseSpeed = 3.5f;
    [SerializeField] float patrolSpeed = 2f;
    
    [Header("Patrol/Guard Area")]
    [SerializeField] Transform guardPosition;
    [SerializeField] float guardRadius = 5f;
    [SerializeField] bool isRoaming = false;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] string guardTag = "GuardPosition";
    Transform[] guardPositions;
    
    [Header("Door Breaking")]
    [SerializeField] float doorDetectionRange = 3f;
    [SerializeField] float doorHitInterval = 0.5f;
    [SerializeField] float stuckCheckInterval = 1f;
    [SerializeField] float stuckThreshold = 0.5f;

    NavMeshAgent agent;
    
    enum EnemyState { Idle, Return, Following }
    EnemyState currentState = EnemyState.Idle;
    
    int currentPatrolIndex = 0;
    Door targetDoor = null;
    float doorHitTimer = 0f;
    
    Vector3 lastStuckCheckPosition;
    float stuckCheckTimer = 0f;
    bool isStuck = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = true;
        agent.speed = patrolSpeed;

        CacheGuardPositions();
        guardPosition = GetNearestGuardPosition();
        
        LoadPatrolPointsFromGuard();
            
        lastStuckCheckPosition = transform.position;
    }

    void LoadPatrolPointsFromGuard()
    {
        if (guardPosition != null && guardPosition.childCount > 0)
        {
            List<Transform> points = new List<Transform>();
            foreach (Transform child in guardPosition)
            {
                points.Add(child);
            }
            patrolPoints = points.ToArray();
        }
    }

    void Update()
    {
        CheckForPlayer();
        CheckIfStuck();
        
        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
                
            case EnemyState.Following:
                UpdateFollowing();
                break;
                
            case EnemyState.Return:
                UpdateReturn();
                break;
        }
    }

    void UpdateIdle()
    {
        if (isRoaming && patrolPoints != null && patrolPoints.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, guardPosition.position) > 0.5f)
            {
                agent.SetDestination(guardPosition.position);
            }
        }
    }

    void UpdateFollowing()
    {
        if (player == null)
        {
            EnterReturn();
            return;
        }
        
        if (!CanSeePlayer())
        {
            EnterReturn();
            return;
        }
        
        if (isStuck)
        {
            TryBreakNearbyDoor();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    void UpdateReturn()
    {
        if (guardPosition == null || !IsReachable(guardPosition.position))
        {
            guardPosition = GetNearestGuardPosition();
        }

        Vector3 returnPos = guardPosition.position;
        
        if (Vector3.Distance(transform.position, returnPos) < 1f)
        {
            EnterIdle();
        }
        else
        {
            agent.SetDestination(returnPos);
        }
        
        if (CanSeePlayer())
        {
            EnterFollowing();
        }
    }

    void EnterIdle()
    {
        currentState = EnemyState.Idle;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        targetDoor = null;
        doorHitTimer = 0f;
        isStuck = false;
    }

    void EnterFollowing()
    {
        currentState = EnemyState.Following;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        targetDoor = null;
        doorHitTimer = 0f;
        Debug.Log("Enemy started following player!");
    }

    void EnterReturn()
    {
        currentState = EnemyState.Return;
        agent.speed = patrolSpeed;
        agent.isStopped = false;
        guardPosition = GetNearestGuardPosition();
        targetDoor = null;
        doorHitTimer = 0f;
        isStuck = false;
        Debug.Log("Enemy returning to guard position.");
    }

    void CheckIfStuck()
    {
        if (currentState != EnemyState.Following) 
        {
            isStuck = false;
            return;
        }
        
        stuckCheckTimer += Time.deltaTime;
        
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastStuckCheckPosition);
            isStuck = distanceMoved < stuckThreshold;
            
            lastStuckCheckPosition = transform.position;
            stuckCheckTimer = 0f;
        }
    }

    void TryBreakNearbyDoor()
    {
        if (targetDoor == null)
        {
            targetDoor = FindNearestDoor();
        }
        
        if (targetDoor == null)
        {
            agent.isStopped = false;
            return;
        }
        
        float doorDistance = Vector3.Distance(transform.position, targetDoor.transform.position);
        
        if (doorDistance > doorDetectionRange)
        {
            targetDoor = null;
            agent.isStopped = false;
            return;
        }
        
        agent.isStopped = true;
        
        doorHitTimer += Time.deltaTime;
        if (doorHitTimer >= doorHitInterval)
        {
            targetDoor.NotifyAttacked();
            doorHitTimer = 0f;
            Debug.Log("Enemy attacking door!");
        }
    }

    Door FindNearestDoor()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, doorDetectionRange);
        
        Door nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (Collider2D col in colliders)
        {
            Door door = col.GetComponent<Door>();
                
            if (door != null)
            {
                float dist = Vector3.Distance(transform.position, door.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = door;
                }
            }
        }
        
        return nearest;
    }

    void CheckForPlayer()
    {
        if (player == null) return;
        if (currentState == EnemyState.Following) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= detectionRange && CanSeePlayer())
        {
            EnterFollowing();
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > detectionRange) return false;

        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D[] hits = new RaycastHit2D[10];
        int hitCount = Physics2D.RaycastNonAlloc(transform.position, direction, hits, distanceToPlayer, obstacleLayers);

        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider == null) continue;
            
            Door door = hits[i].collider.GetComponent<Door>();
            
            if (door == null)
            {
                return false;
            }
        }

        return true;
    }

    void CacheGuardPositions()
    {
        try
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(guardTag);
            List<Transform> list = new List<Transform>();
            foreach (GameObject obj in objs)
            {
                if (obj != null)
                    list.Add(obj.transform);
            }
            guardPositions = list.ToArray();
        }
        catch
        {
            guardPositions = new Transform[0];
            Debug.LogWarning($"Enemy: Tag '{guardTag}' not found. Using default guard position.");
        }
    }

    Transform GetNearestGuardPosition()
    {
        if (guardPositions == null || guardPositions.Length == 0)
        {
            return guardPosition != null ? guardPosition : transform;
        }

        Transform best = null;
        float bestDist = float.MaxValue;

        foreach (Transform t in guardPositions)
        {
            if (t == null) continue;
            if (!IsReachable(t.position)) continue;

            float dist = Vector3.Distance(transform.position, t.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        if (best == null)
        {
            foreach (Transform t in guardPositions)
            {
                if (t != null)
                {
                    best = t;
                    break;
                }
            }
        }

        return best != null ? best : (guardPosition != null ? guardPosition : transform);
    }

    bool IsReachable(Vector3 targetPosition)
    {
        if (agent == null) return false;
        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(targetPosition, path)) return false;
        return path.status == NavMeshPathStatus.PathComplete;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, doorDetectionRange);
        
    }
}
        
        
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform target;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateUpAxis = false;
        agent.updateRotation = false;
        agent.updatePosition = false;

        agent.Warp(transform.position);
    }

    void Update()
    {
        if (!target) return;

        agent.SetDestination(target.position);

        transform.position = agent.nextPosition;

        Vector2 v = agent.desiredVelocity;
        if (v.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}

using UnityEngine;
using UnityEngine.AI;

public class NPCVisitor : MonoBehaviour
{
    [Header("Movement")]
    public float walkRadius = 10f;
    public float waitTime = 3f;

    private Animator animator;
    private NavMeshAgent agent;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        FindNewDestination();
    }

    void Update()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTime)
            {
                FindNewDestination();
                waitTimer = 0f;
            }
        }
    }

    void FindNewDestination()
    {
        Vector3 randomPosition =
            Random.insideUnitSphere * walkRadius;

        randomPosition += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomPosition,
            out hit,
            walkRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
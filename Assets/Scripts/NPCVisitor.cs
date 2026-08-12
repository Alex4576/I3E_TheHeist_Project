using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the random roaming behaviour and movement animations
/// of normal NPC visitors within the museum.
/// </summary>
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
            animator.SetFloat("Speed", agent.velocity.magnitude);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
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
        Vector3 randomPosition = Random.insideUnitSphere * walkRadius + transform.position;
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ---------------- Catch Visitor ----------------
    public void CatchVisitor()
    {
        // Find the GameplayUI in the scene
        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null)
        {
            // Deduct 10 seconds from the global countdown
            ui.AdjustTimer(-10f);

            // Show dialogue feedback
            ui.SetDialogue("That person is not involved in the heist... 10 seconds deducted from timer.");
        }
    }
}

/*
* Author: Sheryn Batrisyia
* Date: 13/08/2026
* Description: Controls the behaviour of normal NPC visitors within the museum.
* Visitors randomly roam around the NavMesh and switch between idle and
* walking animations based on their movement speed. The script also handles
* incorrect player catches by deducting time from the gameplay timer.
*/

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the random roaming behaviour, movement animations,
/// and catch interaction of normal NPC visitors within the museum.
/// </summary>
public class NPCVisitor : MonoBehaviour
{
    [Header("Movement")]
    public float walkRadius = 10f;
    public float waitTime = 3f;

    private Animator animator;
    private NavMeshAgent agent;
    private float waitTimer = 0f;

    /// <summary>
    /// Gets the visitor's NavMeshAgent and Animator components,
    /// then assigns an initial random destination.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        FindNewDestination();
    }

    /// <summary>
    /// Updates the visitor's walking animation based on movement speed
    /// and selects a new destination after the visitor has waited.
    /// </summary>
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

    /// <summary>
    /// Finds a random valid position on the NavMesh within the visitor's
    /// walking radius and sets it as the visitor's next destination.
    /// </summary>
    void FindNewDestination()
    {
        Vector3 randomPosition = Random.insideUnitSphere * walkRadius + transform.position;
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ---------------- Catch Visitor ----------------

    /// <summary>
    /// Handles the player incorrectly catching a normal visitor.
    /// Deducts 10 seconds from the gameplay timer and displays
    /// feedback informing the player that the wrong person was caught.
    /// </summary>
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

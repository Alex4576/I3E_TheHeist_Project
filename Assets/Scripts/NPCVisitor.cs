/*
* Author: Sheryn Batrisyia
* Date: 12/08/2026
* Description: Controls the movement and animation behaviour of normal
* NPC visitors in the museum. Visitors move between random locations
* on the NavMesh, wait briefly after reaching each destination, and
* then select a new destination to continue roaming around the museum.
*/

using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the random roaming behaviour and movement animations
/// of normal NPC visitors within the museum.
/// </summary>
public class NPCVisitor : MonoBehaviour
{
    [Header("Movement")]

    /// <summary>
    /// Maximum radius around the visitor used when searching
    /// for a new random destination on the NavMesh.
    /// </summary>
    public float walkRadius = 10f;

    /// <summary>
    /// Amount of time the visitor waits after reaching a
    /// destination before moving to another location.
    /// </summary>
    public float waitTime = 3f;


    // Animator used to switch between the visitor's
    // idle and walking animations.
    private Animator animator;

    // NavMeshAgent responsible for moving the visitor
    // around the museum.
    private NavMeshAgent agent;

    // Tracks how long the visitor has been waiting
    // at the current destination.
    private float waitTimer = 0f;


    /// <summary>
    /// Gets the visitor's NavMeshAgent and Animator components,
    /// then selects the first random destination.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        FindNewDestination();
    }


    /// <summary>
    /// Updates the visitor's movement animation and checks whether
    /// the visitor has reached its current destination. After waiting
    /// for the specified amount of time, a new destination is selected.
    /// </summary>
    void Update()
    {
        // Update the Speed parameter based on the NavMeshAgent's
        // current movement speed.
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        // Check whether the visitor has reached its destination.
        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;

            // Select another destination after waiting.
            if (waitTimer >= waitTime)
            {
                FindNewDestination();
                waitTimer = 0f;
            }
        }
    }


    /// <summary>
    /// Generates a random position around the visitor and searches
    /// for the nearest valid point on the NavMesh. If a valid point
    /// is found, it becomes the visitor's next destination.
    /// </summary>
    void FindNewDestination()
    {
        Vector3 randomPosition =
            Random.insideUnitSphere * walkRadius;

        randomPosition += transform.position;

        NavMeshHit hit;

        // Ensure that the randomly generated position exists
        // on a valid NavMesh area before moving the visitor.
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
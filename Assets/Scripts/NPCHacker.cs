using UnityEngine;
using UnityEngine.AI;

public class NPCHacker : MonoBehaviour
{
    public enum HackerState
    {
        Roaming,
        GoingToCCTV,
        Hacking,
        Pausing,
        Caught
    }

    [Header("Movement")]
    [SerializeField] private float walkRadius = 10f;
    [SerializeField] private float roamDelay = 3f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("CCTV")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float hackRange = 2.5f;
    [SerializeField] private float hackDuration = 3f;
    [SerializeField] private float pauseAfterHack = 2f;

    private NavMeshAgent agent;
    private NPCCCTV targetCCTV;

    private float roamTimer;
    private float hackTimer;
    private float pauseTimer;

    public HackerState currentState = HackerState.Roaming;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NPCHacker needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        roamTimer = roamDelay;

        // Look for a CCTV when the hacker starts
        FindNearestActiveCCTV();

        if (targetCCTV != null)
        {
            currentState = HackerState.GoingToCCTV;
        }
    }

    void Update()
    {
        if (agent == null)
            return;

        if (currentState == HackerState.Caught)
            return;

        switch (currentState)
        {
            case HackerState.Roaming:
                Roam();
                break;

            case HackerState.GoingToCCTV:
                GoToCCTV();
                break;

            case HackerState.Hacking:
                HackCCTV();
                break;

            case HackerState.Pausing:
                PauseAfterHack();
                break;
        }
    }

    // =====================================================
    // ROAMING
    // =====================================================

    void Roam()
    {
        // Check if there is an active CCTV available
        FindNearestActiveCCTV();

        if (targetCCTV != null)
        {
            currentState = HackerState.GoingToCCTV;
            return;
        }

        roamTimer -= Time.deltaTime;

        if (roamTimer <= 0f ||
            (!agent.pathPending &&
             agent.remainingDistance <= agent.stoppingDistance))
        {
            SetRandomDestination();

            roamTimer = roamDelay;
        }
    }

    // =====================================================
    // GO TO CCTV
    // =====================================================

    void GoToCCTV()
    {
        // CCTV no longer exists
        if (targetCCTV == null)
        {
            FindNearestActiveCCTV();

            if (targetCCTV == null)
            {
                currentState = HackerState.Roaming;
            }

            return;
        }

        // CCTV was already disabled
        if (!targetCCTV.IsActive())
        {
            targetCCTV = null;

            FindNearestActiveCCTV();

            if (targetCCTV != null)
            {
                currentState = HackerState.GoingToCCTV;
            }
            else
            {
                currentState = HackerState.Roaming;
            }

            return;
        }

        float distance = Vector3.Distance(
            transform.position,
            targetCCTV.transform.position
        );

        // Close enough to start hacking
        if (distance <= hackRange)
        {
            agent.ResetPath();

            hackTimer = hackDuration;
            currentState = HackerState.Hacking;

            Debug.Log("Hacker is hacking " + targetCCTV.name);

            return;
        }

        // Keep walking towards CCTV
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(targetCCTV.transform.position);
        }
    }

    // =====================================================
    // HACK CCTV
    // =====================================================

    void HackCCTV()
    {
        if (targetCCTV == null)
        {
            FindNearestActiveCCTV();

            if (targetCCTV != null)
                currentState = HackerState.GoingToCCTV;
            else
                currentState = HackerState.Roaming;

            return;
        }

        // CCTV somehow became disabled
        if (!targetCCTV.IsActive())
        {
            targetCCTV = null;
            FindNearestActiveCCTV();

            if (targetCCTV != null)
                currentState = HackerState.GoingToCCTV;
            else
                currentState = HackerState.Roaming;

            return;
        }

        // Wait for hacking to finish
        hackTimer -= Time.deltaTime;

        if (hackTimer <= 0f)
        {
            // Disable this CCTV
            targetCCTV.DisableCamera();

            Debug.Log("Hacker hacked " + targetCCTV.name);

            // Forget this CCTV
            targetCCTV = null;

            // Pause before looking for another CCTV
            pauseTimer = pauseAfterHack;
            currentState = HackerState.Pausing;
        }
    }

    // =====================================================
    // PAUSE
    // =====================================================

    void PauseAfterHack()
    {
        pauseTimer -= Time.deltaTime;

        if (pauseTimer <= 0f)
        {
            // Look for another active CCTV
            FindNearestActiveCCTV();

            if (targetCCTV != null)
            {
                currentState = HackerState.GoingToCCTV;
            }
            else
            {
                // All CCTVs are disabled
                currentState = HackerState.Roaming;
                roamTimer = 0f;
            }
        }
    }

    // =====================================================
    // FIND NEAREST ACTIVE CCTV
    // =====================================================

    void FindNearestActiveCCTV()
    {
        NPCCCTV[] cameras =
            FindObjectsByType<NPCCCTV>(FindObjectsSortMode.None);

        float closestDistance = detectionRange;
        NPCCCTV closestCamera = null;

        foreach (NPCCCTV camera in cameras)
        {
            if (camera == null)
                continue;

            // IMPORTANT:
            // Ignore cameras that have already been hacked
            if (!camera.IsActive())
                continue;

            float distance = Vector3.Distance(
                transform.position,
                camera.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCamera = camera;
            }
        }

        targetCCTV = closestCamera;
    }

    // =====================================================
    // RANDOM ROAM
    // =====================================================

    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh)
            return;

        Vector3 randomDirection =
            Random.insideUnitSphere * walkRadius;

        randomDirection += transform.position;

        randomDirection.y = 0f;

        if (NavMesh.SamplePosition(
            randomDirection,
            out NavMeshHit hit,
            walkRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // =====================================================
    // CATCH
    // =====================================================

    public void Catch()
    {
        if (currentState == HackerState.Caught)
            return;

        currentState = HackerState.Caught;

        if (agent != null)
            agent.ResetPath();

        Debug.Log("Hacker caught!");

        Destroy(gameObject);
    }
}
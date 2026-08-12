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
        Evading,
        Caught
    }

    public enum HackerMode
    {
        Scouting,
        Hunting
    }

    public HackerState currentState = HackerState.Roaming;
    public HackerMode currentMode = HackerMode.Scouting;

    [Header("Movement")]
    [SerializeField] private float walkRadius = 10f;
    [SerializeField] private float roamDelay = 3f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("CCTV")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float hackRange = 2.5f;
    [SerializeField] private float hackDuration = 3f;
    [SerializeField] private float pauseAfterHack = 2f;

    [Header("Scouting")]
    [SerializeField] private float scoutDurationMin = 8f;
    [SerializeField] private float scoutDurationMax = 15f;

    [Header("Hunting")]
    [SerializeField] private float huntDurationMin = 5f;
    [SerializeField] private float huntDurationMax = 10f;

    [Header("Player Evasion")]
    [SerializeField] private float playerAvoidRadius = 6f;
    [SerializeField] private float evadeDuration = 4f;

    private NavMeshAgent agent;
    private Animator animator;
    private NPCCCTV targetCCTV;
    private Transform playerTransform;

    private float roamTimer;
    private float hackTimer;
    private float pauseTimer;
    private float scoutTimer;
    private float huntTimer;
    private float evadeTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent == null)
        {
            Debug.LogError("NPCHacker needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        roamTimer = roamDelay;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        // Start out roaming naturally instead of beelining for a camera immediately
        currentMode = HackerMode.Scouting;
        scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);

        currentState = HackerState.Roaming;
    }

    void Update()
    {
        if (agent == null)
            return;

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

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

            case HackerState.Evading:
                Evade();
                break;
        }
    }

    // =====================================================
    // ROAMING (Scouting / Hunting mode switch lives here)
    // =====================================================

    void Roam()
    {
        if (currentMode == HackerMode.Scouting)
        {
            scoutTimer -= Time.deltaTime;

            if (scoutTimer <= 0f)
            {
                currentMode = HackerMode.Hunting;
                huntTimer = Random.Range(huntDurationMin, huntDurationMax);
                Debug.Log(name + " is now looking for a camera to hack.");
            }
        }
        else if (currentMode == HackerMode.Hunting)
        {
            FindNearestActiveCCTV();

            if (targetCCTV != null)
            {
                currentState = HackerState.GoingToCCTV;
                return;
            }

            // No camera found yet — count down the hunting window
            huntTimer -= Time.deltaTime;

            if (huntTimer <= 0f)
            {
                currentMode = HackerMode.Scouting;
                scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);
                Debug.Log(name + " gave up hunting, back to scouting.");
            }
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
        // Player got too close while approaching — bail out and evade
        if (IsPlayerNear())
        {
            StartEvading();
            return;
        }

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

        // Compare horizontal distance only — cameras mounted up on walls/ceilings
        // would otherwise never register as "close enough" due to the height
        // difference, even when the hacker is standing right underneath them.
        Vector3 flatHackerPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatCamPos = new Vector3(targetCCTV.transform.position.x, 0f, targetCCTV.transform.position.z);
        float distance = Vector3.Distance(flatHackerPos, flatCamPos);

        // Close enough to start hacking
        if (distance <= hackRange)
        {
            agent.ResetPath();

            hackTimer = hackDuration;
            currentState = HackerState.Hacking;

            Debug.Log(name + " is hacking " + targetCCTV.name);

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
        // Player got too close mid-hack — abort and evade
        if (IsPlayerNear())
        {
            Debug.Log(name + " spotted the player — aborting hack!");
            targetCCTV = null;
            StartEvading();
            return;
        }

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

            Debug.Log(name + " hacked " + targetCCTV.name);

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
            // Go back to roaming naturally rather than immediately re-hunting
            currentState = HackerState.Roaming;
            currentMode = HackerMode.Scouting;
            scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);
        }
    }

    // =====================================================
    // EVADE
    // =====================================================

    void StartEvading()
    {
        agent.ResetPath();
        evadeTimer = evadeDuration;
        currentState = HackerState.Evading;
        Debug.Log(name + " is evading the player.");
    }

    void Evade()
    {
        // Keep moving away from the player while evading
        if (playerTransform != null && agent.isOnNavMesh &&
            !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 awayFromPlayer = (transform.position - playerTransform.position).normalized;
            Vector3 retreatPoint = transform.position + awayFromPlayer * walkRadius;

            if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        evadeTimer -= Time.deltaTime;

        if (evadeTimer <= 0f)
        {
            if (IsPlayerNear())
            {
                // Still too close — keep evading a while longer
                evadeTimer = evadeDuration;
            }
            else
            {
                currentState = HackerState.Roaming;
                currentMode = HackerMode.Scouting;
                scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);
                Debug.Log(name + " resumed roaming.");
            }
        }
    }

    bool IsPlayerNear()
    {
        if (playerTransform == null) return false;
        return Vector3.Distance(transform.position, playerTransform.position) <= playerAvoidRadius;
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

        // Compare horizontal distance only — see note in GoToCCTV() above.
        Vector3 flatHackerPos = new Vector3(transform.position.x, 0f, transform.position.z);

        foreach (NPCCCTV camera in cameras)
        {
            if (camera == null)
                continue;

            // IMPORTANT:
            // Ignore cameras that have already been hacked
            if (!camera.IsActive())
                continue;

            Vector3 flatCamPos = new Vector3(camera.transform.position.x, 0f, camera.transform.position.z);
            float distance = Vector3.Distance(flatHackerPos, flatCamPos);

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

        Debug.Log(name + " caught!");

        Destroy(gameObject);
    }
}
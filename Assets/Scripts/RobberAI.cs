using UnityEngine;
using UnityEngine.AI;

public class RobberAI : MonoBehaviour
{
    public enum RobberState
    {
        Roaming,
        GoingToItem,
        Stealing,
        Fleeing,
        Caught
    }

    public enum RobberMode
    {
        Scouting,
        Stealing
    }

    public RobberMode currentMode = RobberMode.Scouting;
    public RobberState currentState = RobberState.Roaming;

    [Header("Movement")]
    [SerializeField] private float walkRadius = 10f;
    [SerializeField] private float roamDelay = 3f;
    [SerializeField] private float stoppingDistance = 1.5f;

    [Header("Item Interaction")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private float stealRange = 2f;
    [SerializeField] private float stealDuration = 3f;

    [Header("Steal Chance")]
    [SerializeField, Range(0, 1)] private float stealChanceCameraActive = 0.25f;
    [SerializeField, Range(0, 1)] private float stealChanceCameraDown = 0.85f;
    [SerializeField] private float cameraCheckRadius = 15f;

    [Header("Scouting")]
    [SerializeField] private float scoutDurationMin = 8f;
    [SerializeField] private float scoutDurationMax = 15f;

    [Header("Hunting")]
    [SerializeField] private float huntDurationMin = 5f;
    [SerializeField] private float huntDurationMax = 10f;

    [Header("Fleeing / Exit")]
    [SerializeField] private Transform exitPoint;        // drag the lift's position here
    [SerializeField] private float playerAvoidRadius = 6f;
    [SerializeField] private float avoidRepathInterval = 0.5f;
    [SerializeField] private float escapeDistance = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private StealableItem targetItem;
    private StealableItem stolenItem;
    private Transform playerTransform;

    private bool detectedByScan;
    private bool holdingItem;

    private float roamTimer;
    private float stealTimer;
    private Animator animator;
    private float scoutTimer;
    private float huntTimer;
    private float avoidRepathTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        
        if (agent == null)
        {
            Debug.LogError("RobberAI needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        roamTimer = roamDelay;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        currentMode = RobberMode.Scouting;
        scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);

        currentState = RobberState.Roaming;
    }

    void Update()
    {
        if (agent == null)
            return;

        // Drive movement animation off the agent's current speed
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        if (currentState == RobberState.Caught)
            return;

        switch (currentState)
        {
            case RobberState.Roaming: Roam(); break;
            case RobberState.GoingToItem: GoToItem(); break;
            case RobberState.Stealing: DoSteal(); break;
            case RobberState.Fleeing: Flee(); break;
        }
    }

    // Called externally by NPCCCTV while scanning
    public void OnDetectedByScan()
    {
        detectedByScan = true;

        // Interrupt a theft in progress
        if (currentState == RobberState.Stealing)
        {
            Debug.Log(name + " spotted by scan — aborting steal!");
            currentState = RobberState.Fleeing;
        }
    }

    // =====================================================
    // ROAMING (Scouting / Hunting mode switch lives here)
    // =====================================================

    void Roam()
    {
        if (currentMode == RobberMode.Scouting)
        {
            scoutTimer -= Time.deltaTime;

            if (scoutTimer <= 0f)
            {
                currentMode = RobberMode.Stealing;
                huntTimer = Random.Range(huntDurationMin, huntDurationMax);
                Debug.Log(name + " is now looking for something to steal.");
            }
        }
        else if (currentMode == RobberMode.Stealing)
        {
            FindNearestItem();

            if (targetItem != null)
            {
                currentState = RobberState.GoingToItem;
                return;
            }

            // No item found yet — count down the hunting window
            huntTimer -= Time.deltaTime;

            if (huntTimer <= 0f)
            {
                currentMode = RobberMode.Scouting;
                scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);
                Debug.Log(name + " gave up hunting, back to scouting.");
            }
        }

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f ||
            (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            SetRandomDestination();
            roamTimer = roamDelay;
        }
    }

    // =====================================================
    // GO TO ITEM
    // =====================================================

    void GoToItem()
    {
        if (targetItem == null || targetItem.IsStolen)
        {
            targetItem = null;
            FindNearestItem();
            currentState = targetItem != null ? RobberState.GoingToItem : RobberState.Roaming;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetItem.transform.position);

        if (distance <= stealRange)
        {
            agent.ResetPath();
            stealTimer = stealDuration;
            detectedByScan = false;
            currentState = RobberState.Stealing;
            return;
        }

        if (agent.isOnNavMesh)
            agent.SetDestination(targetItem.transform.position);
    }

    // =====================================================
    // STEAL
    // =====================================================

    void DoSteal()
    {
        if (targetItem == null || targetItem.IsStolen)
        {
            currentState = RobberState.Roaming;
            return;
        }

        stealTimer -= Time.deltaTime;
        if (stealTimer > 0f) return;

        if (detectedByScan)
        {
            // Safety net — should already have been redirected in OnDetectedByScan
            currentState = RobberState.Fleeing;
            return;
        }

        float chance = IsWatchedByActiveCamera() ? stealChanceCameraActive : stealChanceCameraDown;
        bool success = Random.value <= chance;

        if (success)
        {
            targetItem.Steal();
            holdingItem = true;
            stolenItem = targetItem;
            Debug.Log(name + " successfully stole " + targetItem.name);
        }
        else
        {
            Debug.Log(name + " failed to steal " + targetItem.name);
        }

        targetItem = null;
        currentState = RobberState.Fleeing;
    }

    // =====================================================
    // FLEE
    // =====================================================

    void Flee()
    {
        // If carrying the item, head for the exit instead of roaming
        if (holdingItem && exitPoint != null)
        {
            FleeToExit();
            return;
        }

        // Steal failed — nothing to protect, just retreat and go back to scouting
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomDestination();
            currentState = RobberState.Roaming;

            currentMode = RobberMode.Scouting;
            scoutTimer = Random.Range(scoutDurationMin, scoutDurationMax);
        }
        else if (!agent.hasPath)
        {
            SetRandomDestination();
        }
    }

    void FleeToExit()
    {
        float distToExit = Vector3.Distance(transform.position, exitPoint.position);

        if (distToExit <= escapeDistance)
        {
            Debug.Log(name + " escaped with the stolen item!");
            Destroy(gameObject); // robber + item are gone for good
            return;
        }

        avoidRepathTimer -= Time.deltaTime;
        if (avoidRepathTimer > 0f && agent.hasPath)
            return; // don't recompute the path every single frame

        avoidRepathTimer = avoidRepathInterval;

        Vector3 destination = exitPoint.position;

        if (playerTransform != null)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distToPlayer <= playerAvoidRadius)
            {
                // Steer away from the player, but blend toward the exit
                // so it doesn't just run in circles avoiding them forever
                Vector3 awayFromPlayer = (transform.position - playerTransform.position).normalized;
                Vector3 biasedPoint = transform.position + awayFromPlayer * playerAvoidRadius;
                Vector3 blended = Vector3.Lerp(biasedPoint, exitPoint.position, 0.5f);

                if (NavMesh.SamplePosition(blended, out NavMeshHit hit, playerAvoidRadius, NavMesh.AllAreas))
                    destination = hit.position;
            }
        }

        if (agent.isOnNavMesh)
            agent.SetDestination(destination);
    }

    // =====================================================
    // CAMERA CHECK
    // =====================================================

    bool IsWatchedByActiveCamera()
    {
        NPCCCTV[] cameras = FindObjectsByType<NPCCCTV>(FindObjectsSortMode.None);

        foreach (NPCCCTV cam in cameras)
        {
            if (cam == null || !cam.IsActive()) continue;

            float dist = Vector3.Distance(transform.position, cam.transform.position);
            if (dist <= cameraCheckRadius)
                return true; // an active camera is watching this area
        }

        return false; // no active camera nearby -> treat as "camera down"
    }

    // =====================================================
    // FIND NEAREST ITEM
    // =====================================================

    void FindNearestItem()
    {
        StealableItem[] items = FindObjectsByType<StealableItem>(FindObjectsSortMode.None);

        float closestDistance = detectionRange;
        StealableItem closest = null;

        foreach (StealableItem item in items)
        {
            if (item == null || item.IsStolen) continue;

            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closest = item;
            }
        }

        targetItem = closest;
    }

    // =====================================================
    // RANDOM ROAM
    // =====================================================

    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        randomDirection.y = 0f;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // =====================================================
    // CATCH
    // =====================================================

    public void Catch()
    {
        if (currentState == RobberState.Caught) return;

        currentState = RobberState.Caught;
        if (agent != null) agent.ResetPath();

        if (stolenItem != null)
        {
            stolenItem.Restore();
            stolenItem = null;
        }

        Debug.Log(name + " caught!");
        Destroy(gameObject);
    }
}
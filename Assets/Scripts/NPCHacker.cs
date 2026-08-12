using UnityEngine;
using UnityEngine.AI;

public class NPCHacker : MonoBehaviour
{
    public enum HackerState { Roaming, GoingToCCTV, Hacking, Pausing, Evading, Caught }
    public enum HackerMode  { Scouting, Hunting }

    public HackerState currentState = HackerState.Roaming;
    public HackerMode  currentMode  = HackerMode.Scouting;

    [Header("Movement")]
    [SerializeField] private float walkRadius      = 10f;
    [SerializeField] private float roamDelayMin    = 2f;
    [SerializeField] private float roamDelayMax    = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;
    [SerializeField] private float scoutSpeed      = 2f;
    [SerializeField] private float huntSpeed       = 3.5f;
    [SerializeField] private float evadeSpeed      = 5.5f;

    [Header("CCTV")]
    [SerializeField] private float detectionRange  = 20f;
    [SerializeField] private float hackRange       = 2.5f;
    [SerializeField] private float hackDuration    = 3f;
    [SerializeField] private float pauseMinAfterHack = 1.5f;
    [SerializeField] private float pauseMaxAfterHack = 3.5f;
    [SerializeField] private float cctvSearchInterval = 1f;

    [Header("Scouting")]
    [SerializeField] private float scoutDurationMin = 8f;
    [SerializeField] private float scoutDurationMax = 15f;

    [Header("Hunting")]
    [SerializeField] private float huntDurationMin = 5f;
    [SerializeField] private float huntDurationMax = 10f;

    [Header("Player Evasion")]
    [SerializeField] private float playerAvoidRadius = 6f;
    [SerializeField] private float evadeDuration    = 4f;
    [SerializeField] private float evadeLateralAngle = 35f; // randomness in flee direction

    [Header("Idle Behaviour")]
    [SerializeField] private float lookAroundChance   = 0.25f; // probability per second
    [SerializeField] private float lookAroundDuration = 1.8f;

    private NavMeshAgent agent;
    private Animator     animator;
    private NPCCCTV      targetCCTV;
    private Transform    playerTransform;

    private float roamTimer;
    private float hackTimer;
    private float pauseTimer;
    private float scoutTimer;
    private float huntTimer;
    private float evadeTimer;
    private float cctvSearchTimer;

    private bool      isLookingAround;
    private float     lookAroundTimer;
    private Quaternion lookAroundTarget;

    

    // =====================================================
    // LIFECYCLE
    // =====================================================

    void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (agent == null)
        {
            Debug.LogError("NPCHacker needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        EnterScouting();
        SetRandomDestination();
    }

    void Update()
    {
        if (agent == null || currentState == HackerState.Caught)
            return;

        UpdateAnimator();

        switch (currentState)
        {
            case HackerState.Roaming:      Roam();          break;
            case HackerState.GoingToCCTV:  GoToCCTV();      break;
            case HackerState.Hacking:      HackCCTV();      break;
            case HackerState.Pausing:      PauseAfterHack();break;
            case HackerState.Evading:      Evade();         break;
        }
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("IsHacking", currentState == HackerState.Hacking);
    }

    // =====================================================
    // ROAMING
    // =====================================================

    void Roam()
    {
        if (currentMode == HackerMode.Scouting)
        {
            scoutTimer -= Time.deltaTime;

            if (scoutTimer <= 0f)
            {
                currentMode     = HackerMode.Hunting;
                agent.speed     = huntSpeed;
                huntTimer       = Random.Range(huntDurationMin, huntDurationMax);
                cctvSearchTimer = 0f; // search immediately on entering hunt
            }
            else
            {
                TickLookAround();
            }
        }
        else
        {
            cctvSearchTimer -= Time.deltaTime;
            if (cctvSearchTimer <= 0f)
            {
                FindNearestActiveCCTV();
                cctvSearchTimer = cctvSearchInterval;
            }

            if (targetCCTV != null)
            {
                currentState = HackerState.GoingToCCTV;
                return;
            }

            huntTimer -= Time.deltaTime;
            if (huntTimer <= 0f)
                EnterScouting();
        }

        roamTimer -= Time.deltaTime;
        if ((roamTimer <= 0f || IsAgentIdle()) && !isLookingAround)
        {
            SetRandomDestination();
            roamTimer = Random.Range(roamDelayMin, roamDelayMax);
        }
    }

    // Randomly pause and glance around while idle during scouting.
    void TickLookAround()
    {
        if (!IsAgentIdle()) return;

        if (!isLookingAround && Random.value < lookAroundChance * Time.deltaTime)
        {
            isLookingAround  = true;
            lookAroundTimer  = lookAroundDuration;
            lookAroundTarget = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        if (isLookingAround)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation, lookAroundTarget, Time.deltaTime * 2.5f);

            lookAroundTimer -= Time.deltaTime;
            if (lookAroundTimer <= 0f)
                isLookingAround = false;
        }
    }

    // =====================================================
    // GO TO CCTV
    // =====================================================

    void GoToCCTV()
    {
        if (IsPlayerNear()) { StartEvading(); return; }
        if (!ValidateTarget()) return;

        if (FlatDistance(transform.position, targetCCTV.transform.position) <= hackRange)
        {
            agent.ResetPath();
            hackTimer    = hackDuration;
            currentState = HackerState.Hacking;
            return;
        }

        if (agent.isOnNavMesh)
            agent.SetDestination(targetCCTV.transform.position);
    }

    // =====================================================
    // HACK CCTV
    // =====================================================

    void HackCCTV()
    {
        if (IsPlayerNear()) { targetCCTV = null; StartEvading(); return; }
        if (!ValidateTarget()) return;

        // Smoothly face the camera while hacking
        Vector3 toCamera = targetCCTV.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(toCamera), Time.deltaTime * 5f);

        hackTimer -= Time.deltaTime;
        if (hackTimer <= 0f)
        {
            targetCCTV.DisableCamera();
            targetCCTV   = null;
            pauseTimer   = Random.Range(pauseMinAfterHack, pauseMaxAfterHack);
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
            EnterScouting();
    }

    // =====================================================
    // EVADE
    // =====================================================

    void StartEvading()
    {
        agent.ResetPath();
        agent.speed  = evadeSpeed;
        evadeTimer   = evadeDuration;
        currentState = HackerState.Evading;
    }

    void Evade()
    {
        if (playerTransform != null && agent.isOnNavMesh && IsAgentIdle())
        {
            Vector3 away = (transform.position - playerTransform.position).normalized;

            // Add lateral jitter so the path isn't a perfect straight line
            away = Quaternion.Euler(0f, Random.Range(-evadeLateralAngle, evadeLateralAngle), 0f) * away;

            Vector3 retreatPoint = transform.position + away * walkRadius;
            if (NavMesh.SamplePosition(retreatPoint, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
                agent.SetDestination(hit.position);
        }

        evadeTimer -= Time.deltaTime;
        if (evadeTimer <= 0f)
        {
            if (IsPlayerNear())
                evadeTimer = evadeDuration;
            else
                EnterScouting();
        }
    }

    // =====================================================
    // HELPERS
    // =====================================================

    void EnterScouting()
    {
        currentState = HackerState.Roaming;
        currentMode  = HackerMode.Scouting;
        agent.speed  = scoutSpeed;
        scoutTimer   = Random.Range(scoutDurationMin, scoutDurationMax);
    }

    // Validates targetCCTV; handles state transition if it's gone, returns false if invalid.
    bool ValidateTarget()
    {
        if (targetCCTV != null && targetCCTV.IsActive())
            return true;

        targetCCTV = null;
        FindNearestActiveCCTV();
        currentState = targetCCTV != null ? HackerState.GoingToCCTV : HackerState.Roaming;
        return false;
    }

    bool IsAgentIdle() =>
        !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

    bool IsPlayerNear() =>
        playerTransform != null &&
        Vector3.Distance(transform.position, playerTransform.position) <= playerAvoidRadius;

    static float FlatDistance(Vector3 a, Vector3 b) =>
        new Vector2(a.x - b.x, a.z - b.z).magnitude;

    void FindNearestActiveCCTV()
    {
        NPCCCTV[] cameras = FindObjectsByType<NPCCCTV>(FindObjectsSortMode.None);
        float   closest = detectionRange;
        NPCCCTV best    = null;

        foreach (NPCCCTV cam in cameras)
        {
            if (cam == null || !cam.IsActive()) continue;
            float d = FlatDistance(transform.position, cam.transform.position);
            if (d < closest) { closest = d; best = cam; }
        }

        targetCCTV = best;
    }

    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 point = transform.position + Random.insideUnitSphere * walkRadius;
        point.y = transform.position.y;

        if (NavMesh.SamplePosition(point, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // =====================================================
    // CATCH
    // =====================================================

    public void Catch()
    {
        if (currentState == HackerState.Caught) return;
        currentState = HackerState.Caught;
        agent?.ResetPath();
        Destroy(gameObject);
    }
}
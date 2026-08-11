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

    private NavMeshAgent agent;
    private StealableItem targetItem;
    private bool detectedByScan;

    private float roamTimer;
    private float stealTimer;

    public RobberState currentState = RobberState.Roaming;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("RobberAI needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        roamTimer = roamDelay;

        FindNearestItem();
        if (targetItem != null)
            currentState = RobberState.GoingToItem;
    }

    void Update()
    {
        if (agent == null || currentState == RobberState.Caught)
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

    void Roam()
    {
        FindNearestItem();
        if (targetItem != null)
        {
            currentState = RobberState.GoingToItem;
            return;
        }

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f ||
            (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            SetRandomDestination();
            roamTimer = roamDelay;
        }
    }

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
            Debug.Log(name + " successfully stole " + targetItem.name);
        }
        else
        {
            Debug.Log(name + " failed to steal " + targetItem.name);
        }

        targetItem = null;
        currentState = RobberState.Fleeing;
    }

    void Flee()
    {
        // Simple flee: run to a random far point, then go back to roaming
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetRandomDestination();
            currentState = RobberState.Roaming;
        }
        else if (!agent.hasPath)
        {
            SetRandomDestination();
        }
    }

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

    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        randomDirection.y = 0f;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    public void Catch()
    {
        if (currentState == RobberState.Caught) return;

        currentState = RobberState.Caught;
        if (agent != null) agent.ResetPath();

        Debug.Log(name + " caught!");
        Destroy(gameObject);
    }
}
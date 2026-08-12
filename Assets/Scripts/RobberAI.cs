using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class RobberAI : MonoBehaviour
{
    public enum RobberState { Roaming, GoingToItem, Stealing, Caught }
    public enum RobberMode { Scouting, Stealing }

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
    [SerializeField, Range(0, 1)] private float stealChanceCameraActive = 0.05f;
    [SerializeField, Range(0, 1)] private float stealChanceCameraDown = 1f;
    [SerializeField] private float cameraCheckRadius = 15f;

    private NavMeshAgent agent;
    private Animator animator;
    private StealableItem targetItem;
    private List<StealableItem> stolenItems = new List<StealableItem>();

    private bool detectedByScan;
    private float roamTimer;
    private float stealTimer;

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
        currentMode = RobberMode.Scouting;
        currentState = RobberState.Roaming;
    }

    void Update()
    {
        if (agent == null || currentState == RobberState.Caught) return;

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        switch (currentState)
        {
            case RobberState.Roaming: Roam(); break;
            case RobberState.GoingToItem: GoToItem(); break;
            case RobberState.Stealing: DoSteal(); break;
        }
    }

    // ---------------- Roaming ----------------
    void Roam()
    {
        FindNearestItem();
        if (targetItem != null)
        {
            currentState = RobberState.GoingToItem;
            return;
        }

        roamTimer -= Time.deltaTime;
        if (roamTimer <= 0f || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance))
        {
            SetRandomDestination();
            roamTimer = roamDelay;
        }
    }

    // ---------------- Go To Item ----------------
    void GoToItem()
    {
        if (targetItem == null || targetItem.IsStolen)
        {
            targetItem = null;
            currentState = RobberState.Roaming;
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

    // ---------------- Steal ----------------
        void DoSteal()
    {
        if (targetItem == null || targetItem.IsStolen)
        {
            currentState = RobberState.Roaming;
            return;
        }

        stealTimer -= Time.deltaTime;
        if (stealTimer > 0f) return;

        // CCTV check: 25% if watched, 100% if not
        float chance = IsWatchedByActiveCamera() ? stealChanceCameraActive : 1f;
        bool success = Random.value <= chance;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();

       if (success)
        {
            targetItem.Steal();
            stolenItems.Add(targetItem);   // add to list
            if (ui != null) ui.SetDialogue("Thief has stolen the " + targetItem.name + "!");
        }
        else
        {
            if (ui != null) ui.SetDialogue("Thief failed to steal the " + targetItem.name + ".");
        }


        targetItem = null;
        currentState = RobberState.Roaming;
    }

    // ---------------- Camera Check ----------------
    bool IsWatchedByActiveCamera()
    {
        NPCCCTV[] cameras = FindObjectsByType<NPCCCTV>(FindObjectsSortMode.None);
        foreach (NPCCCTV cam in cameras)
        {
            if (cam != null && cam.IsActive() &&
                Vector3.Distance(transform.position, cam.transform.position) <= cameraCheckRadius)
                return true;
        }
        return false;
    }

    // ---------------- Find Nearest Item ----------------
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

    // ---------------- Random Roam ----------------
    void SetRandomDestination()
    {
        if (!agent.isOnNavMesh) return;
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, walkRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    // ---------------- OnDetectedByScan ----------------
    public void OnDetectedByScan()
    {
        // Dialogue feedback when CCTV spots a robber
        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null) ui.SetDialogue("A robber has been spotted by CCTV!");
    }

   // ---------------- Catch ----------------
        public void Catch()
{
    if (currentState == RobberState.Caught) return;
    currentState = RobberState.Caught;
    agent?.ResetPath();

    GameplayUI ui = FindFirstObjectByType<GameplayUI>();
    if (stolenItems.Count > 0)
    {
        foreach (StealableItem item in stolenItems)
        {
            item.Restore();
        }

        if (ui != null)
        {
            string itemNames = string.Join(", ", stolenItems.Select(i => i.name));
            ui.SetDialogue("Thief has been caught and the following items are recovered: " + itemNames);
            ui.CatchThief();   // increment thief count in UI
        }

        stolenItems.Clear();
    }
    else
    {
        if (ui != null)
        {
            ui.SetDialogue("The Thief has been caught!");
            ui.CatchThief();   // increment thief count in UI
        }
    }

    Debug.Log(name + " caught!");
    Destroy(gameObject);
}

}

using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class ThiefAI : MonoBehaviour
{
    public enum ThiefState { Roaming, GoingToItem, Stealing, Caught }
    public enum ThiefMode { Scouting, Stealing }

    public ThiefMode currentMode = ThiefMode.Scouting;
    public ThiefState currentState = ThiefState.Roaming;

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
            Debug.LogError("ThiefAI needs a NavMeshAgent!");
            return;
        }

        agent.stoppingDistance = stoppingDistance;
        roamTimer = roamDelay;
        currentMode = ThiefMode.Scouting;
        currentState = ThiefState.Roaming;
    }

    void Update()
    {
        if (agent == null || currentState == ThiefState.Caught) return;

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        switch (currentState)
        {
            case ThiefState.Roaming: Roam(); break;
            case ThiefState.GoingToItem: GoToItem(); break;
            case ThiefState.Stealing: DoSteal(); break;
        }
    }

    // ---------------- Roaming ----------------
    void Roam()
    {
        FindNearestItem();
        if (targetItem != null)
        {
            currentState = ThiefState.GoingToItem;
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
            currentState = ThiefState.Roaming;
            return;
        }

        float distance = Vector3.Distance(transform.position, targetItem.transform.position);
        if (distance <= stealRange)
        {
            agent.ResetPath();
            stealTimer = stealDuration;
            detectedByScan = false;
            currentState = ThiefState.Stealing;
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
            currentState = ThiefState.Roaming;
            return;
        }

        stealTimer -= Time.deltaTime;
        if (stealTimer > 0f) return;

        float chance = IsWatchedByActiveCamera() ? stealChanceCameraActive : 1f;
        bool success = Random.value <= chance;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();

        if (success)
        {
            targetItem.Steal();
            stolenItems.Add(targetItem);
            if (ui != null) ui.SetDialogue("Thief has stolen the " + targetItem.name + "!");
        }
        else
        {
            if (ui != null) ui.SetDialogue("Thief failed to steal the " + targetItem.name + ".");
        }

        targetItem = null;
        currentState = ThiefState.Roaming;
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
        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null) ui.SetDialogue("A thief has been spotted by CCTV!");
    }

    // ---------------- Catch ----------------
    public void Catch()
    {
        if (currentState == ThiefState.Caught) return;
        currentState = ThiefState.Caught;
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
                ui.CatchThief();
            }

            stolenItems.Clear();
        }
        else
        {
            if (ui != null)
            {
                ui.SetDialogue("The Thief has been caught!");
                ui.CatchThief();
            }
        }

        Debug.Log(name + " caught!");
        Destroy(gameObject);
    }
}

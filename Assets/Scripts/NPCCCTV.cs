using UnityEngine;

public class NPCCCTV : MonoBehaviour
{
    public enum CameraState { Active, Disabled }
    public CameraState currentState = CameraState.Active;

    [Header("VFX")]
    [SerializeField] private GameObject sparkVFXPrefab;
    [SerializeField] private GameObject smokeVFXPrefab;

    private GameObject activeSpark;
    private GameObject activeSmoke;

    [Header("Scan Ability")]
    [SerializeField] private float scanDuration = 4f;
    [SerializeField] private float scanCooldown = 8f;
    [SerializeField] private float scanRadius = 12f;

    private bool isScanning;
    private float scanTimer;
    private float cooldownTimer;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isScanning)
        {
            scanTimer -= Time.deltaTime;

            // Check for thieves in range
            ThiefAI[] thieves = FindObjectsByType<ThiefAI>(FindObjectsSortMode.None);
            foreach (ThiefAI thief in thieves)
            {
                if (thief == null) continue;
                if (Vector3.Distance(transform.position, thief.transform.position) <= scanRadius)
                    thief.OnDetectedByScan();
            }

            if (scanTimer <= 0f)
            {
                isScanning = false;
                cooldownTimer = scanCooldown;
            }
        }
    }

    // ---------------- Disable ----------------
    public void DisableCamera()
    {
        if (currentState == CameraState.Disabled) return;
        currentState = CameraState.Disabled;

        if (sparkVFXPrefab != null)
            activeSpark = Instantiate(sparkVFXPrefab, transform.position, Quaternion.identity, transform);
        if (smokeVFXPrefab != null)
            activeSmoke = Instantiate(smokeVFXPrefab, transform.position, Quaternion.identity, transform);

        isScanning = false;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null) ui.SetDialogue("A CCTV has been hacked!");
    }

    // ---------------- Repair ----------------
    public void RepairCamera()
    {
        if (currentState == CameraState.Active) return;
        currentState = CameraState.Active;

        if (activeSpark != null) Destroy(activeSpark);
        if (activeSmoke != null) Destroy(activeSmoke);
        activeSpark = null;
        activeSmoke = null;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null) ui.SetDialogue("A CCTV has been repaired!");
    }

    // ---------------- Scan ----------------
    public bool ToggleScan()
    {
        if (currentState == CameraState.Disabled) return false;
        if (isScanning) return false;
        if (cooldownTimer > 0f) return false;

        isScanning = true;
        scanTimer = scanDuration;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();
        if (ui != null) ui.SetDialogue("CCTV scan activated!");

        return true;
    }

    // ---------------- Helpers ----------------
    public bool IsActive() => currentState == CameraState.Active;
    public bool IsDisabled() => currentState == CameraState.Disabled;
    public bool IsScanning() => isScanning;
    public bool IsScanOnCooldown() => cooldownTimer > 0f;
}

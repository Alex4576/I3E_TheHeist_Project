using UnityEngine;

public class NPCCCTV : MonoBehaviour
{
    public enum CameraState
    {
        Active,
        Disabled
    }

    public CameraState currentState = CameraState.Active;

    [Header("VFX")]
    [SerializeField] private GameObject sparkVFXPrefab;
    [SerializeField] private GameObject smokeVFXPrefab;

    private GameObject activeSpark;
    private GameObject activeSmoke;

    public void DisableCamera()
    {
        if (currentState == CameraState.Disabled)
            return;

        currentState = CameraState.Disabled;

        Debug.Log(name + " has been hacked!");

        if (sparkVFXPrefab != null)
        {
            activeSpark = Instantiate(
                sparkVFXPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
        }

        if (smokeVFXPrefab != null)
        {
            activeSmoke = Instantiate(
                smokeVFXPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
        }
        isScanning = false;
    }

    public void RepairCamera()
    {
        if (currentState == CameraState.Active)
            return;

        currentState = CameraState.Active;

        Debug.Log(name + " has been repaired!");

        if (activeSpark != null)
            Destroy(activeSpark);

        if (activeSmoke != null)
            Destroy(activeSmoke);

        activeSpark = null;
        activeSmoke = null;
    }

    public bool IsActive()
    {
        return currentState == CameraState.Active;
    }

    public bool IsDisabled()
    {
        return currentState == CameraState.Disabled;
    }
    [Header("Scan Ability")]
    [SerializeField] private float scanDuration = 4f;
    [SerializeField] private float scanCooldown = 8f;
    [SerializeField] private float scanRadius = 12f;

    private bool isScanning;
    private float scanTimer;
    private float cooldownTimer;

    public bool IsScanning() => isScanning;
    public bool IsScanOnCooldown() => cooldownTimer > 0f;

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (isScanning)
        {
            scanTimer -= Time.deltaTime;

            // Check for robbers in range every frame while active
            RobberAI[] robbers = FindObjectsByType<RobberAI>(FindObjectsSortMode.None);
            foreach (RobberAI robber in robbers)
            {
                if (robber == null) continue;

                float dist = Vector3.Distance(transform.position, robber.transform.position);
                if (dist <= scanRadius)
                    robber.OnDetectedByScan();
            }

            if (scanTimer <= 0f)
            {
                isScanning = false;
                cooldownTimer = scanCooldown;
            }
        }
    }

    public bool ToggleScan()
    {
        // Can't scan while the camera itself is hacked, or while on cooldown
        if (currentState == CameraState.Disabled) return false;
        if (isScanning) return false;
        if (cooldownTimer > 0f) return false;

        isScanning = true;
        scanTimer = scanDuration;
        Debug.Log(name + " scan activated!");
        return true;
    }
}
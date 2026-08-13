using UnityEngine;
using TMPro;
using System.Collections;

public class NPCCCTV : MonoBehaviour
{
    public enum CameraState { Active, Disabled }
    public CameraState currentState = CameraState.Active;

    [Header("Interaction")]
    [SerializeField] private UIController uiController;

    [Header("CCTV Camera")]
    [SerializeField] private Camera cctvCamera;

    [Header("CCTV UI")]
    [SerializeField] private GameObject cctvScanUI;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text subText;

    private Camera mainCamera;

    [Header("VFX")]
    [SerializeField] private GameObject sparkVFXPrefab;
    [SerializeField] private GameObject smokeVFXPrefab;

    private GameObject activeSpark;
    private GameObject activeSmoke;

    [Header("Scan Ability")]
    [SerializeField] private float scanDuration = 4f;
    [SerializeField] private float scanCooldown = 8f;
    [SerializeField] private float scanRadius = 12f;
    [SerializeField] private float resultDisplayTime = 1.5f;

    private bool isScanning;
    private bool thiefDetected = false;
    private float scanTimer;
    private float cooldownTimer;

    void Start()
    {
        mainCamera = Camera.main;

        if (cctvCamera != null)
        {
            cctvCamera.enabled = false;
        }

        if (cctvScanUI != null)
        {
            cctvScanUI.SetActive(false);
        }
    }

    void Update()
    {
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isScanning)
        {
            scanTimer -= Time.deltaTime;

            // Check for thieves within the CCTV scan radius
            ThiefAI[] thieves =
                FindObjectsByType<ThiefAI>(FindObjectsSortMode.None);

            foreach (ThiefAI thief in thieves)
            {
                if (thief == null)
                    continue;

                float distance = Vector3.Distance(
                    transform.position,
                    thief.transform.position
                );

                if (distance <= scanRadius)
                {
                    thiefDetected = true;
                    thief.OnDetectedByScan();
                }
            }

            // Scan has finished
            if (scanTimer <= 0f)
            {
                isScanning = false;
                cooldownTimer = scanCooldown;

                StartCoroutine(ShowScanResult());
            }
        }
    }

    // ---------------- Scan Result ----------------

    IEnumerator ShowScanResult()
    {
        // Show the result while CCTV view is still active
        if (statusText != null)
        {
            if (statusText != null)
            {
                statusText.text = "SUSPICIOUS ACTIVITY DETECTED";
            }

            if (subText != null)
            {
                subText.text = "Movement recorded near the Gramophone exhibit.";
            }

            else
            {
                if (statusText != null)
                {
                    statusText.text = "NO SUSPICIOUS ACTIVITY DETECTED";
                }

                if (subText != null)
                {
                    subText.text = "No unusual movement was recorded.";
                }
            }
        }

        // Give the player time to read the result
        yield return new WaitForSeconds(resultDisplayTime);

        // Hide CCTV scan UI
        if (cctvScanUI != null)
        {
            cctvScanUI.SetActive(false);
        }

        // Turn off CCTV camera
        if (cctvCamera != null)
        {
            cctvCamera.enabled = false;
        }

        // Return to player camera
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        if (uiController != null)
        {
            uiController.ShowInteractionUI();
        }
    }

    // ---------------- Disable ----------------

    public void DisableCamera()
    {
        if (currentState == CameraState.Disabled)
            return;

        currentState = CameraState.Disabled;

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

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();

        if (ui != null)
        {
            ui.SetDialogue("A CCTV has been hacked!");
        }
    }

    // ---------------- Repair ----------------

    public void RepairCamera()
    {
        if (currentState == CameraState.Active)
            return;

        currentState = CameraState.Active;

        if (activeSpark != null)
        {
            Destroy(activeSpark);
        }

        if (activeSmoke != null)
        {
            Destroy(activeSmoke);
        }

        activeSpark = null;
        activeSmoke = null;

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();

        if (ui != null)
        {
            ui.SetDialogue("A CCTV has been repaired!");
        }
    }

    // ---------------- Scan ----------------

    public bool ToggleScan()
    {
        // Cannot scan if CCTV is hacked
        if (currentState == CameraState.Disabled)
            return false;

        // Cannot start another scan while scanning
        if (isScanning)
            return false;

        // Cannot scan during cooldown
        if (cooldownTimer > 0f)
            return false;

        isScanning = true;
        scanTimer = scanDuration;
        thiefDetected = false;

        if (uiController != null)
        {
            uiController.HideInteractionUI();
        }

        // Switch from player camera to CCTV camera
        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        if (cctvCamera != null)
        {
            cctvCamera.enabled = true;
        }

        // Show CCTV scan overlay
        if (cctvScanUI != null)
        {
            cctvScanUI.SetActive(true);
        }

        if (statusText != null)
        {
            statusText.text = "SCANNING...";
        }

        if (subText != null)
        {
            subText.text = "Checking CCTV footage...";
        }

        GameplayUI ui = FindFirstObjectByType<GameplayUI>();

        if (ui != null)
        {
            ui.SetDialogue("CCTV scan activated!");
        }

        return true;
    }

    // ---------------- Helpers ----------------

    public bool IsActive() =>
        currentState == CameraState.Active;

    public bool IsDisabled() =>
        currentState == CameraState.Disabled;

    public bool IsScanning() =>
        isScanning;

    public bool IsScanOnCooldown() =>
        cooldownTimer > 0f;
}
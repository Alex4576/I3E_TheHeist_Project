using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// PlayerScript.cs
/// Handles player persistence across scenes, teleportation to SpawnPoints,
/// and interaction with NPC objects (CCTV repair, Hacker catch).
/// Shows UI prompt only for CCTV repair.
/// </summary>
public class PlayerScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] float interactDistance = 3f;          // Max distance for raycast interaction
    [SerializeField] Camera playerCamera;                  // First-person camera
    [SerializeField] TMP_Text interactionText;             // UI prompt text (only for CCTV)
    [SerializeField] UIController uiController;

    void Awake()
    {
        // Prevent duplicate players if one already exists
        if (FindObjectsOfType<PlayerScript>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Keep this Player alive across scene loads
        DontDestroyOnLoad(transform.root.gameObject);
    }

    void OnEnable()
    {
        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid errors
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiController = FindFirstObjectByType<UIController>();
        
        // Reset UI prompt when entering new scene
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        // Move to SpawnPoint if present
        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
        {
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
        }
    }

    void Update()
    {
        HandleInteraction();
    }

    /// <summary>
    /// Handles raycast interaction with CCTV and Hacker NPCs.
    /// CCTV shows prompt, Hacker does not.
    /// </summary>
    void HandleInteraction()
    {    
        if (uiController != null)
        {
            uiController.ClearPrompt();
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // CCTV Repair
            NPCCCTV cctv =
                hit.collider.GetComponentInParent<NPCCCTV>();

            if (cctv != null &&
                cctv.currentState == NPCCCTV.CameraState.Disabled)
            {
                if (uiController != null)
                {
                    uiController.ShowPrompt(
                        "CCTV",
                        "[E] Repair"
                    );
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    cctv.RepairCamera();
                }
            }

            // Hacker Catch
            NPCHacker hacker =
                hit.collider.GetComponentInParent<NPCHacker>();

            if (hacker != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    hacker.Catch();
                }
            }

            // Robber Catch
            RobberAI robber =
                hit.collider.GetComponentInParent<RobberAI>();

            if (robber != null)
            {
                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    robber.Catch();
                }
            }

            // CCTV Scan
            NPCCCTV cctvScan =
                hit.collider.GetComponentInParent<NPCCCTV>();

            if (cctvScan != null &&
                cctvScan.IsActive() &&
                !cctvScan.IsScanOnCooldown())
            {
                if (uiController != null)
                {
                    uiController.ShowPrompt(
                        "CCTV",
                        "[F] Scan for Robbers"
                    );
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    cctvScan.ToggleScan();
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// PlayerScript.cs
/// Handles player persistence across scenes, teleportation to SpawnPoints,
/// and interaction with NPC objects (CCTV repair/scan, Hacker catch, Robber catch).
/// Prompts are shown via UIController if assigned, otherwise fall back to interactionText.
/// </summary>
public class PlayerScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] float interactDistance = 3f;          // Max distance for raycast interaction
    [SerializeField] Camera playerCamera;                   // First-person camera
    [SerializeField] TMP_Text interactionText;              // Fallback UI prompt text
    [SerializeField] UIController uiController;             // Preferred UI prompt system

    void Awake()
    {
        // Prevent duplicate players if one already exists
        if (FindObjectsByType<PlayerScript>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Keep this Player alive across scene loads (persists the whole hierarchy root)
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
        // UIController lives in the scene (not persisted), so re-find it after every load
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
    /// Handles raycast interaction with CCTV (repair + scan), Hacker, and Robber.
    /// </summary>
    void HandleInteraction()
    {
        ClearInteractionPrompt();

        if (playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // CCTV repair prompt
            NPCCCTV cctv = hit.collider.GetComponentInParent<NPCCCTV>();
            if (cctv != null && cctv.currentState == NPCCCTV.CameraState.Disabled)
            {
                ShowInteractionPrompt("CCTV", "[E] Repair");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    cctv.RepairCamera();
                }
            }
            // CCTV scan prompt (only relevant if not disabled — repair takes priority above)
            else if (cctv != null && cctv.IsActive() && !cctv.IsScanOnCooldown())
            {
                ShowInteractionPrompt("CCTV", "[F] Scan for Robbers");

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    cctv.ToggleScan();
                }
            }

            // Hacker catch (no prompt shown)
            NPCHacker hacker = hit.collider.GetComponentInParent<NPCHacker>();
            if (hacker != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    hacker.Catch();
                }
            }

            // Robber catch (no prompt shown)
            RobberAI robber = hit.collider.GetComponentInParent<RobberAI>();
            if (robber != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    robber.Catch();
                }
            }
        }
    }

    /// <summary>
    /// Shows a prompt via UIController if available, otherwise falls back to interactionText.
    /// </summary>
    void ShowInteractionPrompt(string header, string action)
    {
        if (uiController != null)
        {
            uiController.ShowPrompt(header, action);
        }
        else if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = action;
        }
    }

    /// <summary>
    /// Clears whichever prompt system is active.
    /// </summary>
    void ClearInteractionPrompt()
    {
        if (uiController != null)
            uiController.ClearPrompt();

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }
}
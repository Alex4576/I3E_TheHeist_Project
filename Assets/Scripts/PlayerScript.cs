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

    void Awake()
    {
        // Prevent duplicate players if one already exists
        if (FindObjectsOfType<PlayerScript>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Keep this Player alive across scene loads
        DontDestroyOnLoad(gameObject);
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
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            // CCTV repair prompt
            NPCCCTV cctv = hit.collider.GetComponentInParent<NPCCCTV>();
            if (cctv != null && cctv.currentState == NPCCCTV.CameraState.Disabled)
            {
                if (interactionText != null)
                {
                    interactionText.gameObject.SetActive(true);
                    interactionText.text = "Press E to Repair CCTV";
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    cctv.RepairCamera();
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
        }
    }
}

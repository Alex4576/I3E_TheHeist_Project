using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// PlayerScript.cs
/// Handles player persistence across scenes, teleportation to SpawnPoints,
/// and interaction with NPC objects (CCTV repair/scan, Hacker catch, Thief catch, Visitor catch).
/// Prompts are shown via UIController if assigned, otherwise fall back to interactionText.
/// </summary>
public class PlayerScript : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] float interactDistance = 3f;          
    [SerializeField] Camera playerCamera;                   
    [SerializeField] TMP_Text interactionText;              
    [SerializeField] UIController uiController;             

    void Awake()
    {
        if (FindObjectsByType<PlayerScript>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(transform.root.gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        uiController = FindFirstObjectByType<UIController>();

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);

        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
        {
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
        }
    }

    void Update()
    {
        if (UIController.interactionEnabled)
        {
            HandleInteraction();
        }
    }

    void HandleInteraction()
    {
        ClearInteractionPrompt();

        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            NPCCCTV cctv = hit.collider.GetComponentInParent<NPCCCTV>();
            if (cctv != null && cctv.currentState == NPCCCTV.CameraState.Disabled)
            {
                ShowInteractionPrompt("CCTV", "[E] Repair");
                if (Keyboard.current.eKey.wasPressedThisFrame) cctv.RepairCamera();
            }
            else if (cctv != null && cctv.IsActive() && !cctv.IsScanOnCooldown())
            {
                ShowInteractionPrompt("CCTV", "[F] Scan for Thieves");
                if (Keyboard.current.fKey.wasPressedThisFrame) cctv.ToggleScan();
            }

            NPCHacker hacker = hit.collider.GetComponentInParent<NPCHacker>();
            if (hacker != null)
            {
                ShowInteractionPrompt("Suspect", "[E] Catch");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    hacker.Catch();
                }
            }

            ThiefAI thief = hit.collider.GetComponentInParent<ThiefAI>();
            if (thief != null)
            {
                ShowInteractionPrompt("Suspect", "[E] Catch");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    thief.Catch();

                    GameplayUI ui = FindFirstObjectByType<GameplayUI>();

                    if (ui != null)
                    {
                        ui.SetDialogue("SUCCESS! You caught the thief!");
                    }
                }
            }

            NPCVisitor visitor = hit.collider.GetComponentInParent<NPCVisitor>();
            if (visitor != null)
            {
                ShowInteractionPrompt("Suspect", "[E] Catch");

                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    visitor.CatchVisitor();

                    GameplayUI ui = FindFirstObjectByType<GameplayUI>();

                    if (ui != null)
                    {
                        ui.SetDialogue("WRONG PERSON! This visitor is innocent.");
                    }
                }
            }
        }
    }

    void ShowInteractionPrompt(string header, string action)
    {
        if (uiController != null) uiController.ShowPrompt(header, action);
        else if (interactionText != null)
        {
            interactionText.gameObject.SetActive(true);
            interactionText.text = action;
        }
    }

    void ClearInteractionPrompt()
    {
        if (uiController != null) uiController.ClearPrompt();
        if (interactionText != null) interactionText.gameObject.SetActive(false);
    }
}

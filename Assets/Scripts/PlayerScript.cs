using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{
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
        // Look for a GameObject named "SpawnPoint" in the new scene
        GameObject spawn = GameObject.Find("SpawnPoint");
        if (spawn != null)
        {
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
        }
    }

    void Update()
    {
        // Your player movement / input code goes here
    }
}

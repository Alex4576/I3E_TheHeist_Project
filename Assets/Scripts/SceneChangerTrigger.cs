using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerTrigger : MonoBehaviour
{
    [Header("Index of scene to change to")]
    [SerializeField] private int sceneToGoIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToGoIndex);
        }
    }
}

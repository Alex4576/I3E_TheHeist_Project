using UnityEngine;

/// <summary>
/// Controls a museum door that opens when the player enters the trigger zone
/// and closes when the player exits.
/// </summary>
public class MuseumDoorTrigger : MonoBehaviour
{
    /// <summary>
    /// Animator controlling the door animations.
    /// </summary>
    public Animator anim;

    /// <summary>
    /// Called when the player enters the trigger zone.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Near", true); // Open door
        }
    }

    /// <summary>
    /// Called when the player exits the trigger zone.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            anim.SetBool("Near", false); // Close door
        }
    }
}

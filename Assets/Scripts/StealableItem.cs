using UnityEngine;

public class StealableItem : MonoBehaviour
{
    [SerializeField] private bool destroyOnSteal = true;

    public bool IsStolen { get; private set; }

    public void Steal()
    {
        if (IsStolen) return;

        IsStolen = true;
        Debug.Log(name + " was stolen!");

        if (destroyOnSteal)
            gameObject.SetActive(false); // or Destroy(gameObject);
    }

    public void Restore()
    {
        if (!IsStolen) return;

        IsStolen = false;
        gameObject.SetActive(true);
        Debug.Log(name + " has been restored to its display.");
    }
}
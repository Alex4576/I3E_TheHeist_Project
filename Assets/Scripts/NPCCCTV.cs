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
}
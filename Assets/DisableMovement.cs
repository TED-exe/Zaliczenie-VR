using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class DisableMovement : MonoBehaviour
{
    public ContinuousTurnProvider moveProvider;

    void Start()
    {
        if (moveProvider != null)
        {
            moveProvider.enabled = false;
        }
    }
}

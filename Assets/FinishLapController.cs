using System;
using Unity.VisualScripting;
using UnityEngine;

public class FinishLapController : MonoBehaviour
{
    [SerializeField] private GameManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.gameObject.TryGetComponent(out CarPhysics physics))
        {
            manager.AddLoop(physics);
        }
    }
}
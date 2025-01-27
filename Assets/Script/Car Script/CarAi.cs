using System;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(CarPhysics))]
public class CarAi : MonoBehaviour
{
    [SerializeField] private Transform targetPositionTransform;
    private Vector3 targetPosition;
    private CarPhysics carPhysics;

    private void Awake()
    {
        carPhysics = GetComponent<CarPhysics>();
    }

    private void Update()
    {
        SetTargetPosition(targetPositionTransform.position);

        float forwardAmount = 0;
        float sideAmount = 0;

        float reachedTargetPosition = 5f;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget > reachedTargetPosition)
        {
            Vector3 dirToMovePosition = (targetPosition - transform.position).normalized;
            float dot = Vector3.Dot(transform.forward, dirToMovePosition);

            if (dot > 0)
                forwardAmount = 1f;
            else if (dot < 0)
            {
                float reverseDistance = 25f;
                if (distanceToTarget > reverseDistance)
                {
                    forwardAmount = 1f;
                }
                else
                {
                    forwardAmount = -1f;
                }
            }

            float angleDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);

            if (angleDir > 0)
                sideAmount = 1f;
            else
                sideAmount = -1f;
        }
        else
        {
            if (carPhysics.GetSpeed() > 15f)
            {
                forwardAmount = -1f;
            }
            else
            {
                forwardAmount = 0;
            }
            sideAmount = 0;
            
        }

        carPhysics.GetInput(sideAmount, forwardAmount);
    }

    public void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}
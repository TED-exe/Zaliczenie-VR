using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting;

[RequireComponent(typeof(CarPhysics))]
public class CarAi : MonoBehaviour
{
    [SerializeField] private Transform targetPositionTransform;
    private Vector3 targetPosition;
    private CarPhysics carPhysics;


    [Header("PAth/Waypoints")] public CarPath carPath;
    [SerializeField] private float maxWaypointSize;
    [SerializeField] private float minWaypointSize;
    [SerializeField] private float currWaypointSize = 0.5f;


    public void StartAI()
    {
        carPhysics = GetComponent<CarPhysics>();
        SetTargetPosition(carPath.GetCurrWaypointPosition());
    }

    private float lowSpeedThreshold = 10f; // Próg niskiej prędkości
    private float lowSpeedDuration = 3f; // Czas, przez jaki prędkość musi być niska, aby zresetować pozycję
    private float lowSpeedTimer = 0f; // Licznik czasu niskiej prędkości

    private void Update()
    {
        if (!carPhysics.canRide)
            return;

        var distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        BaseControll(distanceToTarget);
        CheckDistance(distanceToTarget);
        CheckLowSpeed();
    }

    private void CheckLowSpeed()
    {
        if (carPhysics.GetSpeed() < lowSpeedThreshold)
        {
            lowSpeedTimer += Time.deltaTime;
            if (lowSpeedTimer >= lowSpeedDuration)
            {
                ResetCarPosition();
                lowSpeedTimer = 0f; // Reset timera
            }
        }
        else
        {
            lowSpeedTimer = 0f; // Reset timera, jeśli prędkość wróciła do normy
        }
    }

    private void ResetCarPosition()
    {
        Vector3 lastWaypoint = carPath.GetPreviousWaypointPosition();
        Vector3 nextWaypoint = carPath.GetCurrWaypointPosition();

        // Przesunięcie w górę o 2 jednostki, by uniknąć kolizji
        transform.position = lastWaypoint + Vector3.up * 2f;

        // Obrót w kierunku następnego waypointa
        Vector3 directionToNext = (nextWaypoint - lastWaypoint).normalized;
        transform.rotation = Quaternion.LookRotation(directionToNext, Vector3.up);

        carPhysics.ResetVelocity(); // Opcjonalnie, jeśli CarPhysics ma taką metodę
    }


    private void CheckDistance(float distanceToTarget)
    {
        if (distanceToTarget < currWaypointSize)
        {
            carPath.NextWaypoint();
            SetTargetPosition(carPath.GetCurrWaypointPosition());
        }
    }

    private void BaseControll(float distanceToTarget)
    {
        float forwardAmount = 0;
        float sideAmount = 0;

        float reachedTargetPosition = 1f;
        float slowDownDistance = 2f; // Od jakiej odległości zaczyna zwalniać
        float minAccelerationFactor = 0.4f; // Minimalna wartość przyspieszenia
        float maxAngleSlowDown = 45f; // Kąt, od którego zaczyna zwalniać na zakrętach
        float hardBrakeAngleThreshold = 70f; // Kąt, od którego włącza ręczny

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
            float devidedAngleDir = angleDir / carPhysics.GetWheeleRotationLimit();
            devidedAngleDir = Mathf.Clamp(devidedAngleDir, -1f, 1f);
            sideAmount = devidedAngleDir;

            // **1. Zmniejszanie prędkości im bliżej celu**
            if (distanceToTarget < slowDownDistance)
            {
                float slowDownFactor = Mathf.Clamp01(distanceToTarget / slowDownDistance);
                forwardAmount *= Mathf.Lerp(minAccelerationFactor, 1f, slowDownFactor);
            }

            // **2. Zmniejszanie prędkości przy dużym skręcie**
            float angleFactor = Mathf.Clamp01(Mathf.Abs(angleDir) / maxAngleSlowDown);
            forwardAmount *= (1f - angleFactor);

            // **3. Hamowanie ręcznym przy bardzo dużym kącie skrętu**
            if (Mathf.Abs(angleDir) > hardBrakeAngleThreshold)
            {
                carPhysics.isBreaking = true;
            }
            else
            {
                carPhysics.isBreaking = false;
            }
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


    private void SetTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private float DistanceToPoint(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (carPath != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(carPath.GetCurrWaypointPosition(), currWaypointSize);
        }
    }
}
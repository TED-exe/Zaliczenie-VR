using System;
using UnityEngine;

public class PlayerWaypointSystem : MonoBehaviour
{
    [SerializeField] private CarPath path;
    
    [SerializeField] private float currWaypointSize;
    [HideInInspector] public int currAllWaypointsindex = 0;
    
    [SerializeField] private GameObject waypointIndicatorPrefab;
    private GameObject currentWaypointIndicator;
    
    private bool isReversing = false;


    public void SetUpPath(CarPath path)
    {
        this.path = path;
        this.path.SetUpPath(path);
        SpawnWaypointIndicator();
    }

    private void FixedUpdate()
    {
        CheckDistance(Vector3.Distance(transform.position, path.pathWaypoints[currAllWaypointsindex].position));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCarPosition();
        }
    }

    public Vector3 GetCurrWaypointPosition()
    {
        return path.pathWaypoints[currAllWaypointsindex].position;
    }

    public Vector3 GetNextWaypointPosition()
    {
        if (path.pathWaypoints == null || path.pathWaypoints.Count == 0)
            return Vector3.zero;

        int nextIndex = GetNextWaypointIndex();
        return path.pathWaypoints[nextIndex].position;
    }

    private int GetNextWaypointIndex()
    {
        if (path.LoopPath)
            return (currAllWaypointsindex + 1) % path.pathWaypoints.Count;

        if (!isReversing)
        {
            if (currAllWaypointsindex + 1 < path.pathWaypoints.Count)
                return currAllWaypointsindex + 1;
            return currAllWaypointsindex - 1;
        }
        else
        {
            if (currAllWaypointsindex - 1 >= 0)
                return currAllWaypointsindex - 1;
            return currAllWaypointsindex + 1;
        }
    }
    public Vector3 GetPreviousWaypointPosition()
    {
        if (path.pathWaypoints == null || path.pathWaypoints.Count == 0)
            return Vector3.zero;

        int prevIndex = currAllWaypointsindex;

        if (path.LoopPath)
        {
            prevIndex = (currAllWaypointsindex - 1 + path.pathWaypoints.Count) % path.pathWaypoints.Count;
        }
        else
        {
            if (!isReversing)
            {
                if (currAllWaypointsindex - 1 >= 0)
                    prevIndex = currAllWaypointsindex - 1;
                else
                    prevIndex = currAllWaypointsindex + 1; // Jeśli jesteśmy na początku, zwróć następny waypoint
            }
            else
            {
                if (currAllWaypointsindex + 1 < path.pathWaypoints.Count)
                    prevIndex = currAllWaypointsindex + 1;
                else
                    prevIndex = currAllWaypointsindex - 1; // Jeśli jesteśmy na końcu, zwróć poprzedni waypoint
            }
        }

        return path.pathWaypoints[prevIndex].position;
    }

    public void NextWaypoint()
    {
        if (path.pathWaypoints == null || path.pathWaypoints.Count == 0)
            return;

        if (path.LoopPath)
        {
            currAllWaypointsindex = (currAllWaypointsindex + 1) % path.pathWaypoints.Count;
        }
        else
        {
            if (!isReversing)
            {
                currAllWaypointsindex++;
                if (currAllWaypointsindex >= path.pathWaypoints.Count)
                {
                    currAllWaypointsindex = path.pathWaypoints.Count - 2;
                    isReversing = true;
                }
            }
            else
            {
                currAllWaypointsindex--;
                if (currAllWaypointsindex < 0)
                {
                    currAllWaypointsindex = 1;
                    isReversing = false;
                }
            }
        }
        
        UpdateWaypointIndicator();
    }
    private void CheckDistance(float distanceToTarget)
    {
        if (distanceToTarget < currWaypointSize)
        {
            NextWaypoint();
        }
    }
    
    private void ResetCarPosition()
    {
        Vector3 lastWaypoint = GetPreviousWaypointPosition();
        Vector3 nextWaypoint = GetCurrWaypointPosition();

        transform.position = lastWaypoint + Vector3.up * 2f;
    
        Vector3 directionToNext = (nextWaypoint - lastWaypoint).normalized;
        transform.rotation = Quaternion.LookRotation(directionToNext, Vector3.up);

        transform.GetComponent<CarPhysics>().ResetVelocity();
    }
    
    private void SpawnWaypointIndicator()
    {
        if (waypointIndicatorPrefab != null)
        {
            if (currentWaypointIndicator != null)
            {
                Destroy(currentWaypointIndicator);
            }

            Vector3 nextWaypointPos = GetCurrWaypointPosition();
            currentWaypointIndicator = Instantiate(waypointIndicatorPrefab, nextWaypointPos, Quaternion.identity);
        }
    }

    private void UpdateWaypointIndicator()
    {
        if (currentWaypointIndicator != null)
        {
            Destroy(currentWaypointIndicator);
        }

        SpawnWaypointIndicator();
    }

}
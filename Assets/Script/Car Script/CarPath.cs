using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CarPath : MonoBehaviour
{
    public List<Transform> pathWaypoints = new List<Transform>();
    public List<Transform> AllPathWaypoints = new List<Transform>();

    [HideInInspector] public Transform WaypointsHolder = null;
    [HideInInspector] public Transform AllWaypointsPathHolder = null;

    [HideInInspector] public int PathWaypointsCount = 0;
    public int AllPathWaypointsCount = 0;

    public bool LoopPath = false;
    [Min(1)] public int segmentsCounts = 20;

    [Header("gizmos")]
    [SerializeField] private float gizmoSphereSize = 0.5f;

    /*    private void OnValidate()
        {
            DrawQuadraticBezierPath();
        }*/
    public void CreatePath()
    {
        AllWaypointsPathHolder = new GameObject("AllPoints").transform;
        AllWaypointsPathHolder.transform.parent = transform;

        WaypointsHolder = new GameObject("Waypoints").transform;
        WaypointsHolder.transform.parent = transform;
    }
    public void AddPathWaypoint(Vector3 waypointPosition)
    {
        GameObject waypoint = new GameObject($"waypoint {PathWaypointsCount}");
        waypoint.transform.position = waypointPosition;
        waypoint.transform.parent = WaypointsHolder;
        pathWaypoints.Add(waypoint.transform);

        // Dodanie punktu kontrolnego (dziecka) do każdego waypointa
        GameObject controlPoint = new GameObject($"controlPoint {PathWaypointsCount}");
        controlPoint.transform.position = waypointPosition; // Początkowa pozycja punktu kontrolnego
        controlPoint.transform.parent = waypoint.transform;

        if (PathWaypointsCount != 0)
        {
            Vector3 pointA = controlPoint.transform.position;
            Vector3 pointB = pathWaypoints[PathWaypointsCount - 1].position;
            float t = 0.5f;
            pathWaypoints[PathWaypointsCount - 1].Find($"controlPoint {PathWaypointsCount - 1}").transform.position = Vector3.Lerp(pointA, pointB, t);
        }

        PathWaypointsCount++;

    }
    public void DrawQuadraticBezierPath()
    {
        if (AllPathWaypoints == null)
            return;
        if (pathWaypoints.Count == 0)
            return;

        if (AllPathWaypoints.Count != 0)
        {
            foreach (var pathWaypoint in AllPathWaypoints)
            {
                DestroyImmediate(pathWaypoint.gameObject);
            }
            AllPathWaypoints.Clear();
            AllPathWaypointsCount = 0;
        }

        for (var i = 0; i < pathWaypoints.Count - 1; i++)
        {
            Transform waypointA = pathWaypoints[i];
            Transform waypointB = pathWaypoints[i + 1];

            // Pobierz punkt kontrolny z waypointa A
            Transform controlPoint = waypointA.Find($"controlPoint {i}");

            // Rysuj krzywą Béziera między waypointem A, punktem kontrolnym i waypointem B
            if (controlPoint == null)
                continue;

            int dynamicSegmentCount = CalculateDynamicSegmentCount(waypointA.position, controlPoint.position, waypointB.position);

            if (i == 0)
            {
                GameObject waypoint = new GameObject($"point {AllPathWaypointsCount}");
                waypoint.transform.position = pathWaypoints[i].position;
                waypoint.transform.parent = AllWaypointsPathHolder;
                AllPathWaypoints.Add(waypoint.transform);
                AllPathWaypointsCount++;
            }

            for (int z = 1; z <= dynamicSegmentCount; z++)
            {
                float t = (float)z / dynamicSegmentCount;
                Vector3 pointOnCurve = CalculateQuadraticBezier(waypointA.position, controlPoint.position, waypointB.position, t);

                GameObject waypoint = new GameObject($"point {AllPathWaypointsCount}");
                waypoint.transform.position = pointOnCurve;
                waypoint.transform.parent = AllWaypointsPathHolder;
                AllPathWaypoints.Add(waypoint.transform);
                AllPathWaypointsCount++;
            }
        }
    }
    private int CalculateDynamicSegmentCount(Vector3 positionA, Vector3 controller, Vector3 positionB)
    {
        // Długość bezpośredniej linii między pozycją A a B
        float straightDistance = Vector3.Distance(positionA, positionB);

        // Długość krzywej, którą liczymy jako suma dwóch odcinków: A -> control i control -> B
        float curveDistance = Vector3.Distance(positionA, controller) + Vector3.Distance(controller, positionB);

        // Oblicz odchylenie: im większa różnica, tym większa wykrzywienie
        float curvature = Mathf.Abs(curveDistance - straightDistance);

        // Na podstawie stopnia wykrzywienia dobierz liczbę segmentów
        // Zakładamy, że większa różnica wymaga większej liczby segmentów (max = segmentsCounts)
        int calculatedSegments = Mathf.CeilToInt(Mathf.Lerp(1, segmentsCounts, curvature / straightDistance));

        // Upewnij się, że zwracana liczba segmentów jest co najmniej 1
        return Mathf.Max(1, calculatedSegments);
    }
    private void DrawQuadraticBezierGizmo(Vector3 positionA, Vector3 controller, Vector3 positionB)
    {
        Vector3 previousPoint = positionA;

        int dynamicSegmentCount = CalculateDynamicSegmentCount(positionA, controller, positionB);

        for (int i = 1; i <= dynamicSegmentCount; i++)
        {
            float t = (float)i / dynamicSegmentCount;
            Vector3 pointOnCurve = CalculateQuadraticBezier(positionA, controller, positionB, t);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(previousPoint, pointOnCurve);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(pointOnCurve, gizmoSphereSize * 0.2f);

            previousPoint = pointOnCurve;
        }
    }
    private Vector3 CalculateQuadraticBezier(Vector3 positionA, Vector3 controller, Vector3 positionB, float t)
    {
        Vector3 p0 = Vector3.Lerp(positionA, controller, t);
        Vector3 p1 = Vector3.Lerp(controller, positionB, t);
        return Vector3.Lerp(p0, p1, t);
    }
    private void OnDrawGizmos()
    {
        // Rysowanie sfer wokół waypointów
        for (var i = 0; i < pathWaypoints.Count; i++)
        {
            if (i == 0)
                Gizmos.color = Color.red;
            else if (i == pathWaypoints.Count - 1)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.blue;

            Gizmos.DrawSphere(pathWaypoints[i].position, gizmoSphereSize);

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(pathWaypoints[i].GetChild(0).transform.position, gizmoSphereSize * 0.3f);
        }

        // Rysowanie krzywej lub linii
        Gizmos.color = Color.white;
        for (var i = 0; i < pathWaypoints.Count - 1; i++)
        {
            Transform waypointA = pathWaypoints[i];
            Transform waypointB = pathWaypoints[i + 1];

            // Pobierz punkt kontrolny z waypointa A
            Transform controlPoint = waypointA.Find($"controlPoint {i}");

            // Rysuj krzywą Béziera między waypointem A, punktem kontrolnym i waypointem B
            if (controlPoint != null)
            {
                DrawQuadraticBezierGizmo(waypointA.position, controlPoint.position, waypointB.position);
            }
            else
            {
                // Jeśli nie ma punktu kontrolnego, rysuj linię prostą
                Gizmos.DrawLine(waypointA.position, waypointB.position);
            }
        }

        // Jeśli pętla ścieżki, połącz ostatni punkt z pierwszym
        if (LoopPath && pathWaypoints.Count > 1)
        {
            Transform waypointA = pathWaypoints[pathWaypoints.Count - 1];
            Transform waypointB = pathWaypoints[0];
            Transform controlPoint = waypointA.Find($"controlPoint {pathWaypoints.Count - 1}");

            if (controlPoint != null)
            {
                DrawQuadraticBezierGizmo(waypointA.position, controlPoint.position, waypointB.position);
            }
            else
            {
                Gizmos.DrawLine(waypointA.position, waypointB.position);
            }
        }
    }
}
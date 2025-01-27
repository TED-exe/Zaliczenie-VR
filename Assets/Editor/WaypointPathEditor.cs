using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CarPath))]
public class WaypointPathEditor : Editor
{
    private CarPath waypointPath;

    private void OnEnable()
    {
        waypointPath = (CarPath)target;
    }

    private void OnSceneGUI()
    {
        // Sprawdź, czy waypointy lub ich punkty kontrolne zostały przesunięte
        bool hasChanged = false;

        for (int i = 0; i < waypointPath.pathWaypoints.Count; i++)
        {
            // Obsługa waypointów (używamy kolorów i innego kształtu)
            EditorGUI.BeginChangeCheck();
            Handles.color = Color.green; // Ustaw kolor waypointu na zielony
            Vector3 newWaypointPos = Handles.PositionHandle(waypointPath.pathWaypoints[i].position, Quaternion.identity);
            Handles.SphereHandleCap(0, waypointPath.pathWaypoints[i].position, Quaternion.identity, 0.5f, EventType.Repaint); // Rysowanie waypointu jako sfera

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypointPath.pathWaypoints[i], "Move Waypoint");
                waypointPath.pathWaypoints[i].position = newWaypointPos;
                hasChanged = true;
            }

            // Obsługa punktów kontrolnych (inny kolor i kształt)
            Transform controlPoint = waypointPath.pathWaypoints[i].GetChild(0); // Pobierz punkt kontrolny
            if (controlPoint != null)
            {
                EditorGUI.BeginChangeCheck();
                Handles.color = Color.red; // Ustaw kolor punktu kontrolnego na czerwony
                Vector3 newControlPointPos = Handles.PositionHandle(controlPoint.position, Quaternion.identity);
                Handles.CubeHandleCap(0, controlPoint.position, Quaternion.identity, 0.3f, EventType.Repaint); // Rysowanie punktu kontrolnego jako kwadrat

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controlPoint, "Move Control Point");
                    controlPoint.position = newControlPointPos;
                    hasChanged = true;
                }
            }
        }

        // Jeśli wykryto zmiany, odśwież ścieżkę
        if (hasChanged)
        {
            waypointPath.DrawQuadraticBezierPath();
        }
    }
}

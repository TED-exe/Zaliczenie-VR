using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class HideChildrenInHierarchy
{
    static HideChildrenInHierarchy()
    {
        // Callback do rysowania w hierarchii
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCallback;
    }

    static void HierarchyItemCallback(int instanceID, Rect selectionRect)
    {
        // Pobierz obiekt w hierarchii na podstawie ID
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        // Sprawdź, czy obiekt istnieje i czy ma nazwę, którą chcemy ukryć
        if (obj != null && ((obj.name == "AllPoints" || obj.transform.parent?.name == "AllPoints") ||
                            (obj.name == "Waypoints" || obj.transform.parent?.name == "Waypoints")))
        {
            // Zablokuj jego rysowanie, używając Event.current.Use()
            Event.current.Use(); // Zablokuj zdarzenie

            // Ustal prostokąt na podstawie wybranej pozycji, ale nie rysuj obiektu
            return; // Przerywamy, aby nie rysować tego obiektu
        }

    }
}

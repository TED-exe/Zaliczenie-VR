using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    void Awake()
    {
        Collider hiddenCol = GetComponent<Collider>();
        Collider[] parentCols = GetComponentsInParent<Collider>();
        foreach (Collider col in parentCols)
        {
            if (col != hiddenCol)
                Physics.IgnoreCollision(hiddenCol, col);
        }
    }
}

using UnityEditor;
using UnityEngine;

public class SelectParentShortcut
{
    [MenuItem("Tools/Select Parent #p")]
    private static void Main()
    {
        if (Selection.activeObject == null)
        {
            Debug.LogWarning("SelectParent: Nothing selected.");
            return;
        }

        if (Selection.activeGameObject.transform.parent == null)
        {
            Debug.LogWarning("SelectParent: No parent found");
            return;
        }

        Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
    }
}

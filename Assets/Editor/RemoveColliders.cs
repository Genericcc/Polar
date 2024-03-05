using _Scripts.Data;

using UnityEditor;

using UnityEngine;

namespace Editor
{
    [CustomEditor (typeof(ComponentDestroyer))]
    [CanEditMultipleObjects]
    public class RemoveColliders : UnityEditor.Editor 
    {
        public override void OnInspectorGUI () 
        {
            DrawDefaultInspector();
            var myScript = (ComponentDestroyer)target;
            if (GUILayout.Button("Destroy Folder Colliders"))
            {
                myScript.DestroyCollidersInFolder();
            }
            if (GUILayout.Button("Destroy Colliders"))
            {
                myScript.DestroyColliders();
            }
        }
    }
}
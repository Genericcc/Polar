using System.Collections.Generic;
using System.IO;

using UnityEditor;

using UnityEngine;

namespace _Scripts.Data
{
    public class ComponentDestroyer : MonoBehaviour 
    {
        public GameObject myObject;
        private BoxCollider[] _childColliders;

        public string pathToFolderToCleanFromColliders; 

        public void DestroyColliders(GameObject toDestroy = null)
        {
            var toProcess = toDestroy != null ? toDestroy : myObject;
            
            var children = new List<Transform>();
            GetAllChildren(toProcess.transform, children);
            children.Add(toProcess.transform);

            foreach (var child in children)
            {
                Debug.Log("Checking child: " + child.name);

                var childCollider = child.GetComponent<Collider>();

                Debug.Log("Collider: " + childCollider);

                if (childCollider != null)
                {
                    #if UNITY_EDITOR
                    DestroyImmediate(childCollider, true);
                    //PrefabUtility.ApplyRemovedComponent(myObject, childCollider, InteractionMode.UserAction);
                    #else
                    Destroy(childCollider);
                    #endif
                }
                else
                {
                    Debug.LogWarning("Collider not found on child: " + child.name);
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GetAllChildren(Transform parent, List<Transform> transforms)
        {
            foreach (Transform child in parent)
            {
                transforms.Add(child);
                GetAllChildren(child, transforms);
            }
        }
        
        public void DestroyCollidersInFolder() 
        {
            var fullPath = "Assets/" + pathToFolderToCleanFromColliders;
            
            if (Directory.Exists(fullPath)) 
            {
                var prefabPaths = Directory.GetFiles(fullPath, "*.prefab", SearchOption.AllDirectories);
            
                foreach (var prefabPath in prefabPaths) 
                {
                    var relativePath = prefabPath.Replace(Application.dataPath, "Assets");
                    var prefab = AssetDatabase.LoadAssetAtPath(relativePath, typeof(GameObject)) as GameObject;

                    if (prefab == null)
                    {
                        continue;
                    }
                    
                    DestroyColliders(prefab);

                    // var prefabTransforms = prefab.GetComponentsInChildren<Transform>(true);
                    //
                    // DestroyColliders()
                    // foreach (var prefabChild in prefabTransforms) 
                    // {
                    //     _childColliders = prefabChild.GetComponentsInChildren<Collider>();
                    //     foreach (var collider in _childColliders) 
                    //     {
                    //         DestroyImmediate(collider);
                    //     }
                    // }
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else 
            {
                Debug.LogError("Folder not found: " + fullPath);
            }
        }
    }
}
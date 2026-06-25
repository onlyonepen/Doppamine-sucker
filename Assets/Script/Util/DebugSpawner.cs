using UnityEngine;
using VInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugSpawner : MonoBehaviour
{
    [Header("Debug Spawner Settings")]
    public GameObject objectToSpawn;
    
    [Space(10)]
    [Tooltip("How far in front of the camera the object should spawn.")]
    public float spawnForwardOffset = 3f;

    [Button("Spawn at Scene Camera")]
    public void SpawnObjectAtSceneCamera()
    {
        if (objectToSpawn == null)
        {
            Debug.LogWarning("DebugSpawner: No object selected to spawn!");
            return;
        }

        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

#if UNITY_EDITOR
        // 1. Try to grab the exact position of the Editor's Scene View camera
        if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
        {
            Transform sceneCamTransform = SceneView.lastActiveSceneView.camera.transform;
            
            // Push it slightly forward so it doesn't spawn perfectly inside the camera lens
            spawnPosition = sceneCamTransform.position + (sceneCamTransform.forward * spawnForwardOffset);
            spawnRotation = sceneCamTransform.rotation;
        }
        else
#endif
        {
            // 2. Fallback to the Main Camera if the Scene View isn't active (e.g., testing in a built build)
            if (Camera.main != null)
            {
                spawnPosition = Camera.main.transform.position + (Camera.main.transform.forward * spawnForwardOffset);
                spawnRotation = Camera.main.transform.rotation;
            }
            else
            {
                Debug.LogWarning("DebugSpawner: Could not find a Scene Camera or Main Camera!");
                return;
            }
        }

        // 3. Spawn the object
        GameObject spawnedObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

#if UNITY_EDITOR
        // 4. Register this action so you can press Ctrl+Z to undo the spawn!
        Undo.RegisterCreatedObjectUndo(spawnedObj, "Spawned Debug Object");
#endif

        Debug.Log($"<color=cyan>[DebugSpawner]</color> Spawned {objectToSpawn.name} successfully.");
    }
}
using UnityEngine;

public class LockonObject : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The object to look at. If left empty, it will try to find the 'Player' tag on Start.")]
    public Transform target;

    [Header("Axis Locks")]
    [Tooltip("Lock the X axis (Pitch - looking up/down)")]
    public bool lockX = false;
    
    [Tooltip("Lock the Y axis (Yaw - turning left/right)")]
    public bool lockY = false;
    
    [Tooltip("Lock the Z axis (Roll - tilting side to side)")]
    public bool lockZ = true; // Z is often locked for standard 3D look-ats to prevent weird tilting

    void Start()
    {
        if(target == null) Debug.LogError("No Target");
    }

    void LateUpdate()
    {
        // Don't do anything if we don't have a target
        if (target == null) return;

        // Calculate the direction from this object to the target
        Vector3 direction = target.position - transform.position;

        // Prevent Unity from throwing a warning if the objects are in the exact same spot
        if (direction != Vector3.zero)
        {
            // Calculate what the rotation SHOULD be to look directly at the target
            Quaternion desiredRotation = Quaternion.LookRotation(direction);
            Vector3 desiredEuler = desiredRotation.eulerAngles;

            // Get our current rotation so we can keep the locked values
            Vector3 currentEuler = transform.eulerAngles;

            // Decide which axes to use based on the lock toggles
            float finalX = lockX ? currentEuler.x : desiredEuler.x;
            float finalY = lockY ? currentEuler.y : desiredEuler.y;
            float finalZ = lockZ ? currentEuler.z : desiredEuler.z;

            // Apply the final rotation
            transform.eulerAngles = new Vector3(finalX, finalY, finalZ);
        }
    }
}

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
        if(target == null) target = GlobalReference.Instance.player.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(direction);
            Vector3 desiredEuler = desiredRotation.eulerAngles;

            Vector3 currentEuler = transform.eulerAngles;

            float finalX = lockX ? 0 : desiredEuler.x;
            float finalY = lockY ? 0 : desiredEuler.y;
            float finalZ = lockZ ? 0 : desiredEuler.z;

            transform.localEulerAngles = new Vector3(finalX, finalY, finalZ);
        }
    }
}

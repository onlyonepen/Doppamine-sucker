using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackArea : MonoBehaviour
{
    [SerializeField] private LayerMask AttackableLayer;
    public List<GameObject> InArea = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        if((1 << other.gameObject.layer & AttackableLayer) != 0)
        {
            InArea.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (InArea.Contains(other.gameObject))
        {
            InArea.Remove(other.gameObject);
        }
    }
}

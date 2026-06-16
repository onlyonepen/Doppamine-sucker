using System;
using UnityEngine;

public class ObtainGrapple : MonoBehaviour
{
    public LayerMask PlayerLayer;
    public GameObject[] ToSwitchActive;
    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & PlayerLayer) != 0)
        {
            GlobalReference.Instance.player.canGrapple = true;
            gameObject.SetActive(false);
            foreach (GameObject obj in ToSwitchActive)
            {
                obj.SetActive(true);
            }
        }
    }
}

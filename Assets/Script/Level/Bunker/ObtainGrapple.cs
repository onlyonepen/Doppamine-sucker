using System;
using UnityEngine;

public class ObtainGrapple : MonoBehaviour
{
    public LayerMask PlayerLayer;
    public GameObject[] ToSwitchActive;
    public OSTController ostController;
    private void Start()
    {
        if (GameValue.ObtainedGrapple)
        {
            gameObject.SetActive(false);
            foreach (GameObject obj in ToSwitchActive)
            {
                obj.SetActive(true);
            }
            ostController.PlayOST();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((1 << other.gameObject.layer & PlayerLayer) != 0)
        {
            GlobalReference.Instance.player.canGrapple = true;
            GameValue.ObtainedGrapple = true;
            foreach (GameObject obj in ToSwitchActive)
            {
                obj.SetActive(true);
            }
            ostController.PlayOST();
            gameObject.SetActive(false);
        }
    }
}

using System;
using UnityEngine;

public class AmimTest : MonoBehaviour
{
    public Animator GB_anim;
    public Animator R_Arm;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GB_anim.SetTrigger("ChargeUp");
            R_Arm.SetTrigger("ChargeUp");
        }
        if (Input.GetMouseButtonUp(0))
        {
            GB_anim.SetTrigger("Slash");
            R_Arm.SetTrigger("Slash");
        }
    }
}

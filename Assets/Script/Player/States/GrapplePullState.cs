using DG.Tweening;
using System.Collections;
using UnityEngine;

public class GrapplePullState : PlayerState
{
    //bool isExtraGravOn;
    private float airFloatForce = 7f;

    private SpringJoint joint;

    GameObject grappledObj;
    Vector3 initialObjPos;

    //bool grappleEnemy = false;
    
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        grappledObj = manager.RUD.GrappledObject;
        initialObjPos = grappledObj.transform.position;

        //if (grappledObj.transform.parent.TryGetComponent<GroundedEnemySM>( out var component))
        //{
        //    component.Grappled();
        //    grappleEnemy = true;
        //   sm = component;
        //}
        
        manager.rb.linearVelocity = manager.rb.linearVelocity * 0.2f;
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.RUD.GrapplePoint = grappledObj.transform.position;
        manager.GuntipPointToGrapple();

        float elapsed = Time.time - stateEnterTime;
        float percent = Mathf.Clamp01(elapsed / .5f);
        DrawTuggingRope(percent);
        UpdateObjectPos(percent);
        if (percent == 1)
        {
            manager.ChangeState(manager.BaseState);
        }
        
        manager.rb.AddForce(Vector3.up * airFloatForce, ForceMode.Acceleration);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;
        grappledObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        
        //if(grappleEnemy) sm.TakeDamage();
        manager.rb.linearVelocity = manager.rb.linearVelocity * 5f;
    }

    private void UpdateObjectPos(float percent)
    {
        Vector3 origin = manager.Cam.transform.position + (manager.Cam.transform.forward * 1);
        Vector3 target = initialObjPos;
        Vector3 pos = Vector3.Lerp(target, origin, percent);
        grappledObj.transform.position = pos;
    }

    public int segmentCount = 30;
    public float maxTugAmplitude = 1.5f; // How far the rope bends at max tug

    void DrawTuggingRope(float percent)
    {
        if (percent == 1)
        {
            manager.GrappleLr.positionCount = 2;
            manager.GrappleLr.SetPosition(0, manager.Guntip.position);
            manager.GrappleLr.SetPosition(1, manager.RUD.GrappledObject.transform.position);
            return;
        }

        Vector3 origin = manager.Guntip.position;

        Vector3 target = manager.RUD.GrappledObject.transform.position;

        manager.GrappleLr.positionCount = segmentCount;

        float animationLift = Mathf.Sin(percent * Mathf.PI * 2) * maxTugAmplitude;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);

            Vector3 pos = Vector3.Lerp(origin, target, t);

            float curveShape = Mathf.Sin(t * Mathf.PI);

            pos += manager.transform.up * (curveShape * animationLift);

            manager.GrappleLr.SetPosition(i, pos);
        }
    }
}

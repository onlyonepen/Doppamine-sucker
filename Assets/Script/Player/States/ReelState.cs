using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ReelState : PlayerState
{
    //bool isExtraGravOn;

    private SpringJoint joint;

    GameObject grappledObj;
    Vector3 initialObjPos;

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        grappledObj = manager.RUD.GrappledObject;
        initialObjPos = grappledObj.transform.position;

        //joint = grappledObj.gameObject.AddComponent<SpringJoint>();
        //joint.autoConfigureConnectedAnchor = false;
        //joint.maxDistance = 0;
        //joint.minDistance = 0;
        //joint.spring = 10;
        //joint.damper = 3;


        //manager.GrappleLr.enabled = true;

    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.RUD.GrapplePoint = grappledObj.transform.position;
        manager.GuntipPointToGrapple();

        //joint.connectedAnchor = manager.transform.position;

        float elapsed = Time.time - stateEnterTime;
        float percent = Mathf.Clamp01(elapsed / .5f);
        DrawTuggingRope(percent);
        UpdateObjectPos(percent);
        if (percent == 1)
        {
            manager.ChangeState(manager.pullRopeBackState);
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;
        grappledObj.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        MonoBehaviour.Destroy(grappledObj);
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

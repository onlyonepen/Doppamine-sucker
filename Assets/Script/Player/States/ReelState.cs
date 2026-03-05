using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ReelState : PlayerState
{
    bool isExtraGravOn;
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.playerCanMove = false;
        manager.PBM.FloatingCapsuleActive = false;
        if (manager.PBM.hasFallingExtraGrav)
        {
            isExtraGravOn = true;
            manager.PBM.hasFallingExtraGrav = false;
        }
        manager.rb.linearVelocity = Vector3.zero;


        manager.GrappleLr.enabled = true;

        manager.StartCoroutine(ReelEnemy());
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GuntipPointToGrapple();

        float elapsed = Time.time - stateEnterTime;
        float percent = Mathf.Clamp01(elapsed / .5f);
        DrawTuggingRope(percent);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;

        if (isExtraGravOn) manager.PBM.hasFallingExtraGrav = true;
        manager.PBM.FloatingCapsuleActive = true;
    }

    private void drawRope()
    {
        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
        manager.GrappleLr.SetPosition(1, manager.RUD.GrappledObject.transform.position);
    }

    IEnumerator ReelEnemy()
    {
        //manager.RUD.GrappledObject.transform.DOMove(manager.Guntip.position, 0.5f);
        GameObject grappledObj = manager.RUD.GrappledObject;
        JumpToGrapplePos(grappledObj, manager.transform.position);
        JumpToGrapplePos(manager.gameObject, grappledObj.transform.position);

        yield return new WaitForSeconds(0.5f);
        manager.ChangeState(manager.BaseState);
    }

    private void JumpToGrapplePos(GameObject jumpObj, Vector3 targetPos)
    {
        Vector3 lowestPoint = new Vector3(manager.transform.position.x, manager.transform.position.y,
                                manager.transform.position.z);

        float grapplePointRelativeYPos = manager.RUD.GrapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + manager.OvershootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = manager.OvershootYAxis;

        jumpObj.GetComponent<Rigidbody>().linearVelocity = calculateJumpVelocity(jumpObj.transform.position, targetPos, 1) * 0.7f;
    }

    private Vector3 calculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        float optimizedHeight = Mathf.Max(displacementY + 0.1f, trajectoryHeight);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * optimizedHeight);

        float timeUp = Mathf.Sqrt(-2 * optimizedHeight / gravity);
        float timeDown = Mathf.Sqrt(2 * (displacementY - optimizedHeight) / gravity);

        Vector3 velocityXZ = displacementXZ / (timeUp + timeDown);

        return velocityXZ + velocityY;
    }

    public int segmentCount = 30;
    public float maxTugAmplitude = 1.5f; // How far the rope bends at max tug

    void DrawTuggingRope(float percent)
    {
        if (percent > 1)
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

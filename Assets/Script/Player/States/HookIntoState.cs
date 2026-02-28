using DG.Tweening;
using UnityEngine;

public class HookIntoState : PlayerState
{
    float enterTimeStamp;

    bool isExtraGravOn = false;
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        enterTimeStamp = Time.time;

        manager.PBM.playerCanMove = false;
        manager.PBM.FloatingCapsuleActive = false;
        if (manager.PBM.hasFallingExtraGrav)
        {
            isExtraGravOn = true;
            manager.PBM.hasFallingExtraGrav = false;
        }

        manager.GrappleLr.SetPosition(1, manager.RUD.GrapplePoint);

        manager.rb.linearVelocity = Vector3.zero;

        //float dist = Vector3.Distance(manager.transform.position, manager.RUD.GrapplePoint);
        //float pullDur = manager.Speed * dist / manager.GrappleMaxDistance * 2;
        //if (pullDur < 0.5) pullDur = 0.5f;
        //float overShootY = manager.OvershootYAxis * dist / manager.GrappleMaxDistance * 2;
        //seq = manager.transform.DOJump(manager.RUD.GrapplePoint, manager.OvershootYAxis, 1, pullDur);

        JumpToGrapplePos();
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        AirControl();

        if(Time.time - enterTimeStamp > 0.5f)
        {
            manager.GrapplePrediction();
            if (Input.GetMouseButtonDown(1)) manager.ChangeState(manager.ThrowGrappleState);
        }

        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        if (isExtraGravOn) manager.PBM.hasFallingExtraGrav = true;
        manager.PBM.FloatingCapsuleActive = true;
    }

    public override void OnStateTriggerEnter(Collider collider)
    {
        base.OnStateTriggerEnter(collider);
        manager.ChangeState(manager.BaseState);
    }


    private void AirControl()
    {
        float vertical = Input.GetAxis("Vertical") * manager.AirControlFwdForce;
        float horizontal = Input.GetAxis("Horizontal") * manager.AirControlHorizontalForce;

        Vector3 TotalForceDir = (manager.Cam.transform.forward * vertical) + (manager.Cam.transform.right * horizontal);
        manager.rb.AddForce(TotalForceDir * Time.deltaTime, ForceMode.Force);
    }

    private void JumpToGrapplePos()
    {
        Vector3 lowestPoint = new Vector3(manager.transform.position.x, manager.transform.position.y,
                                manager.transform.position.z);

        float grapplePointRelativeYPos = manager.RUD.GrapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + manager.OvershootYAxis;


        if (grapplePointRelativeYPos < 0) highestPointOnArc = manager.OvershootYAxis;

        manager.rb.linearVelocity = calculateJumpVelocity(manager.transform.position, manager.RUD.GrapplePoint, highestPointOnArc);
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

}

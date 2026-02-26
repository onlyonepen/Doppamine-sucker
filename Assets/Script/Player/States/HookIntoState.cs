using UnityEngine;

public class HookIntoState : PlayerState
{
    float enterTimeStamp;
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        enterTimeStamp = Time.time;

        manager.PBM.playerCanMove = false;
        manager.PBM.FloatingCapsuleActive = false;

        manager.GrappleLr.SetPosition(1, manager.RUD.GrapplePoint);

        manager.rb.linearVelocity = Vector3.zero;

        JumpToGrapplePos();
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GrappleLr.SetPosition(0, manager.Guntip.position);

        AirControl();

        if(Time.time - enterTimeStamp > 0.2f)
        {
            manager.ChangeState(manager.BaseState);
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.PBM.FloatingCapsuleActive = true;
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

        manager.rb.linearVelocity = calculateJumpVelocity(manager.transform.position, manager.RUD.GrapplePoint, highestPointOnArc) * 1.2f;
    }

    private Vector3 calculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        // Ensure trajectoryHeight is always higher than the displacement to avoid NaN
        float optimizedHeight = Mathf.Max(displacementY + 0.1f, trajectoryHeight);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * optimizedHeight);

        float timeUp = Mathf.Sqrt(-2 * optimizedHeight / gravity);
        float timeDown = Mathf.Sqrt(2 * (displacementY - optimizedHeight) / gravity);

        Vector3 velocityXZ = displacementXZ / (timeUp + timeDown);

        return velocityXZ + velocityY;
    }
}

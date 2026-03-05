using DG.Tweening;
using UnityEngine;

public class WallRunningState : PlayerState
{
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    bool wallLeft;
    bool wallRight;

    bool upwardsRunning;
    bool downwardsRunning;

    Tween rotateTween;

    Vector3 orient;
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);
        manager.GuntipDefault();
        manager.rb.useGravity = false;
        manager.predictionPoint.gameObject.SetActive(false);
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        WallRunCheck();
        if (wallLeft) rotateTween = manager.SideRotateJoint.DOLocalRotate(new Vector3(0, 0, -15f), 0.5f);
        if (wallRight) rotateTween = manager.SideRotateJoint.DOLocalRotate(new Vector3(0, 0, 15f), 0.5f);

        WallRunMovement();

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyUp(KeyCode.W)) manager.ChangeState(manager.BaseState);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.rb.useGravity = true;

        rotateTween.Kill();
        manager.SideRotateJoint.DOLocalRotate(new Vector3(0, 0, 0f), 0.5f);

        WallJump();
    }
    
    private void WallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 jumpDir = (wallNormal.normalized * manager.WallJumpForce) + (manager.transform.up.normalized * manager.PBM.jumpPower);

        manager.rb.AddForce(jumpDir, ForceMode.Impulse);
    }

    private void WallRunMovement()
    {
        orient = new Vector3(manager.Cam.transform.forward.x, 0 , manager.Cam.transform.forward.z).normalized;

        upwardsRunning = Input.GetKey(KeyCode.LeftShift);
        downwardsRunning = Input.GetKey(KeyCode.LeftControl);

        Rigidbody rb = manager.rb;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, manager.transform.up);

        if ((orient - wallForward).magnitude > (orient - -wallForward).magnitude)
            wallForward = -wallForward;



        Vector3 targetVelocity = manager.transform.TransformDirection(orient) * manager.WallRunMaxSpeed;

        float currentVelocity = rb.linearVelocity.magnitude;
        float currentMoveSpeed = currentVelocity;

        if (currentVelocity <= manager.WallRunMaxSpeed)
        {
            currentMoveSpeed += manager.WallRunAccel;
        }
        else currentMoveSpeed = manager.WallRunMaxSpeed;

            rb.AddForce(currentMoveSpeed * wallForward.normalized * Time.deltaTime, ForceMode.VelocityChange);

        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, manager.WallClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -manager.WallClimbSpeed, rb.linearVelocity.z);
    }

    private void WallRunCheck()
    {
        wallRight = Physics.Raycast(manager.transform.position, manager.Cam.transform.right, out rightWallHit, manager.WallCheckDistance, manager.WallRunable);
        wallLeft = Physics.Raycast(manager.transform.position, -manager.Cam.transform.right, out leftWallHit, manager.WallCheckDistance, manager.WallRunable);
        bool grounded = Physics.Raycast(manager.transform.position, Vector3.down, manager.GroundCheckDistance, LayerMask.GetMask("Ground"));

        if(grounded || (!wallLeft && !wallRight))
        {
            manager.ChangeState(manager.BaseState);
        }
    }
}

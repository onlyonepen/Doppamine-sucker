using DG.Tweening;
using UnityEngine;

public class PlayerBaseState : PlayerState
{
    private float distanceToFeet;


    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.GuntipDefault();

        manager.PBM.playerCanMove = true;

        distanceToFeet = Vector3.Distance(manager.transform.position, manager.feetTrans.position);

        manager.SideRotateJoint.DOLocalRotate(new Vector3(0, 0, 0f), 0.5f);
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GrapplePrediction();

        WallRunCheck();
        MantleCheck();
        SlideCheck();
        if (Input.GetMouseButtonDown(1)) manager.ChangeState(manager.ThrowGrappleState);
    }

    private void WallRunCheck()
    {
        bool wallRightUpper = Physics.Raycast(manager.transform.position, manager.Cam.transform.right, manager.WallCheckDistance, manager.TerrainLayer);
        bool wallRightLower = Physics.Raycast(manager.transform.position - Vector3.up * manager.PlayerHeightOffset, manager.Cam.transform.right, manager.WallCheckDistance, manager.TerrainLayer);
        bool wallLeftUpper = Physics.Raycast(manager.transform.position, -manager.Cam.transform.right, manager.WallCheckDistance, manager.TerrainLayer);
        bool wallLeftLower = Physics.Raycast(manager.transform.position - Vector3.up * manager.PlayerHeightOffset , -manager.Cam.transform.right, manager.WallCheckDistance, manager.TerrainLayer);

        bool wallRight = wallRightLower && wallRightUpper;
        bool wallLeft = wallLeftLower && wallLeftUpper;

        bool grounded = Physics.Raycast(manager.transform.position, Vector3.down, manager.GroundCheckDistance, LayerMask.GetMask("Ground"));

        if ((wallRight || wallLeft) && !manager.PBM.isGrounded && Input.GetKey(KeyCode.W))
        {
            manager.ChangeState(manager.WallRunState);
        }
    }

    private void MantleCheck()
    {
        if (manager.PBM.isGrounded) return ;
        if (!Input.GetKey(KeyCode.W)) return;

        float forwardReach = 1.5f;
        Vector3 origin = manager.transform.position;
        Vector3 forward = manager.transform.forward;

        if (Physics.Raycast(origin + (Vector3.up * 0.5f), forward, out RaycastHit wallHit, forwardReach, manager.TerrainLayer))
        {
            bool headBlocked = Physics.Raycast(origin + (Vector3.up * manager.PlayerHeightOffset), Vector3.up, manager.PlayerHeightOffset, manager.TerrainLayer);
            if (headBlocked) return;

            Vector3 ledgeCheckOrigin = wallHit.point + (forward * 0.2f) + (Vector3.up * manager.PlayerHeightOffset);

            if (Physics.Raycast(ledgeCheckOrigin, Vector3.down, out RaycastHit ledgeHit, manager.PlayerHeightOffset, manager.TerrainLayer))
            {
                Vector3 targetPos = ledgeHit.point;

                bool spaceOccupied = Physics.CheckCapsule(targetPos + Vector3.up * 0.5f, targetPos + Vector3.up * (manager.PlayerHeightOffset - 0.5f), 0.4f, manager.TerrainLayer);

                if (!spaceOccupied)
                {
                    manager.RUD.MantlePoint = targetPos + Vector3.up * distanceToFeet;
                    manager.ChangeState(manager.MantleState);
                }
            }
        }
    }
    private void SlideCheck()
    {
        if (!manager.PBM.isGrounded) return;
        if (!Input.GetKey(KeyCode.LeftControl)) return;

        Vector3 playerfwd = manager.transform.forward;

        Vector3 groundNormal = manager.GroundNormal();
        float angelBetween = Vector3.Angle(playerfwd, groundNormal);
        bool canSlideWithSlope = angelBetween < 85 && angelBetween > 50;
        bool canSlideFlatGround = angelBetween >= 85 && manager.rb.linearVelocity.magnitude > manager.PBM.walkSpeed;
        Debug.Log("groun = " + groundNormal);
        Debug.Log(angelBetween);
        if (canSlideWithSlope || canSlideFlatGround)
        {
            manager.ChangeState(manager.SlideState);
        }
    }
}

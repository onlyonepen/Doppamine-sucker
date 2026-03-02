using UnityEngine;

public class PlayerBaseState : PlayerState
{

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.playerCanMove = true;
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GrapplePrediction();

        WallRunCheck();
        if (Input.GetMouseButtonDown(1)) manager.ChangeState(manager.ThrowGrappleState);
    }

    private void WallRunCheck()
    {
        bool wallRight = Physics.Raycast(manager.transform.position, manager.Cam.transform.right, manager.WallCheckDistance, manager.WallRunable);
        bool wallLeft = Physics.Raycast(manager.transform.position, -manager.Cam.transform.right,manager.WallCheckDistance, manager.WallRunable);
        bool grounded = Physics.Raycast(manager.transform.position, Vector3.down, manager.GroundCheckDistance, LayerMask.GetMask("Ground"));

        if ((wallRight || wallLeft) && !grounded && Input.GetKey(KeyCode.W))
        {
            manager.ChangeState(manager.WallRunState);
        }
    }
}

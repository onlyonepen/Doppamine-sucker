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
        if (Input.GetMouseButtonDown(1)) manager.ChangeState(manager.ThrowGrappleState);
    }
}

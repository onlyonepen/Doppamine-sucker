using UnityEngine;

public class ThrowGrappleState : PlayerState
{
    private float stateEnterTime;
    RaycastHit grappleCastHit;

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.enabled = true;

        stateEnterTime = Time.time;

        grappleCastHit = manager.GrapplePrediction();
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        if(Time.time - stateEnterTime > manager.GrappleTravelTime)
        {
            if (!Input.GetMouseButton(1) || grappleCastHit.collider == null)
            {
                manager.ChangeState(manager.BaseState);
                return;
            }

            manager.RUD.GrapplePoint = grappleCastHit.point;
            manager.RUD.GrappledObject = grappleCastHit.collider.gameObject;

            if ((1 << manager.RUD.GrappledObject.layer & manager.Swingable) != 0) { manager.ChangeState(manager.SwingState); }
            else if ((1 << manager.RUD.GrappledObject.layer & manager.Pullable) != 0) { manager.ChangeState(manager.ReelState); }
            else manager.ChangeState(manager.BaseState);
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        manager.predictionPoint.gameObject.SetActive(false);
    }
}
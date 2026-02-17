using UnityEngine;

public class ThrowGrappleState : PlayerState
{
    private float stateEnterTime;

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.enabled = true;

        stateEnterTime = Time.time;
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        if(Time.time - stateEnterTime > manager.GrappleTravelTime)
        {
            RaycastHit grappleCastHit = grappleRaycast();
            manager.RUD.GrapplePoint = grappleCastHit.point;
            manager.RUD.GrappledObject = grappleCastHit.collider.gameObject;

            //if cast hit enemy -> pull

            //if cast hit ground -> swing
        }
    }



    private RaycastHit grappleRaycast()
    {
        Vector3 screenCenterPoint = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray grappleRay = manager.Cam.ScreenPointToRay(screenCenterPoint);

        RaycastHit grappleHit;
        Physics.Raycast(grappleRay, out grappleHit, manager.GrappleMaxDistance, manager.Grappable);

        return grappleHit;
    }
}
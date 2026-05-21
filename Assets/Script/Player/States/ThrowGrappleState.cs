using DG.Tweening;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ThrowGrappleState : PlayerState
{
    RaycastHit grappleCastHit;

    private int segmentCount = 40;
    private float waveSize = 1f;
    private float waveFrequency = 2f;
    private float waveSpeed = 15f;

    private Vector3 InitialHitPos;
    
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.enabled = true;

        grappleCastHit = manager.GrapplePrediction();
        manager.predictionPoint.gameObject.SetActive(false);

        manager.GrappleLr.enabled = true;
        manager.GrappleLr.positionCount = segmentCount;

        if(grappleCastHit.collider != null)
        {
            manager.RUD.GrappledObject = grappleCastHit.collider.gameObject;
            manager.RUD.GrapplePoint = grappleCastHit.point;
        }
        else
        {
            manager.RUD.GrapplePoint = manager.Guntip.position + manager.Cam.transform.forward * manager.GrappleMaxDistance;
        }
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GuntipPointToGrapple();

        //if (grappleCastHit.collider != null)
        //{
        //    manager.RUD.GrapplePoint = grappleCastHit.point;
        //}

        float elapsed = Time.time - stateEnterTime;
        float percent = Mathf.Clamp01(elapsed / manager.GrappleTravelTime);

        DrawAnimatedRope(percent);

        if (Time.time - stateEnterTime > manager.GrappleTravelTime)
        { 
            //if(grappleCastHit.collider != null)
            //{
            //    manager.RUD.GrappledObject = grappleCastHit.collider.gameObject;
            //}
            if (!Input.GetMouseButton(1) || grappleCastHit.collider == null)
            {
                //manager.RUD.GrapplePoint = manager.Guntip.position + manager.Cam.transform.forward * manager.GrappleMaxDistance;
                manager.ChangeState(manager.pullRopeBackState);
                return;
            }

            if ((1 << manager.RUD.GrappledObject.layer & manager.Swingable) != 0) { manager.ChangeState(manager.SwingState); }
            else if ((1 << manager.RUD.GrappledObject.layer & manager.Pullable) != 0) { manager.ChangeState(manager.GrapplePullState); }
            else if((1 << manager.RUD.GrappledObject.layer & manager.HeavyPull) != 0) { manager.ChangeState(manager.GrapplePullinState); }
            else manager.ChangeState(manager.pullRopeBackState);
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        manager.GrappleLr.enabled = false;
        manager.GrappleLr.positionCount = 2;
    }


    private void DrawAnimatedRope(float percent)
    {
        Vector3 origin = manager.Guntip.position;
        Vector3 targetGoal = manager.RUD.GrapplePoint;
        if((1 << manager.RUD.GrappledObject.layer & GlobalReference.Instance.EnemyLayer) != 0) targetGoal = manager.RUD.GrappledObject.transform.position;
        Vector3 currentTipPos = Vector3.Lerp(origin, targetGoal, percent);

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 pos = Vector3.Lerp(origin, currentTipPos, t);

            if (percent < 0.99f)
            {
                float taper = Mathf.Sin(t * Mathf.PI);

                float wave = Mathf.Sin(t * Mathf.PI * waveFrequency + (Time.time * waveSpeed))
                             * waveSize
                             * (1 - percent)
                             * taper;

                pos += manager.transform.up * wave;
                pos += manager.transform.right * (wave * 0.5f);
            }
            manager.GrappleLr.SetPosition(i, pos);
        }
    }
}
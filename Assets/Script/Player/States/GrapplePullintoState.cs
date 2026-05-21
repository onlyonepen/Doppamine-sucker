using UnityEngine;

public class GrapplePullintoState : PlayerState
{
    private float initialEnemyDistance;
    private Vector3 initialPlayerPosition;
    private float expectedDuration;

    public float pullSpeed = 40f; 

    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);
        
        manager.PBM.playerCanMove = false;
        manager.PBM.FloatingCapsuleActive = false;
        if (manager.PBM.hasFallingExtraGrav)
        {
            manager.PBM.hasFallingExtraGrav = false;
        }

        manager.GrappleLr.enabled = true;
        
        initialPlayerPosition = manager.transform.position;
        initialEnemyDistance = Vector3.Distance(initialPlayerPosition, manager.RUD.GrappledObject.transform.position);
        
        expectedDuration = initialEnemyDistance / pullSpeed;

        manager.rb.linearVelocity = Vector3.zero; 
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.GuntipPointToGrapple();

        float elapsed = Time.time - stateEnterTime;
        float percent = expectedDuration > 0 ? Mathf.Clamp01(elapsed / expectedDuration) : 1f;
        
        PullIn(percent);
        
        if (percent >= 0.95f)
        {
            if (manager.RUD.GrappledObject.TryGetComponent<IDamagable>(out var component)) 
            {
                component.TakeDamage();
            }
            manager.ChangeState(manager.BaseState);
        }

        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
        manager.GrappleLr.SetPosition(1, manager.RUD.GrappledObject.transform.position);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        manager.GrappleLr.enabled = false;
        manager.GrappleLr.positionCount = 2;

        manager.PBM.FloatingCapsuleActive = true;

        Vector3 pullDirection = (manager.RUD.GrappledObject.transform.position - initialPlayerPosition).normalized;
        
        manager.rb.linearVelocity = pullDirection * pullSpeed * 0.8f;
    }

    private void PullIn(float percent)
    {
        Vector3 origin = initialPlayerPosition;
        Vector3 targetGoal = manager.RUD.GrappledObject.transform.position;
        
        Vector3 newPos = Vector3.Lerp(origin, targetGoal, percent);
        manager.rb.MovePosition(newPos); 
    }
}
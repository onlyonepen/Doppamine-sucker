using DG.Tweening;
using System.Collections;
using Script.Enemy;
using UnityEngine;

public class GrapplePullState : PlayerState
{
    private float airFloatForce = 7f;
    private SpringJoint joint;

    GameObject grappledObj;
    private Rigidbody grappledObjRb;
    Vector3 initialObjPos;

    bool grappleEnemy = false;
    private BaseEmemy enemy;

    private Vector3 initialVelocity;
    
    private float visualOffset = 0.25f; 
    
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        grappledObj = manager.RUD.GrappledObject;
        initialObjPos = grappledObj.transform.position;
        
        grappledObj.TryGetComponent<Rigidbody>(out grappledObjRb);

        if (grappledObj.TryGetComponent<BaseEmemy>( out var component))
        {
            component.GetPull();
            grappleEnemy = true;
            enemy = component;
        }
        
        initialVelocity = manager.rb.linearVelocity;
        manager.rb.linearVelocity *= 0.2f;

        // Ensure the line renderer is ready for a simple 2-point straight line
        manager.GrappleLr.enabled = true;
        manager.GrappleLr.positionCount = 2;
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();

        manager.RUD.GrapplePoint = grappledObj.transform.position;
        manager.GuntipPointToGrapple();

        float currentOffset = manager.GrappleEnemyOffset;

        // Check if object is an enemy and dynamically calculate offset from collider size
        if (((1 << grappledObj.layer) & GlobalReference.Instance.EnemyLayer) != 0)
        {
            if (grappledObj.TryGetComponent<Collider>(out Collider col))
            {
                currentOffset = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
            }
        }

        Vector3 dirToPlayer = (manager.transform.position - grappledObj.transform.position).normalized;
        Vector3 visualPos = grappledObj.transform.position + (dirToPlayer * currentOffset);
        
        manager.GrappleHand.position = visualPos;

        // Draw simple straight rope
        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
        manager.GrappleLr.SetPosition(1, visualPos);

        float elapsed = Time.time - stateEnterTime;
        float percent = Mathf.Clamp01(elapsed / .5f);
        
        UpdateObjectPos(percent);
        
        if (percent == 1)
        {
            manager.ChangeState(manager.BaseState);
        }
        
        manager.rb.AddForce(Vector3.up * airFloatForce, ForceMode.Acceleration);
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;
        
        if (grappledObjRb != null) 
        {
            grappledObjRb.linearVelocity = Vector3.zero;
        }
        
        if(grappleEnemy) enemy.SplitDeath();
        manager.rb.linearVelocity = initialVelocity;
    }

    private void UpdateObjectPos(float percent)
    {
        Vector3 origin = manager.Cam.transform.position + (manager.Cam.transform.forward * 1);
        Vector3 target = initialObjPos;
        Vector3 pos = Vector3.Lerp(target, origin, percent);
        
        if (grappledObjRb != null)
        {
            grappledObjRb.MovePosition(pos);
        }
        else
        {
            grappledObj.transform.position = pos;
        }
        
        float distance = Vector3.Distance(pos, manager.transform.position);
        if (distance <= 6) 
        {
            manager.ChangeState(manager.BaseState);
        }
    }
}
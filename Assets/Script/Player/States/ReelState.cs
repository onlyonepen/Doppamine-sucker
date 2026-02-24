using DG.Tweening;
using System.Collections;
using UnityEngine;

public class ReelState : PlayerState
{
    public override void OnStateEnter(PlayerStateManager gamestateManager)
    {
        base.OnStateEnter(gamestateManager);

        manager.PBM.playerCanMove = false;
        manager.rb.constraints = RigidbodyConstraints.FreezeAll;

        manager.GrappleLr.enabled = true;

        manager.StartCoroutine(ReelEnemy());
    }

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        drawRope();
    }

    public override void OnStateExit()
    {
        base.OnStateExit();
        manager.GrappleLr.enabled = false;

        manager.rb.linearVelocity = Vector3.zero;
        manager.rb.constraints = RigidbodyConstraints.None;
        manager.rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void drawRope()
    {
        manager.GrappleLr.SetPosition(0, manager.Guntip.position);
        manager.GrappleLr.SetPosition(1, manager.RUD.GrappledObject.transform.position);
    }

    IEnumerator ReelEnemy()
    {
        manager.RUD.GrappledObject.transform.DOMove(manager.Guntip.position, 1.5f);
        yield return new WaitForSeconds(1.5f);
        manager.ChangeState(manager.BaseState);
    }
}

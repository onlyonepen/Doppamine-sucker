using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LeapEnemyGround : EnemyState
{
    public override void OnStateEnter(EnemySM gamestateManager)
    {
        base.OnStateEnter(gamestateManager);
        manager.currentSpot = (manager.currentSpot + 1) % manager.AllSpot.Count;
        manager.enemyObj.transform.DOJump(manager.AllSpot[manager.currentSpot].transform.position, 1, 1, 2);
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        manager.WaitToChangeState("Idle", 2, stateEnterTime);
    }
}
using UnityEngine;

public class AggroEnemyGround : EnemyState
{
    private float toLeapDistance = 12f;

    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        GameObject player = GlobalReference.Instance.player.gameObject;

        float playerDist = Vector3.Distance(player.transform.position, manager.enemyObj.transform.position);
        if (playerDist < toLeapDistance) manager.ChangeState("Leap"); 
        else if (playerDist > manager.SightRange) manager.ChangeState("Idle");

        if (Time.time - stateEnterTime > manager.AttackFrequentcy)
        {
            manager.ChangeState("Attack");
        }
    }
}
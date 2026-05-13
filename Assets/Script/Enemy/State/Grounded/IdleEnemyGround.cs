using UnityEngine;

public class IdleEnemyGround : EnemyState
{
    // ReSharper disable Unity.PerformanceAnalysis
    public override void OnStateUpdate()
    {
        base.OnStateUpdate();
        PlayerCheck();
    }

    private void PlayerCheck()
    {
        GameObject player = GlobalReference.Instance.player.gameObject;

        float playerDist = Vector3.Distance(player.transform.position, manager.enemyObj.transform.position);
        if (playerDist > manager.SightRange) return;

        Vector3 toPlayer = player.transform.position - manager.enemyObj.transform.position;
        bool seePLayer = Physics.Raycast(manager.enemyObj.transform.position, toPlayer, playerDist + 5f, GlobalReference.Instance.playerLayer);

        if (seePLayer)
        {
            manager.ChangeState("Aggro");
        }
    }
}

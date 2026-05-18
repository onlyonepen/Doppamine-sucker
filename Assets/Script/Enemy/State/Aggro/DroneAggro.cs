using UnityEngine;

namespace Script.Enemy.State.Aggro
{
    public class DroneAggro : EnemyBaseState
    {
        public override void OnStateUpdate()
        {
            playerCheck();
        }

        private void playerCheck()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;

            float playerDist = Vector3.Distance(player.transform.position, Enemy.transform.position);

            Vector3 toPlayer = player.transform.position - Enemy.transform.position;
            bool seePLayer = Physics.Raycast(Enemy.transform.position, toPlayer, playerDist + 5f, GlobalReference.Instance.playerLayer);

            if (!seePLayer || playerDist > Enemy.Stat.DetectionRange)
            {
                StateFactory factory = new StateFactory();
                Enemy.ChangeState(factory.CreateState(EnemyStatesEnum.Idle));
            }
        }
    }
}
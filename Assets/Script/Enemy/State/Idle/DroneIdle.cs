using UnityEngine;

namespace Script.Enemy.State.Idle
{
    public class DroneIdle : EnemyBaseState
    {
        public DroneIdle(BaseRangedEmemy enemy)
        {
            Enemy = enemy;
        }
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            Enemy.rb.linearVelocity = Vector3.zero;
        }

        public override void OnStateUpdate()
        {
            playerCheck();
        }

        private void playerCheck()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;

            float playerDist = Vector3.Distance(player.transform.position, Enemy.transform.position);
            if (playerDist > Enemy.Stat.DetectionRange) return;

            Vector3 toPlayer = player.transform.position - Enemy.transform.position;
            bool seePLayer = Physics.Raycast(Enemy.transform.position, toPlayer, playerDist + 5f, GlobalReference.Instance.playerLayer);

            
            if (seePLayer)
            {
                Enemy.ChangeState(Enemy.stateFactory.CreateAggroState(Enemy));
            }
        }
    }
}
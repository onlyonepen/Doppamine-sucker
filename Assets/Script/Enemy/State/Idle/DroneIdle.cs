using UnityEngine;

namespace Script.Enemy.State.Idle
{
    public class DroneIdle : EnemyBaseState
    {
        private float detectionValue;
        private float toAggroTime = 3f;
        
        public DroneIdle(BaseEmemy enemy)
        {
            Enemy = enemy;
        }
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            Enemy.rb.linearVelocity = Vector3.zero;
            Enemy.rb.constraints =  RigidbodyConstraints.FreezePosition;
        }

        public override void OnStateUpdate()
        {
            playerCheck();
        }

        public override void OnStateExit()
        {
            Enemy.rb.constraints =  RigidbodyConstraints.None;
            Enemy.rb.constraints =  RigidbodyConstraints.FreezeRotation;
        }

        private void playerCheck()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;

            float playerDist = Vector3.Distance(player.transform.position, Enemy.transform.position);
            if (playerDist > Enemy.Stat.DetectionRange) return;

            Vector3 toPlayer = player.transform.position - Enemy.transform.position;
            bool seePLayer = Physics.Raycast(Enemy.transform.position, toPlayer, playerDist + 5f, GlobalReference.Instance.playerLayer);

            
            if (seePLayer) detectionValue += Time.deltaTime;
            else detectionValue -= Time.deltaTime;
            
            if (seePLayer && (detectionValue > toAggroTime) || playerDist < 15) Enemy.ChangeState(Enemy.stateFactory.CreateAggroState(Enemy));
        }
    }
}
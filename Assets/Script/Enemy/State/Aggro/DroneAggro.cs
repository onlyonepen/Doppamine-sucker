using UnityEngine;

namespace Script.Enemy.State.Aggro
{
    public class DroneAggro : EnemyBaseState
    {
        private float strafeDirection = 1f; // 1 = Clockwise, -1 = Counter-Clockwise
        private float directionChangeTimer;
        
        // Adjust these depending on your drone's physical size
        private float droneRadius = 0.5f; 
        private float collisionCheckDistance = 2.5f; 

        public override void OnStateUpdate()
        {
            playerCheck();
            StrafeAroundPlayer();
        }

        private void playerCheck()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;

            float playerDist = Vector3.Distance(player.transform.position, Enemy.transform.position);
            Vector3 toPlayer = player.transform.position - Enemy.transform.position;
            
            bool seePLayer = Physics.Raycast(Enemy.transform.position, toPlayer.normalized, playerDist + 5f, GlobalReference.Instance.playerLayer);

            if(Time.time - StateEnterTime > Enemy.Stat.AttackFrequentcy) Enemy.ChangeState(StateFactory.Instance.CreateState(EnemyStatesEnum.Attack));
            if (!seePLayer || playerDist > Enemy.Stat.DetectionRange)
            {
                Enemy.ChangeState(StateFactory.Instance.CreateState(EnemyStatesEnum.Idle));
            }
        }

        private void StrafeAroundPlayer()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;
            //Rigidbody rb = Enemy.GetComponent<Rigidbody>();
            
            if (Enemy.rb == null) return;

            Vector3 dronePos = Enemy.transform.position;
            Vector3 playerPos = player.transform.position;

            Vector3 toPlayer = playerPos - dronePos;
            Vector3 directionToPlayer = toPlayer.normalized;
            float currentDist = toPlayer.magnitude;

            Vector3 strafeLeftRight = Vector3.Cross(directionToPlayer, Vector3.up).normalized * strafeDirection;
            
            LayerMask obstacleMask = ~GlobalReference.Instance.playerLayer; 

            if (Physics.SphereCast(dronePos, droneRadius, strafeLeftRight, out RaycastHit hit, collisionCheckDistance, obstacleMask))
            {
                strafeDirection *= -1f;
                strafeLeftRight = Vector3.Cross(directionToPlayer, Vector3.up).normalized * strafeDirection;
                directionChangeTimer = Random.Range(2f, 5f);
            }
            else
            {
                directionChangeTimer -= Time.deltaTime;
                if (directionChangeTimer <= 0)
                {
                    strafeDirection = Random.value > 0.5f ? 1f : -1f;
                    directionChangeTimer = Random.Range(2f, 5f);
                }
            }

            float targetDistance = Enemy.Stat.DetectionRange * 0.6f;
            Vector3 radialCorrection = Vector3.zero;

            if (currentDist > targetDistance + 1f)
            {
                radialCorrection = directionToPlayer; 
            }
            else if (currentDist < targetDistance - 1f)
            {
                if (!Physics.Raycast(dronePos, -directionToPlayer, 1.5f, obstacleMask))
                {
                    radialCorrection = -directionToPlayer;
                }
            }

            Vector3 moveDirection = (strafeLeftRight + radialCorrection * 0.5f).normalized;
            float speed = Enemy.Stat.MoveSpeed;
            Enemy.rb.linearVelocity = moveDirection * speed;

            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            Enemy.transform.rotation = Quaternion.Slerp(Enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
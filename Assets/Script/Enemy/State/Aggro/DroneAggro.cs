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

        private float AggroMax = 2;
        private float AggroTimer;
        
        public DroneAggro(BaseEmemy enemy)
        {
            Enemy = enemy;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();
            AggroTimer = AggroMax;
        }

        public override void OnStateUpdate()
        {
            AggroTimer -= Time.deltaTime;
            playerCheck();
            StrafeAroundPlayer();
        }

        public override void OnStateExit()
        {
            Enemy.rb.linearVelocity = Vector3.zero;
            Enemy.rb.angularVelocity = Vector3.zero;
        }

        private void playerCheck()
        {
            GameObject player = GlobalReference.Instance.player.gameObject;

            float playerDist = Vector3.Distance(player.transform.position, Enemy.transform.position);
            Vector3 toPlayer = player.transform.position - Enemy.transform.position;

            RaycastHit hit;
            LayerMask playerAndTerrain = GlobalReference.Instance.playerLayer | GlobalReference.Instance.TerrainLayer;
            Physics.Raycast(Enemy.transform.position, toPlayer.normalized,out hit, playerDist + 5f, playerAndTerrain);
            bool seePLayer = hit.collider.gameObject.layer == player.layer;
            
            if(Time.time - StateEnterTime > Enemy.Stat.AttackFrequentcy) Enemy.ChangeState(Enemy.stateFactory.CreateAttackState(Enemy));
            if (!seePLayer || playerDist > Enemy.Stat.DetectionRange)
            {
                AggroTimer -= Time.deltaTime;
            }
            else AggroTimer = AggroMax;

            if (AggroTimer < 0)
            {
                Enemy.ChangeState(Enemy.stateFactory.CreateIdleState(Enemy));
            }
        } 
        
        private void StrafeAroundPlayer()
        {
            if (GlobalReference.Instance?.player == null || Enemy.rb == null) return;
        
            GameObject player = GlobalReference.Instance.player.gameObject;
            
            Vector3 dronePos = Enemy.transform.position;
            Vector3 playerPos = player.transform.position;
        
            Vector3 trueToPlayer = playerPos - dronePos;
            Vector3 trueDirectionToPlayer = trueToPlayer.normalized;
        
            Vector3 strafeLeftRight = Vector3.Cross(trueDirectionToPlayer, Vector3.up).normalized * strafeDirection;
            LayerMask obstacleMask = ~GlobalReference.Instance.playerLayer; 
        
            if (Physics.SphereCast(dronePos, droneRadius, strafeLeftRight, out RaycastHit hit, collisionCheckDistance, obstacleMask))
            {
                strafeDirection *= -1f;
                strafeLeftRight = Vector3.Cross(trueDirectionToPlayer, Vector3.up).normalized * strafeDirection;
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
            Vector3 horizontalToPlayer = new Vector3(trueToPlayer.x, 0, trueToPlayer.z);
            float horizontalDist = horizontalToPlayer.magnitude;
            Vector3 horizontalDirToPlayer = horizontalToPlayer.normalized;
            Vector3 radialCorrection = Vector3.zero;
        
            if (horizontalDist > targetDistance + 1f)
            {
                radialCorrection = horizontalDirToPlayer; 
            }
            else if (horizontalDist < targetDistance - 1f)
            {
                if (!Physics.Raycast(dronePos, -horizontalDirToPlayer, 1.5f, obstacleMask))
                {
                    radialCorrection = -horizontalDirToPlayer;
                }
            }
        
            Vector3 lateralMove = (strafeLeftRight + radialCorrection * 0.5f).normalized * Enemy.Stat.MoveSpeed;
        
            float hoverHeightOffset = 4.0f; 
            float targetY = playerPos.y + hoverHeightOffset;
            float yDifference = targetY - dronePos.y;
            float verticalVelocity = Mathf.Clamp(yDifference, -2.5f, 2.5f) * 2f; 
        
            Vector3 finalVelocity = new Vector3(lateralMove.x, verticalVelocity, lateralMove.z);
            Enemy.rb.linearVelocity = finalVelocity;
        
            float relaxedY = Mathf.Lerp(playerPos.y, dronePos.y, 0.4f);
            Vector3 relaxedLookTarget = new Vector3(playerPos.x, relaxedY, playerPos.z);
            Vector3 relaxedDirToPlayer = (relaxedLookTarget - dronePos).normalized;
        
            Quaternion baseLookRotation = Quaternion.LookRotation(relaxedDirToPlayer);
            
            Vector3 localMoveDir = Quaternion.Inverse(baseLookRotation) * finalVelocity.normalized;
            float maxTiltAngle = 25f;
            
            float rollAngle = -localMoveDir.x * maxTiltAngle; 
            float pitchAngle = localMoveDir.z * maxTiltAngle;
        
            Quaternion tiltAdjustment = Quaternion.Euler(pitchAngle, 0, rollAngle);
            Quaternion targetRotation = baseLookRotation * tiltAdjustment;
        
            Enemy.transform.rotation = Quaternion.Slerp(Enemy.transform.rotation, targetRotation, Time.deltaTime * 8f);
        }
    }
}
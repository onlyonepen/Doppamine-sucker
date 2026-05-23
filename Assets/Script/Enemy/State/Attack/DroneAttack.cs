using DG.Tweening;
using UnityEngine;

namespace Script.Enemy.State.Attack
{
    public class DroneAttack : EnemyBaseState
    {
        private bool shoted;
        
        private float Anticipation = 1f;
        private float Recovery = .2f;

        public DroneAttack(BaseEmemy enemy)
        {
            Enemy = enemy;
        }
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            shoted = false;
            Enemy.rb.constraints =  RigidbodyConstraints.FreezePosition;
            Enemy.ChargeUpParticles.Play();
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if(Time.time - StateEnterTime > Anticipation && !shoted)
            {
                shoted = true;
                shootProjectile();
            }

            if(Time.time - StateEnterTime > Anticipation + Recovery) Enemy.ChangeState(Enemy.stateFactory.CreateAggroState(Enemy));
            
            if(!shoted) LookAtPlayer();
        }

        public override void OnStateExit()
        {
            Enemy.rb.constraints =  RigidbodyConstraints.None;
            Enemy.rb.constraints =  RigidbodyConstraints.FreezeRotation;
            Enemy.ChargeUpParticles.Stop();
        }

        private void shootProjectile()
        {
            Enemy.Guntip.LookAt(GlobalReference.Instance.player.transform.position);
            GameObject bullet = GameObject.Instantiate(Enemy.ProjectilePrefab, Enemy.Guntip.position, Enemy.Guntip.rotation);
            bullet.GetComponent<BasicEnemyProjectile>().ProjectileOwner = Enemy;
            Enemy.ChargeUpParticles.Stop();
        }

        private void LookAtPlayer()
        {
            Vector3 dronePos = Enemy.transform.position;
            Vector3 playerPos = GlobalReference.Instance.player.gameObject.transform.position;

            Vector3 directionToPlayer = (playerPos - dronePos).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            Enemy.transform.rotation = Quaternion.Slerp(Enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
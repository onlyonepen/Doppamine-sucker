using DG.Tweening;
using UnityEngine;

namespace Script.Enemy.State.Attack
{
    public class DroneAttack : EnemyBaseState
    {
        private bool shoted;
        
        private float Anticipation = 1f;
        private float Recovery = 1f;

        public DroneAttack(BaseRangedEmemy enemy)
        {
            Enemy = enemy;
        }
        
        public override void OnStateEnter()
        {
            base.OnStateEnter();
            shoted = false;
            
            Enemy.transform.DOShakePosition(Anticipation,0.5f,100);
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
        }

        private void shootProjectile()
        {
            Enemy.Guntip.LookAt(GlobalReference.Instance.player.transform.position);
            GameObject.Instantiate(Enemy.ProjectilePrefab, Enemy.Guntip.position, Enemy.Guntip.rotation);

        }
    }
}
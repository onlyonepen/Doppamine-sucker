using UnityEngine;

namespace Script.Enemy.State.Attack
{
    public class DroneSpreadShot : EnemyBaseState
    {
        private bool shoted;
        
        private float Anticipation = 0.5f;
        private float Recovery = 0.5f;

        public override void OnStateEnter(BaseRangedEmemy _ememy)
        {
            base.OnStateEnter(_ememy);
            shoted = false;
        }

        public override void OnStateUpdate()
        {
            base.OnStateUpdate();
            if(Time.time - StateEnterTime > Anticipation && !shoted)
            {
                shoted = true;
                shootProjectile();
            }

            if(Time.time - StateEnterTime > Anticipation + Recovery) Enemy.ChangeState(StateFactory.Instance.CreateState(EnemyStatesEnum.Aggro));
        }

        private void shootProjectile()
        {
            Enemy.Guntip.LookAt(GlobalReference.Instance.player.transform.position);
            Quaternion centerRotation = Enemy.Guntip.rotation;

            float randomSpreadAngle = Random.Range(5f, 35f);

            float randomAxisTilt = Random.Range(0f, 360f);
            Vector3 randomSpreadAxis = Quaternion.AngleAxis(randomAxisTilt, Enemy.Guntip.forward) * Enemy.Guntip.up;

            Quaternion leftRotation = Quaternion.AngleAxis(-randomSpreadAngle, randomSpreadAxis) * centerRotation;
            Quaternion rightRotation = Quaternion.AngleAxis(randomSpreadAngle, randomSpreadAxis) * centerRotation;

            GameObject.Instantiate(Enemy.ProjectilePrefab, Enemy.Guntip.position, centerRotation);
            GameObject.Instantiate(Enemy.ProjectilePrefab, Enemy.Guntip.position, leftRotation);
            GameObject.Instantiate(Enemy.ProjectilePrefab, Enemy.Guntip.position, rightRotation);
        }
    }
    
}
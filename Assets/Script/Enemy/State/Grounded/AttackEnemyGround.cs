using UnityEngine;

public class AttackEnemyGround : EnemyState
{
        private bool shoted;
        
        private float Anticipation = 0.5f;
        private float Recovery = 0.5f;

        public override void OnStateEnter(EnemySM gamestateManager)
        {
                base.OnStateEnter(gamestateManager);
                shoted = false;
        }

        public override void OnStateUpdate()
        {
                base.OnStateUpdate();
                if(Time.time - stateEnterTime > Anticipation && !shoted)
                {
                        shoted = true;
                        shootProjectile();
                }
                if(Time.time - stateEnterTime > Anticipation + Recovery) manager.ChangeState("Aggro");
        }

        private void shootProjectile()
        {
                manager.Guntip.LookAt(GlobalReference.Instance.player.transform.position);
                Instantiate(manager.Projectile, manager.Guntip.position, manager.Guntip.rotation);
        }
}
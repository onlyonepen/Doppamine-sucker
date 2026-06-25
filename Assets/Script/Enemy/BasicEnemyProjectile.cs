using System;
using DG.Tweening;
using Script.Enemy;
using UnityEngine;

public class BasicEnemyProjectile : MonoBehaviour , IParriable
{
        public float speed = 50f;
        public float lifetime = 10f;
        private float spawnTimeStamp;

        [HideInInspector]public BaseEnemy ProjectileOwner;
        private bool parried = false;
        
        private void OnEnable()
        {
                spawnTimeStamp = Time.time;
        }

        private void Update()
        {
                if (!parried)
                {
                        transform.position += transform.forward * speed * Time.deltaTime;
                        if (Time.time - spawnTimeStamp > lifetime)
                        {
                                Destroy(gameObject);
                        }
                }
        }

        private void OnCollisionEnter(Collision collision)
        {
                if(!parried)
                {
                        bool hitPlayer = (1 << collision.gameObject.layer & GlobalReference.Instance.playerLayer) != 0;
                        bool hitTerrain = (1 << collision.gameObject.layer & GlobalReference.Instance.TerrainLayer) != 0;
                        if(hitPlayer) GlobalReference.Instance.player.playerHp.takedamage();
                        if(hitPlayer || hitTerrain) Destroy(gameObject);       
                }
        }

        public void Parried()
        {
                parried = true;
                ProjectileOwner.ChangeState(ProjectileOwner.stateFactory.CreateStaggerState(ProjectileOwner));
                HitStopUtil.Instance.TriggerGlobalHitStop(0.15f);
                float DiedDelay = 0.1f;
                transform.DOMove(ProjectileOwner.transform.position, DiedDelay).SetEase(Ease.Linear);
                Invoke("KillOwner", DiedDelay);
        }

        private void KillOwner()
        {
                ProjectileOwner.Death();
                Destroy(gameObject);
        }
}
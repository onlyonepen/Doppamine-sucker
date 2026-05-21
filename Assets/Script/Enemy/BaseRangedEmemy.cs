using System;
using Script.Enemy.EnemiesStats;
using Script.Enemy.State;
using UnityEngine;

namespace Script.Enemy
{
    public class BaseRangedEmemy : MonoBehaviour, IDamagable
    {
        [SerializeField] internal EnemyStatSO Stat;
        [SerializeField] private EnemyStatesEnum StarterState;
        [SerializeField] internal Rigidbody rb;
        [SerializeField] private ParticleSystem DeathParticles;
        [SerializeField] internal Transform Guntip;
        [SerializeField] internal GameObject ProjectilePrefab;
        private EnemyBaseState currentState;

        private void Start()
        {
            currentState = StateFactory.Instance.CreateState(StarterState);
            currentState.OnStateEnter(this);
        }

        public void ChangeState(EnemyBaseState nextState)
        {
            currentState.OnStateExit();
            currentState = nextState;
            Debug.Log("current state is " + currentState);
            currentState.OnStateEnter(this);
        }

        private void Update()
        {
            currentState.OnStateUpdate();
        }

        public void GetPull()
        {
            ChangeState(StateFactory.Instance.CreateState(EnemyStatesEnum.GetPull));
        }
        
        public void TakeDamage()
        {
            Debug.Log("Died");
            HitStopUtil.Instance.TriggerGlobalHitStop(0.1f);
            gameObject.SetActive(false);
            DeathParticles.transform.parent = null;
            DeathParticles.Play();
        }
    }
}
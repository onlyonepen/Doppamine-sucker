using System;
using Script.Enemy.EnemiesStats;
using Script.Enemy.State;
using UnityEngine;

namespace Script.Enemy
{
    public class BaseRangedEmemy : MonoBehaviour, IDamagable
    {
        public EnemyType Type;
        [SerializeField] internal EnemyStatSO Stat;
        [SerializeField] internal Rigidbody rb;
        [SerializeField] private ParticleSystem DeathParticles;
        [SerializeField] internal Transform Guntip;
        [SerializeField] internal GameObject ProjectilePrefab;

        public IEnemyStateFactory stateFactory { get; private set; }
        private EnemyBaseState currentState;
        
        private void Start()
        {
            stateFactory = CreateFactory(Type);
            currentState = stateFactory.CreateIdleState(this);
            currentState.OnStateEnter();
        }

        public void ChangeState(EnemyBaseState nextState)
        {
            currentState.OnStateExit();
            currentState = nextState;
            Debug.Log("current state is " + currentState);
            currentState.OnStateEnter();
        }

        private void Update()
        {
            currentState.OnStateUpdate();
        }

        public void GetPull()
        {
            ChangeState(stateFactory.CreateGetpullState(this));
        }
        
        public void TakeDamage()
        {
            Debug.Log("Died");
            HitStopUtil.Instance.TriggerGlobalHitStop(0.1f);
            gameObject.SetActive(false);
            DeathParticles.transform.parent = null;
            DeathParticles.Play();
        }

        private IEnemyStateFactory CreateFactory(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.LightDrone:
                    return new LightDroneFactory();
                case EnemyType.HeavyDrone:
                    return new HeavyDroneFactory();
                default:
                    throw new NotImplementedException("Not implemented for " + type);
            }
        }
    }
}
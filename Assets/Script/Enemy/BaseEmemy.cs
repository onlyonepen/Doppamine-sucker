using System;
using System.Collections;
using JL.Splitting;
using Script.Enemy.EnemiesStats;
using Script.Enemy.State;
using Unity.VisualScripting;
using UnityEngine;
using VInspector;

namespace Script.Enemy
{
    public class BaseEmemy : MonoBehaviour, IDamagable
    {
        public EnemyType Type;
        [SerializeField] internal EnemyStatSO Stat;
        [SerializeField] internal Rigidbody rb;
        [SerializeField] private ParticleSystem DeathParticles;
        [SerializeField] internal Transform Guntip;
        [SerializeField] internal GameObject ProjectilePrefab;
        [SerializeField] internal ParticleSystem ChargeUpParticles;
        
        //test
        [SerializeField] private Splittable splittable;
        [SerializeField] private Transform _planeTransform; 

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
            currentState.OnStateEnter();
        }

        private void Update()
        {
            currentState.OnStateUpdate();
        }

        public void GetPull()
        {
            ChangeState(stateFactory.CreateStaggerState(this));
        }
        
        public void SplitDeath()
        {
            Death();
            SplitObject();
        }

        public void Death()
        {
            //StartCoroutine(delayHitstop(0.05f));
            DeathParticles.transform.parent = null;
            DeathParticles.Play();
            ChangeState(stateFactory.CreateStaggerState(this));
            enabled = false;
            GlobalReference.Instance.player.currentEnergy = GlobalReference.Instance.player.MaxEnergy;
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

        internal float staggerTime;
        public void Stagger(float time = 120)
        {
            staggerTime = time;
            ChangeState(stateFactory.CreateStaggerState(this));
        }
        public void SplitObject()
        {
            ChangeState(stateFactory.CreateStaggerState(this));
            PointPlane plane = new PointPlane(_planeTransform.position, _planeTransform.rotation);
            splittable.SplitAsync(plane, (SplitResult result) =>
            {
                result.posObject.AddComponent<Rigidbody>();
                result.negObject.AddComponent<Rigidbody>();
            });
            splittable.SplitAsync(plane);
        }
        IEnumerator delayHitstop(float time)
        {
            yield return new WaitForSeconds(time);
            HitStopUtil.Instance.TriggerGlobalHitStop(0.3f);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Stat.DetectionRange);
        }
    }
}
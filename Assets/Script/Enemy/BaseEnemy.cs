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
    public class BaseEnemy : MonoBehaviour, IDamagable
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

        public IEnemyStateFactory stateFactory { get; private set; }
        private EnemyBaseState currentState;

        [HideInInspector] public bool Iskilled = false;

        private Collider col;
        
        private void Start()
        {
            stateFactory = CreateFactory(Type);
            currentState = stateFactory.CreateIdleState(this);
            currentState.OnStateEnter();
            
            col =  GetComponent<Collider>();
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
        
        public void SplitDeath(Transform plane)
        {
            // 1. Make the enemy untargetable immediately so the player can't hit it again.
            // (This ensures our new OverlapBox cast won't catch it while it's asynchronously splitting)
            if (TryGetComponent<Collider>(out var col))
            {
                col.enabled = false;
            }

            // 2. Start the split. We will call Death() AFTER the split finishes.
            SplitObject(plane);
        }

        public void Death()
        {
            DeathParticles.transform.parent = null;
            DeathParticles.Play();
            ChangeState(stateFactory.CreateStaggerState(this));
            GlobalReference.Instance.player.currentEnergy = GlobalReference.Instance.player.MaxEnergy;
    
            // 2. Disable the entire GameObject (replaces this.enabled = false;)
            gameObject.SetActive(false); 
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
        public void SplitObject(Transform _plane)
        {
            ChangeState(stateFactory.CreateStaggerState(this));
            PointPlane plane = new PointPlane(_plane.position, _plane.rotation);
    
            float splitForce = 15f; 
            Vector3 originalLinearVel = rb.linearVelocity; 
            Vector3 originalAngularVel = rb.angularVelocity;

            splittable.SplitAsync(plane, (SplitResult result) =>
            {
                Rigidbody r1 = result.posObject.AddComponent<Rigidbody>();
                Rigidbody r2 = result.negObject.AddComponent<Rigidbody>();

                r1.linearVelocity = originalLinearVel;
                r1.angularVelocity = originalAngularVel;

                r2.linearVelocity = originalLinearVel;
                r2.angularVelocity = originalAngularVel;

                r1.AddForce(plane.normal * splitForce, ForceMode.Impulse);
                r2.AddForce(-plane.normal * splitForce, ForceMode.Impulse);

                result.posObject.transform.parent = null;
                result.negObject.transform.parent = null;

                // 3. Start a short Coroutine to finish the death sequence safely
            });
            StartCoroutine(WaitAndDie());
        }
        private IEnumerator WaitAndDie()
        {
            // The Splittable plugin requires exactly 1 frame to run its "ResetCenterOfMassNextFrame" coroutine.
            // We yield twice just to be absolutely safe before disabling the parent GameObject.
            yield return null;
            yield return null;
    
            Death();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Stat.DetectionRange);
        }
    }
}
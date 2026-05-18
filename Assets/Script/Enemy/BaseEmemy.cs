using System;
using Script.Enemy.EnemiesStats;
using Script.Enemy.State;
using UnityEngine;

namespace Script.Enemy
{
    public class BaseEmemy : MonoBehaviour
    {
        [SerializeField] internal EnemyStatSO Stat;
        [SerializeField] private EnemyStatesEnum StarterState;
        private EnemyBaseState currentState;

        private void Start()
        {
            StateFactory factory = new StateFactory();
            currentState = factory.CreateState(StarterState);
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
    }
}
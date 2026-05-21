using System;
using Script.Enemy.State;
using Script.Enemy.State.Aggro;
using Script.Enemy.State.Attack;
using Script.Enemy.State.GetPull;
using Script.Enemy.State.Idle;
using UnityEngine;

namespace Script.Enemy
{
    public enum EnemyStatesEnum
    {
        Idle,
        Aggro,
        Attack,
        GetPull
    }

    public class StateFactory : MonoBehaviour
    {
        public static StateFactory Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        } 
        public EnemyBaseState CreateState(EnemyStatesEnum enemyStateEnum)
        {
            switch (enemyStateEnum)
            {
                case EnemyStatesEnum.Idle:
                    return new DroneIdle();
                case EnemyStatesEnum.Aggro:
                    return new DroneAggro();
                case EnemyStatesEnum.GetPull :
                    return new DroneGetPull();
                case EnemyStatesEnum.Attack:
                    return new DroneAttack();
;                default:
                    throw new NotImplementedException("CUM");
            }
        }
    }
}
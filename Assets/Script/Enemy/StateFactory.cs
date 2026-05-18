using System;
using Script.Enemy.State;
using Script.Enemy.State.Aggro;
using Script.Enemy.State.Idle;

namespace Script.Enemy
{
    //TODO: Can be singleton later
    
    public enum EnemyStatesEnum
    {
        Idle,
        Aggro,
        Attack
    }

    public class StateFactory
    {
        public EnemyBaseState CreateState(EnemyStatesEnum enemyStateEnum)
        {
            switch (enemyStateEnum)
            {
                case EnemyStatesEnum.Idle:
                    return new DroneIdle();
                case EnemyStatesEnum.Aggro:
                    return new DroneAggro();
                default:
                    throw new NotImplementedException("CUM");
            }
        }
    }
}
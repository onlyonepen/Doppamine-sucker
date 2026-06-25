using System;
using Script.Enemy.State;
using Script.Enemy.State.Aggro;
using Script.Enemy.State.Attack;
using Script.Enemy.State.GetPull;
using Script.Enemy.State.Idle;
using UnityEngine;

namespace Script.Enemy
{
    public enum EnemyType { LightDrone, HeavyDrone }
    
    public class LightDroneFactory : IEnemyStateFactory
    {
        public EnemyBaseState CreateIdleState(BaseEnemy enemy)
        {
            return new DroneIdle(enemy);
        }

        public EnemyBaseState CreateAggroState(BaseEnemy enemy)
        {
            return new DroneAggro(enemy);
        }

        public EnemyBaseState CreateAttackState(BaseEnemy enemy)
        {
            return new DroneAttack(enemy);
        }

        public EnemyBaseState CreateStaggerState(BaseEnemy enemy)
        {
            return new DroneStagger(enemy);
        }
    }

    public class HeavyDroneFactory : IEnemyStateFactory
    {
        public EnemyBaseState CreateIdleState(BaseEnemy enemy)
        {
            return new DroneIdle(enemy);
        }

        public EnemyBaseState CreateAggroState(BaseEnemy enemy)
        {
            return new DroneAggro(enemy);
        }

        public EnemyBaseState CreateAttackState(BaseEnemy enemy)
        {
            return new DroneSpreadShot(enemy);
        }

        public EnemyBaseState CreateStaggerState(BaseEnemy enemy)
        {
            return new DroneStagger(enemy);
        }
    }
}
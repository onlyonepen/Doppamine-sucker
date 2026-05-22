using Script.Enemy;
using Script.Enemy.State;

public interface IEnemyStateFactory
{
    EnemyBaseState CreateIdleState(BaseRangedEmemy enemy);
    EnemyBaseState CreateAggroState(BaseRangedEmemy enemy);
    EnemyBaseState CreateAttackState(BaseRangedEmemy enemy);
    EnemyBaseState CreateGetpullState(BaseRangedEmemy enemy);
}
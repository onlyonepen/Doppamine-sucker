using Script.Enemy;
using Script.Enemy.State;

public interface IEnemyStateFactory
{
    EnemyBaseState CreateIdleState(BaseEmemy enemy);
    EnemyBaseState CreateAggroState(BaseEmemy enemy);
    EnemyBaseState CreateAttackState(BaseEmemy enemy);
    EnemyBaseState CreateStaggerState(BaseEmemy enemy);
}
using System;

namespace Script.Enemy.State
{
    public abstract class EnemyBaseState
    {
        protected BaseEmemy Enemy;

        public virtual void OnStateEnter(BaseEmemy _ememy)
        {
            Enemy = _ememy;
        }
        public virtual void OnStateUpdate() {  }
        public virtual void OnStateExit() {  }
    }
}
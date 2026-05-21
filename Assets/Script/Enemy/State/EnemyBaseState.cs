using System;
using UnityEngine;

namespace Script.Enemy.State
{
    public abstract class EnemyBaseState
    {
        protected BaseRangedEmemy Enemy;
        protected float StateEnterTime;

        public virtual void OnStateEnter(BaseRangedEmemy _ememy)
        {
            Enemy = _ememy;
            StateEnterTime = Time.time;
        }
        public virtual void OnStateUpdate() {  }
        public virtual void OnStateExit() {  }
    }
}
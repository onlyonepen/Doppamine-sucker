using System;
using UnityEngine;

namespace Script.Enemy.State
{
    public abstract class EnemyBaseState
    {
        protected BaseEmemy Enemy;
        protected float StateEnterTime;
        

        public virtual void OnStateEnter()
        {
            StateEnterTime = Time.time;
        }
        public virtual void OnStateUpdate() {  }
        public virtual void OnStateExit() {  }
    }
}
using UnityEngine;

public class EnemyState : MonoBehaviour
{
    public EnemySM manager;
    public GameObject player;

    public float stateEnterTime;
    public virtual void OnStateEnter(EnemySM gamestateManager)
    {
        manager = gamestateManager;
        player = manager.gameObject;
        stateEnterTime = Time.time;
    }
    public virtual void OnStateUpdate() { }
    public virtual void OnStatePhysicsUpdate() { }
    public virtual void OnStateExit() { }
    public virtual void OnStateTriggerEnter(Collider collider) { }
}
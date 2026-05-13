using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySM : MonoBehaviour, IDamagable
{
    public string CurrentStateString;
    public GameObject Projectile;
    public Transform Guntip;
    public float AttackFrequentcy = 7f;
    public float SightRange = 40f;
    public GameObject enemyObj;
    public ParticleSystem deadParticle;
    [Tooltip("Must have Idle state")]
    public List<EnemyStateSeralize> states;
    public List<Transform> AllSpot;

    [HideInInspector]public int currentSpot = 0;
    [HideInInspector] public EnemyState currentState;
    private IDamagable damagableImplementation;

    #region stateStruct
    [System.Serializable]
    public struct EnemyStateSeralize
    {
        public string stateName;
        public EnemyState state;
    }
    #endregion

    void Start()
    {
        foreach (var _state in states.Where(_state => _state.stateName == "Idle"))
        {
            currentState = _state.state;
            currentState.OnStateEnter(this);
            break;
        }
    }

    void Update()
    {
        currentState.OnStateUpdate();
    }
    
    public void WaitToChangeState(string stateName, float WaitDur , float EnterTime)
    {
        if(Time.time - EnterTime > WaitDur)
        {
            ChangeState(stateName);
        }
    }

    public void ChangeState(string stateName)
    {
        foreach (var _state in states.Where(_state => _state.stateName == stateName))
        {
            currentState.OnStateExit();
            currentState = _state.state;
            currentState.OnStateEnter(this);
            CurrentStateString = currentState.name;
            return;
        }

        Debug.LogError("State " + stateName + " not found");
    }

    public void Grappled()
    {
        enemyObj.transform.DOKill(currentState);
        ChangeState("GetPull");
    }

    public void TakeDamage()
    {
        deadParticle.gameObject.transform.parent = null;
        deadParticle.Play();
        gameObject.SetActive(false);
    }
}
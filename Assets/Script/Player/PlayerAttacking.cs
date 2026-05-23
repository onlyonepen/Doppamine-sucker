using System.Collections;
using System.Collections.Generic;
using Script.Enemy;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    [SerializeField] private PlayerAttackArea attackArea;
    [SerializeField] private PlayerStateManager stateManager;
    public Coroutine Attack1Cou;

    [HideInInspector] bool IsAttacking = false;
    bool attackAgain;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CallAttack();
        }
    }

    public void CallAttack()
    {
        if (IsAttacking)
        {
            attackAgain = true;
        }
        else
        {
            StartCoroutine(attack1Enum());
        }
    }

    IEnumerator attack1Enum()
    {
        IsAttacking = true;
        attackAgain = false;

        yield return new WaitForSeconds(0.1f);
        Attack1();

        if (attackAgain)
        {
            StartCoroutine(attack1Enum());
        }

        IsAttacking = false;
    }

    public void Attack1()
    {
        if (attackArea == null || attackArea.InArea == null) return;

        HashSet<IDamagable> hitTargets = new();
    
        int parriableLayer = LayerMask.NameToLayer("Parriable");
        
        foreach (GameObject obj in attackArea.InArea)
        {
            if (obj == null) continue;

            if (((1 << obj.layer) & GlobalReference.Instance.EnemyLayer) != 0)
            {
                var damagable = obj.GetComponentInParent<IDamagable>();
    
                if (damagable != null && hitTargets.Add(damagable))
                {
                    damagable.TakeDamage(); 
                }
            }
            else if (obj.layer == parriableLayer)
            {
                if (obj.TryGetComponent(out IParriable parriable))
                {
                    parriable.Parried();
                }
                else 
                {
                    Debug.LogWarning($"Object '{obj.name}' is on the Parriable layer but missing the IParriable component.");
                }
            }
        }
    }

}

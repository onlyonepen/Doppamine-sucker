using System.Collections;
using System.Collections.Generic;
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
        HashSet<IDamagable> hitTargets = new();

        foreach (GameObject obj in attackArea.InArea)
        {
            // Use GetComponentInParent to look up the hierarchy if needed
            var damagable = obj.GetComponentInParent<IDamagable>();
        
            if (damagable != null)
            {
                if (hitTargets.Add(damagable))
                {
                    // Don't forget to pass your 'dmg' variable here if your interface supports it!
                    damagable.TakeDamage(); 
                }
            }
        }
    }

}

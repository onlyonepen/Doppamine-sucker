using System.Collections;
using UnityEngine;

public class PlayerAttacking : MonoBehaviour
{
    [SerializeField] private PlayerAttackArea attackArea;
    [SerializeField] private PlayerStateManager stateManager;
    public float AttackDamage = 1;
    public float Knockback = 5f;
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

        yield return new WaitForSeconds(0.2f);
        Attack1(AttackDamage);

        if (attackAgain)
        {
            StartCoroutine(attack1Enum());
        }

        IsAttacking = false;
    }

    public void Attack1(float dmg)
    {
        foreach(GameObject obj in attackArea.InArea)
        {
            if (obj.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.TakeDamage(dmg, gameObject.transform, Knockback);
            }
        }
    }

}

using UnityEngine;

public interface IDamagable
{
    public void TakeDamage(float dmg, Transform origin, float knockback);
}

using UnityEngine;
using VInspector;

public class PlayerHpManager : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    [ReadOnly] public int CurrentHp;
    
    [Header("Invulnerability")]
    public bool isInvulnerable = false;
    [SerializeField] private GameObject InvulnerabilityEfffect;


    public void TurnOnInvulnerability()
    {
        InvulnerabilityEfffect.SetActive(true);
        isInvulnerable = true;
    }

    public void TurnOffInvulnerability()
    {
        InvulnerabilityEfffect.SetActive(false);
        isInvulnerable = false;
    }
    
    public void takedamage(int damage = 1)
    {
        if (isInvulnerable) return;
        CurrentHp -= damage;
    }
}

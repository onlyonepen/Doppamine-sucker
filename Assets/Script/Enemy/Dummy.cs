using UnityEngine;

public class Dummy : MonoBehaviour, IDamagable
{
    private Rigidbody rb;
    public ParticleSystem deadParticle;

    public PlayerAttackArea attackArea;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void TakeDamage()
    {

    }


    private void OnDestroy()
    {
        deadParticle.gameObject.transform.parent = null;
        attackArea.InArea.Remove(gameObject);
        deadParticle.Play();
    }
}

using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 10; 
    public float damageCooldown = 1.0f; 

    public LayerMask targetLayer;

    private float nextHitTime;
    private Animator ownerAnimator;

    private void Start()
    {
        ownerAnimator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Zombie zombie = GetComponentInParent<Zombie>();
        if (zombie != null && !zombie.IsAlive()) return;
        Debug.Log("DamageSource hit: " + other.gameObject.name + " Layer: " + other.gameObject.layer);
        if (Time.time < nextHitTime) { Debug.Log("Blocked by cooldown"); return; }
        if (ownerAnimator != null && !ownerAnimator.GetBool("isAttacking")) { Debug.Log("Blocked by isAttacking = false"); return; }
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Debug.Log("Layer passed, looking for IDamageable");
            IDamageable target = other.GetComponent<IDamageable>();
            Debug.Log("IDamageable found: " + (target != null));
            if (Time.time < nextHitTime) return;
        if (ownerAnimator != null && !ownerAnimator.GetBool("isAttacking")) return;
        }
        else
        {
            Debug.Log("Layer check failed");
        }
    
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            IDamageable target = other.GetComponent<IDamageable>();

            if (target != null)
            {
                Debug.Log("DamageSource on " + transform.root.name + " dealt " + damageAmount + " to " + other.name);
                target.TakeDamage(damageAmount);
                nextHitTime = Time.time + damageCooldown;
            }
        }
    }
}
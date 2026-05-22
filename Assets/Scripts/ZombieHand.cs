using UnityEngine;

public class ZombieHand : MonoBehaviour
{
    public int damage = 10;
    private Collider handCollider;

    private void Start()
    {
        handCollider = GetComponent<Collider>();
        // Start with the "fist" turned off so we don't damage by touching
        EnableAttack(true);
    }

    // This is called by the Animator or State Machine
    public void EnableAttack(bool enable)
    {
        handCollider.enabled = enable;
    }

    private void OnTriggerEnter(Collider other)
    {
        Zombie zombie = GetComponentInParent<Zombie>();
        if (zombie != null && !zombie.IsAlive()) return;

        Debug.Log("Zombie hand touched: " + other.name);

        Debug.Log("Zombie hand touched: " + other.name);
        // Look for the interface on whatever we touched
        IDamageable hitTarget = other.GetComponent<IDamageable>();

        if (hitTarget != null)
        {
            Debug.Log("ZombieHand on " + transform.root.name + " dealt " + damage + " to " + other.name);
            hitTarget.TakeDamage(damage);
            EnableAttack(false);
        }
    }
}

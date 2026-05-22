using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour, IDamageable
{
    [SerializeField] private int HP = 100;
    [SerializeField] private float disappearDelay = 5f;

    private Animator animator;
    private EnemyAudio enemyAudio; // Reference to our audio controller

    private NavMeshAgent navAgent;
    private Collider enemyCollider;
    private bool isDead = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();

        // Find the EnemyAudio component attached to this zombie
        enemyAudio = GetComponent<EnemyAudio>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        HP -= damageAmount;
        if (HP <= 0)
        {
            Die();
        }
        else
        {
            animator.SetTrigger("DAMAGE");

            // OPTIONAL: If you ever want to add hurt sounds, it would go here!
            // if (enemyAudio != null) enemyAudio.PlayHurt();
        }
    }

    private void Die()
    {
        isDead = true;

        // Disable ALL colliders immediately, before anything else
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider childCollider in allColliders)
        {
            childCollider.enabled = false;
        }

        // Then do everything else
        if (enemyAudio != null)
            enemyAudio.PlayDeath();

        int randomValue = Random.Range(0, 2);
        animator.SetTrigger(randomValue == 0 ? "DIE1" : "DIE2");

        CharacterController charController = GetComponent<CharacterController>();
        if (charController != null)
            charController.enabled = false;

        if (navAgent != null)
        {
            navAgent.isStopped = true;
            navAgent.enabled = false;
        }

        if (EnemySpawner.Instance != null)
            EnemySpawner.Instance.EnemyDied();

        Destroy(gameObject, disappearDelay);

        Debug.Log("Zombie " + gameObject.name + " collision completely cleared.");
    }

    public bool IsAlive()
    {
        return !isDead;
    }
    public void PlayAttackSound()
    {
        if (enemyAudio != null && !isDead)
        {
            enemyAudio.PlayAttack();
        }
    }
}
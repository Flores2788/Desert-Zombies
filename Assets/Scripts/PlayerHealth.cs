using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("HUD")]
    public RectTransform healthbarFill;

    [Header("Death")]
    public GameObject deathScreenUI;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (deathScreenUI == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform ds = canvas.transform.Find("DeathScreen");
                if (ds != null)
                    deathScreenUI = ds.gameObject;
            }
        }

        if (deathScreenUI != null)
            deathScreenUI.SetActive(false);

        Debug.Log("DeathScreen reference: " + (deathScreenUI != null ? deathScreenUI.name : "NULL"));
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        Debug.Log("Player took " + damage + " damage\n" + new System.Diagnostics.StackTrace());
        if (damage == 50) { Debug.Log("Blocked 50 damage"); return; }
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("Die() called");
        // Show death screen
        if (deathScreenUI != null)
            deathScreenUI.SetActive(true);

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Freeze the game
        Time.timeScale = 0f;
    }

    private void UpdateHealthUI()
    {
        if (healthbarFill == null)
            return;

        float healthPercent = currentHealth / maxHealth;

        Vector3 scale = healthbarFill.localScale;
        scale.x = healthPercent;
        healthbarFill.localScale = scale;
    }
}

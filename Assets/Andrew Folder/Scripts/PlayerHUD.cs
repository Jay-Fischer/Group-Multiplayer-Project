using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private Image healthFill;
    [SerializeField] private Gradient healthColor;
    [SerializeField] private float barSmoothSpeed = 8f;

    [Header("Damage Flash")]
    [SerializeField] private Image damageFlash;
    [SerializeField] private float flashAlpha = 0.4f;
    [SerializeField] private float flashFadeSpeed = 2f;

    [Header("Events")]
    public UnityEvent onPlayerDied;

    [Header("Testing")]
    [SerializeField] private bool enableTestKeys = true;

    private float displayedFill = 1f;
    private bool isDead;

    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (damageFlash != null)
        {
            SetFlashAlpha(0f);
        }
    }

    private void Update()
    {
        if (enableTestKeys && Keyboard.current != null)
        {
            if (Keyboard.current.kKey.wasPressedThisFrame) TakeDamage(10f);
            if (Keyboard.current.hKey.wasPressedThisFrame) Heal(10f);
        }

        UpdateHealthBar();
        FadeFlash();
    }

    // Same signature as EnemyManager.onPlayerAttacked, so it can be hooked up in the Inspector
    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);

        if (damageFlash != null)
        {
            SetFlashAlpha(flashAlpha);
        }

        if (currentHealth <= 0f)
        {
            isDead = true;
            onPlayerDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null) return;

        float targetFill = currentHealth / maxHealth;
        displayedFill = Mathf.Lerp(displayedFill, targetFill, barSmoothSpeed * Time.deltaTime);

        healthFill.fillAmount = displayedFill;
        healthFill.color = healthColor.Evaluate(displayedFill);
    }

    private void FadeFlash()
    {
        if (damageFlash == null || damageFlash.color.a <= 0f) return;

        SetFlashAlpha(Mathf.MoveTowards(damageFlash.color.a, 0f, flashFadeSpeed * Time.deltaTime));
    }

    private void SetFlashAlpha(float alpha)
    {
        Color c = damageFlash.color;
        c.a = alpha;
        damageFlash.color = c;
    }
}

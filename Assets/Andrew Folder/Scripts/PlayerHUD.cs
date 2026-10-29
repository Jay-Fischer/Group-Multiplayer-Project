using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f; // keep this the same as Max Health on the Player's NetworkHealth
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

    // The local player's networked health, once they have spawned in a multiplayer session
    private NetworkHealth networkHealth;

    public float CurrentHealth => currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;

        if (damageFlash != null)
        {
            SetFlashAlpha(0f);
        }
    }

    private void OnDestroy()
    {
        UnbindNetworkHealth();
    }

    private void Update()
    {
        UpdateNetworkBinding();

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
        if (networkHealth != null)
        {
            // In multiplayer, health lives on the player and only the server/host can change it
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                networkHealth.TakeDamage(amount);
            }
            return;
        }

        if (isDead || amount <= 0f) return;

        SetHealth(currentHealth - amount);
    }

    public void Heal(float amount)
    {
        // NetworkHealth has no heal yet, so healing only works offline
        if (networkHealth != null || isDead || amount <= 0f) return;

        SetHealth(currentHealth + amount);
    }

    private void SetHealth(float newHealth)
    {
        float oldHealth = currentHealth;
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

        if (currentHealth < oldHealth && damageFlash != null)
        {
            SetFlashAlpha(flashAlpha);
        }

        if (currentHealth <= 0f && !isDead)
        {
            isDead = true;
            onPlayerDied?.Invoke();
        }
        else if (currentHealth > 0f)
        {
            isDead = false;
        }
    }

    private void UpdateNetworkBinding()
    {
        NetworkManager manager = NetworkManager.Singleton;

        // Lost our player (disconnected or despawned), so go back to offline mode
        if (networkHealth != null && (!networkHealth.IsSpawned || manager == null || !manager.IsClient))
        {
            UnbindNetworkHealth();
        }

        if (networkHealth != null || manager == null || !manager.IsClient || manager.LocalClient == null)
        {
            return;
        }

        NetworkObject localPlayer = manager.LocalClient.PlayerObject;

        if (localPlayer != null && localPlayer.TryGetComponent(out NetworkHealth health))
        {
            networkHealth = health;
            networkHealth.CurrentHealth.OnValueChanged += OnNetworkHealthChanged;

            isDead = false;
            currentHealth = networkHealth.CurrentHealth.Value;
            displayedFill = currentHealth / maxHealth;
        }
    }

    private void UnbindNetworkHealth()
    {
        if (networkHealth != null)
        {
            networkHealth.CurrentHealth.OnValueChanged -= OnNetworkHealthChanged;
            networkHealth = null;
        }
    }

    private void OnNetworkHealthChanged(float previous, float current)
    {
        SetHealth(current);
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

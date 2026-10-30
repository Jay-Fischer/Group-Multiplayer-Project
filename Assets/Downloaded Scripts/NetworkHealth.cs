using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool despawnOnDeath;
    public PlayerLogic logic;
    public Gun shooting;

    public NetworkVariable<float> CurrentHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (logic != null && shooting != null)
        {
            CurrentHealth.OnValueChanged += OnHealthStateChanged;
            EvaluatePlayerState(CurrentHealth.Value);
        }
        
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            
        }
    }
    public override void OnNetworkDespawn()
    {
        if (logic != null && shooting != null)
            CurrentHealth.OnValueChanged += OnHealthStateChanged;
    }

    public void TakeDamage(float amount)
    {
        if (!IsSpawned || !IsServer)
        {
            return;
        }

        if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount))
        {
            return;
        }

        if (CurrentHealth.Value <= 0f)
        {
            return;
        }

        CurrentHealth.Value = Mathf.Max(0f, CurrentHealth.Value - amount);

        if (CurrentHealth.Value <= 0f && despawnOnDeath)
        {
            NetworkObject.Despawn(true);
        }
    }

    public void HealHealth(float amount)
    {
        if (CurrentHealth.Value == 0f)
        {
            CurrentHealth.Value = 10;
            logic.enabled = true;
            shooting.enabled = true;
        }
        else if (CurrentHealth.Value != 100f) CurrentHealth.Value += amount;
        if (CurrentHealth.Value > 100) CurrentHealth.Value = 100;
    }

    private void OnHealthStateChanged(float previousValue, float newValue)
    {
        EvaluatePlayerState(newValue);
    }

    private void EvaluatePlayerState(float health)
    {
        if (health <= 0)
        {
            SetPlayerCapabilities(isActive: false);
        }
        else
        {
            SetPlayerCapabilities(isActive: true);
        }
    }

    private void SetPlayerCapabilities(bool isActive)
    {
        // 1. Toggle gameplay/logic (Movement, shooting, abilities)
        if (logic != null && shooting != null)
        {
            logic.enabled = isActive;
            shooting.enabled = isActive;
        }
    }
    }

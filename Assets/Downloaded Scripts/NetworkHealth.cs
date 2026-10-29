using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class NetworkHealth : NetworkBehaviour
{
    [SerializeField] float maxHealth = 100f;
    [SerializeField] bool despawnOnDeath;

    public NetworkVariable<float> CurrentHealth = new(100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
        }
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
}

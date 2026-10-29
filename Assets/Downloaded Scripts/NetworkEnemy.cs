using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkHealth))]
public class NetworkEnemy : NetworkBehaviour
{
    [SerializeField] float damage = 10f;
    [SerializeField] float speed = 3.5f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCooldown = 1f;

    private NavMeshAgent agent;
    private NetworkHealth health;
    private float nextAttack;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<NetworkHealth>();
        agent.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        agent.enabled = IsServer;

        if (!IsServer)
        {
            return;
        }

        agent.speed = speed;
        agent.stoppingDistance = attackRange * 0.9f;
        nextAttack = 0f;
    }

    public override void OnNetworkDespawn()
    {
        agent.enabled = false;
    }

    private void Update()
    {
        if (!IsSpawned || !IsServer)
        {
            return;
        }

        if (health.CurrentHealth.Value <= 0f)
        {
            return;
        }

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        NetworkHealth target = FindClosestPlayer();

        if (target == null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }

        float distanceSquared = (target.transform.position - transform.position).sqrMagnitude;

        bool inRange = distanceSquared <= attackRange * attackRange;

        if (inRange) 
        {
            agent.SetDestination(target.transform.position);
        }
        else if (Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCooldown;
            target.TakeDamage(damage);
        }
    }

    private NetworkHealth FindClosestPlayer()
    {
        NetworkHealth closest = null;
        float closestDistanceSquare = Mathf.Infinity;

        foreach (NetworkClient client in NetworkManager.ConnectedClientsList)
        {
            NetworkObject player = client.PlayerObject;

            if (player == null || !player.IsSpawned || !player.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!player.TryGetComponent(out NetworkHealth playerHealth))
            {
                continue;
            }

            if (playerHealth.CurrentHealth.Value <= 0f)
            {
                continue;
            }

            float distanceSquared = (player.transform.position - transform.position).sqrMagnitude;

            if (distanceSquared <= closestDistanceSquare)
            {
                closestDistanceSquare = distanceSquared;
                closest = playerHealth;
            }
        }
        return closest;
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public Transform player;
    public NavMeshAgent enemyPrefab;
    public Transform[] enemySpawnPoints;
    public float spawnTime = 3;

    public float health = 100f;
    public float damage = 10f;
    public float speed = 3.5f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;

    public UnityEvent<float> onPlayerAttacked;

    class Enemy
    {
        public NavMeshAgent agent;
        public float health;
        public float nextAttack;
    }

    readonly List<Enemy> enemies = new();

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), spawnTime, spawnTime);
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null || enemySpawnPoints.Length == 0)
        {
            return;
        }

        Transform point = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
        NavMeshAgent agent = Instantiate(enemyPrefab, point.position, point.rotation);

        agent.speed = speed;
        agent.stoppingDistance = attackRange * 0.9f;

        enemies.Add(new Enemy { agent = agent, health = health });
    }

    private void Update()
    {
        enemies.RemoveAll(enemy => enemy.agent == null);
        if (player == null)
        {
            return;
        }

        foreach (Enemy enemy in enemies)
        {
            NavMeshAgent agent = enemy.agent;
            if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
            {
                continue;
            }

            bool inRange = Vector3.Distance(agent.transform.position, player.position) <= attackRange;

            agent.isStopped = inRange;

            if (!inRange)
            {
                agent.SetDestination(player.position);
            }
            else if (Time.time >= enemy.nextAttack)
            {
                enemy.nextAttack = Time.time + attackCooldown;
                onPlayerAttacked?.Invoke(damage);
            }
        }
    }

    public void DamageEnemy(NavMeshAgent agent, float amount)
    {
        Enemy enemy = enemies.Find(e => e.agent == agent);

        if (enemy == null || amount <= 0f)
        {
            return;
        }

        enemy.health -= amount;

        if (enemy.health <= 0f)
        {
            agent.gameObject.SetActive(false);
            Destroy(agent.gameObject);
        }
    }
}

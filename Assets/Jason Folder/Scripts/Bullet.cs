using UnityEngine;
using UnityEngine.AI;

public class DamageOnContact : MonoBehaviour
{
    public EnemyManager enemyManager;
    public float damage = 25f;

    private bool hasHit;
    private void Start()
    {
        enemyManager = FindAnyObjectByType<EnemyManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        NavMeshAgent enemy = other.GetComponentInParent<NavMeshAgent>();

        if (enemy != null)
        {
            hasHit = true;
            enemyManager.DamageEnemy(enemy, damage);
            Destroy(gameObject);
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class DamageOnContact : NetworkBehaviour
{
    //public EnemyHandler enemyManager;
    public float damage = 25f;
    public KillScript killScript;

    private bool hasHit;
    private void Start()
    {
        //enemyManager = FindAnyObjectByType<EnemyHandler>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        NetworkHealth enemy = other.GetComponentInParent<NetworkHealth>();

        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
            //enemyManager.DamageEnemy(enemy, damage);
            killScript.instantDestroyRpc();
        }
        if(other.tag == "Wall") killScript.instantDestroyRpc();
    }
}

using UnityEngine;
using System.Collections;
using System;
using UnityEngine.AI;

public class EnemyMovementHandler : MonoBehaviour
{
    private Transform player;

    private void Awake()
    {
        player = FindFirstObjectByType</*Player Movement Script Name Here*/>().transform;
    }

    private void Update()
    {
        if (GetComponent</*Enemey Health Script Name Here*/>().currentHealth > 0 && player.GetComponent</*Player Health Script Name Here*/>().currentHealth)
        {
            GetComponent<NavMeshAgent>().SetDestination(player.position);
        }
        else
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }
    }
}

using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NetworkObject))]
public class EnemyHandler : NetworkBehaviour
{
    public enum WaveState
    {
        Waiting,
        Active,
        Stopped
    }

    [SerializeField] NetworkEnemy enemyPrefab;
    [SerializeField] Transform[] enemySpawnPoints;

    [SerializeField, Min(1)] int firstWaveOfEnemies;
    [SerializeField, Min(1)] int enemiesAddedPerWave;

    [SerializeField, Min(0f)] float timeBeforeFirstWave;
    [SerializeField, Min(0f)] float timeBetweenWaves;

    public NetworkVariable<int> CurrentWave = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> EnemiesRemaining = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> TimeToNextWave = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<WaveState> State = new(WaveState.Stopped, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly List<NetworkEnemy> enemiesAlive = new();
    private float nextWaveTime;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        enemiesAlive.Clear();

        CurrentWave.Value = 0;
        EnemiesRemaining.Value = 0;

        BeginWaiting(timeBeforeFirstWave);
    }

    private void Update()
    {
        if(!IsSpawned || !IsServer)
        {
            return;
        }

        switch (State.Value)
        {
            case WaveState.Waiting:
                UpdateCountdown();
                break;
            case WaveState.Active:
                CheckWaveFinished();
                break;
        }
    }

    private void BeginWaiting(float duration)
    {
        duration = Mathf.Max(0f, duration);
        nextWaveTime = Time.time + duration;
        TimeToNextWave.Value = Mathf.CeilToInt(duration);
        State.Value = WaveState.Waiting;
    }

    private void UpdateCountdown()
    {
        float timeRemaining = Mathf.Max(0f, nextWaveTime - Time.time);

        int secondsRemaining = Mathf.CeilToInt(timeRemaining);

        if (TimeToNextWave.Value != secondsRemaining)
        {
            TimeToNextWave.Value = secondsRemaining;
        }

        if (timeRemaining <= 0f)
        {
            StartNextWave();
        }
    }

    private void StartNextWave()
    {
        if (enemyPrefab == null || enemySpawnPoints == null) // failsafe to ensure there are spawn points and enemy prefabs
        {
            return;
        }

        List<Vector3> validPositions = new();
        List<Quaternion> validRotations = new();

        foreach (Transform point in enemySpawnPoints)
        {
            if (point == null)
            {
                continue;
            }

            if(NavMesh.SamplePosition(point.position, out NavMeshHit hit, 2f,  NavMesh.AllAreas))
            {
                validPositions.Add(hit.position);
                validRotations.Add(point.rotation);
            }
        }

        if (validPositions.Count == 0) // failsafe to ensure there are spawn points near the navmesh
        {
            return;
        }

        CurrentWave.Value++;

        int enemyCount = Mathf.Max(1, firstWaveOfEnemies) + (CurrentWave.Value - 1) * Mathf.Max(1, enemiesAddedPerWave);

        for (int i = 0; i < enemyCount; i++)
        {
            int index = Random.Range(0, validPositions.Count);

            NetworkEnemy enemy = Instantiate(enemyPrefab, validPositions[index], validRotations[index]);

            enemy.NetworkObject.Spawn();
            enemiesAlive.Add(enemy);
        }

        EnemiesRemaining.Value = enemiesAlive.Count;
        TimeToNextWave.Value = 0;
        State.Value = WaveState.Active;
    }
    
    private void CheckWaveFinished()
    {
        enemiesAlive.RemoveAll(enemy =>  enemy == null || !enemy.IsSpawned);

        if (EnemiesRemaining.Value != enemiesAlive.Count)
        {
            EnemiesRemaining.Value = enemiesAlive.Count;
        }

        if (enemiesAlive.Count == 0)
        {
            BeginWaiting(timeBetweenWaves);
        }
    }
    
}

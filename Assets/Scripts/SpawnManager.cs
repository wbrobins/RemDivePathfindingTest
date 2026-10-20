using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private bool autoStart = true;

    private readonly List<EnemyBehavior> alive = new List<EnemyBehavior>();

    [SerializeField] private UnityEngine.Events.UnityEvent onAllWavesDefeated;

    void Start()
    {
        if (autoStart)
        {
            StartCoroutine(RunWaves());
        }
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waves.Count; i++)
        {
            Wave wave = waves[i];

            foreach (EnemySpawn spawn in wave.spawns)
            {
                Spawn(spawn);
            }

            bool isLastWave = i == waves.Count - 1;

            if (wave.waitUntilCleared && !isLastWave)
            {
                yield return WaitUntilCleared();
                yield return new WaitForSeconds(wave.delay);
            }
            else if (!wave.waitUntilCleared && !isLastWave)
            {
                yield return new WaitForSeconds(wave.delay);
            }
        }

        yield return WaitUntilCleared();
        
        Debug.Log("All waves defeated!");
        onAllWavesDefeated?.Invoke();
    }

    private IEnumerator WaitUntilCleared()
    {
        yield return new WaitUntil(() =>{alive.RemoveAll(e => e == null); return alive.Count == 0;});
    }

    private EnemyBehavior Spawn(EnemySpawn spawn)
    {
        Vector3 pos = spawn.position;

        if (spawn.snapToNavMesh && NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            pos = hit.position;
        }

        EnemyBehavior enemy = Instantiate(spawn.prefab, pos, Quaternion.identity);

        if (spawn.dataOverride != null)
        {
            enemy.SetUp(new Enemy(spawn.dataOverride));
        }

        enemy.transform.rotation = spawn.rotation;

        alive.Add(enemy);
        return enemy;
    }
}


[System.Serializable]
public class Wave
{
    public string name;
    public bool waitUntilCleared = true;
    public float delay;
    public List<EnemySpawn> spawns = new List<EnemySpawn>();
}

[System.Serializable]
public class EnemySpawn
{
    public EnemyBehavior prefab;
    public EnemyBase dataOverride;
    public Vector3 position;
    public Quaternion rotation;
    public bool snapToNavMesh = true;
}

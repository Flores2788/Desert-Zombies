using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    public GameObject enemyPrefab;
    public GameObject enemyPrefab2;
    public Transform[] spawnPoints;
    public int enemiesPerWave = 5;
    public float respawnDelay = 5f;
    public GameObject portal;
    public int waveToUnlockPortal = 10;
    public VictoryScreen victoryScreen;

    private int aliveEnemies = 0;
    private int currentWave = 0;
    private bool isSpawning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnWave();
    }

    public bool isBossScene = false;

    void SpawnWave()
    {
        currentWave++;
        if (!isBossScene)
            enemiesPerWave = currentWave * 2;
        aliveEnemies = enemiesPerWave;
        Debug.Log("Wave " + currentWave + " started with " + enemiesPerWave + " enemies");

        for (int i = 0; i < enemiesPerWave; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];

            GameObject prefabToSpawn = enemyPrefab;
            if (enemyPrefab2 != null && Random.value > 0.5f)
                prefabToSpawn = enemyPrefab2;

            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
    }

    public void EnemyDied()
    {
        aliveEnemies--;
        Debug.Log("Enemy died. Alive enemies: " + aliveEnemies + " | isSpawning: " + isSpawning);

        if (aliveEnemies <= 0 && !isSpawning)
        {
            Debug.Log("All enemies dead, starting next wave coroutine");
            StartCoroutine(RespawnWave());
        }
    }

    IEnumerator RespawnWave()
    {
        isSpawning = true;
        Debug.Log("Next wave in " + respawnDelay + "s");
        yield return new WaitForSeconds(respawnDelay);

        if (currentWave >= waveToUnlockPortal)
        {
            if (isBossScene && victoryScreen != null)
            {
                victoryScreen.ShowVictory();
                isSpawning = false;
                yield break;
            }

            if (portal != null)
            {
                portal.SetActive(true);
                Debug.Log("Portal unlocked!");
            }
            isSpawning = false;
            yield break;
        }

        SpawnWave();
        isSpawning = false;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }
}
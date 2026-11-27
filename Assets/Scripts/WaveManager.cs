using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Referensi Prefab & Spawn")]
    public GameObject boarPrefab;
    public Transform[] rightSpawnPoints; // Titik spawn di sisi kanan map

    [Header("Konfigurasi Wave 1 (GDD)")]
    public int[] wave1SubWaves = { 1, 3, 6, 10 }; // Total = 20 musuh
    public float spawnInterval = 0.8f; // Jeda antar spawn dalam 1 kelompok
    public float delayBetweenSubWaves = 1.5f; // Jeda setelah kelompok sebelumnya habis
    public int wave1ClearBonus = 100;

    private int currentSubWaveIndex = 0;
    private List<BoarEnemy> activeEnemies = new List<BoarEnemy>();
    private bool isSpawning = false;
    private bool waveCompleted = false;

    private void Start()
    {
        // Wave 1 dimulai otomatis saat masuk ke map (sesuai GDD)
        StartWave1();
    }

    public void StartWave1()
    {
        currentSubWaveIndex = 0;
        waveCompleted = false;
        StartCoroutine(SpawnSubWaveRoutine(wave1SubWaves[currentSubWaveIndex]));
    }

    private IEnumerator SpawnSubWaveRoutine(int enemyCount)
    {
        isSpawning = true;
        Debug.Log($"Memulai Wave 1 - Kelompok {currentSubWaveIndex + 1}: Memunculkan {enemyCount} Babi Hutan.");

        for (int i = 0; i < enemyCount; i++)
        {
            SpawnSingleBoar();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
    }

    private void SpawnSingleBoar()
    {
        if (rightSpawnPoints.Length == 0 || boarPrefab == null)
        {
            Debug.LogError("Prefab Babi Hutan atau Right Spawn Points belum diisi di Inspector!");
            return;
        }

        // Pilih titik spawn secara acak dari daftar spawn point sebelah kanan
        Transform chosenSpawn = rightSpawnPoints[Random.Range(0, rightSpawnPoints.Length)];
        GameObject spawnedObj = Instantiate(boarPrefab, chosenSpawn.position, Quaternion.identity);

        BoarEnemy enemy = spawnedObj.GetComponent<BoarEnemy>();
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            enemy.OnEnemyDied += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(BoarEnemy deadEnemy)
    {
        deadEnemy.OnEnemyDied -= HandleEnemyDied;
        activeEnemies.Remove(deadEnemy);

        // Cek jika semua musuh di kelompok ini sudah dikalahkan dan proses spawn kelompok selesai
        if (!isSpawning && activeEnemies.Count == 0 && !waveCompleted)
        {
            currentSubWaveIndex++;

            if (currentSubWaveIndex < wave1SubWaves.Length)
            {
                // Lanjut ke sub-wave berikutnya (misal dari 1 ke 3, lalu 6, lalu 10)
                StartCoroutine(DelayedNextSubWave());
            }
            else
            {
                // Seluruh 20 musuh di Wave 1 sudah habis
                CompleteWave1();
            }
        }
    }

    private IEnumerator DelayedNextSubWave()
    {
        yield return new WaitForSeconds(delayBetweenSubWaves);
        StartCoroutine(SpawnSubWaveRoutine(wave1SubWaves[currentSubWaveIndex]));
    }

    private void CompleteWave1()
    {
        waveCompleted = true;
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(wave1ClearBonus); // +100 koin bonus

        Debug.Log("WAVE 1 SELESAI! Masuk ke Checkpoint 1 (Free Session).");
    }
}
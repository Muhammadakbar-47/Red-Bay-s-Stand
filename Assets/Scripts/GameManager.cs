using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Ekonomi Koin")]
    public int currentCoins = 0;

    [Header("Status Permainan")]
    public bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        Debug.Log($"Koin bertambah +{amount}. Total koin saat ini: {currentCoins}");
        // Update UI teks koin di HUD di sini nanti
    }

    public bool SpendCoins(int amount)
    {
        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            Debug.Log($"Koin terpakai -{amount}. Sisa koin: {currentCoins}");
            return true;
        }
        Debug.Log("Koin tidak cukup!");
        return false;
    }

    public void TriggerGameOver(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log($"GAME OVER! Penyebab: {reason}");
        Time.timeScale = 0f; // Menghentikan pergerakan game sementara
        // Munculkan panel UI Game Over di sini nanti
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public enum EntityType
    {
        PlayerKala,
        TreeHouse
    }

    [Header("Konfigurasi Entitas")]
    public EntityType entityType = EntityType.PlayerKala;
    public int maxHP = 200; // Set 200 untuk Kala, 1000 untuk Rumah Pohon
    public int currentHP;

    private Animator animator;

    private void Awake()
    {
        currentHP = maxHP;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"{gameObject.name} terkena {damage} damage! Sisa HP: {currentHP}/{maxHP}");

        if (entityType == EntityType.PlayerKala && animator != null)
        {
            // Opsional: trigger animasi terkena hit jika ada
            // animator.SetTrigger("Hurt");
        }

        if (currentHP <= 0)
        {
            HandleDeath();
        }
    }

    // Dipakai oleh item Perban (+100 HP) atau Pupuk (+200 HP)
    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log($"{gameObject.name} dipulihkan +{amount} HP. HP saat ini: {currentHP}/{maxHP}");
    }

    // Khusus item Pupuk pada Rumah Pohon Ulin (+200 Max HP & +200 Current HP)
    public void UpgradeMaxHP(int bonusAmount)
    {
        maxHP += bonusAmount;
        currentHP += bonusAmount;
        Debug.Log($"Max HP {gameObject.name} meningkat menjadi {maxHP}! HP saat ini: {currentHP}/{maxHP}");
    }

    private void HandleDeath()
    {
        if (GameManager.Instance == null) return;

        if (entityType == EntityType.PlayerKala)
        {
            GameManager.Instance.TriggerGameOver("HP Kala habis!");
        }
        else if (entityType == EntityType.TreeHouse)
        {
            GameManager.Instance.TriggerGameOver("Rumah Pohon Ulin hancur!");
        }
    }
}
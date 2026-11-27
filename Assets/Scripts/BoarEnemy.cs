using System;
using UnityEngine;


public class BoarEnemy : MonoBehaviour
{
    private Color originalColor;
    [Header("Statistik Sesuai GDD")]
    public int maxHP = 50;
    public int damageToPlayer = 20;
    public int damageToTree = 30;
    public int coinReward = 5;
    public float moveSpeed = 2.5f;

    [Header("Deteksi & Serangan")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;
    public float playerAggroRange = 3.5f;

    private int currentHP;
    private float lastAttackTime;
    private Transform treeTarget;
    private Transform playerTarget;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Event untuk memberi tahu WaveManager bahwa musuh ini mati
    public Action<BoarEnemy> OnEnemyDied;

    private void Awake()
    {
        currentHP = maxHP;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject treeObj = GameObject.FindGameObjectWithTag("TreeHouse");
        if (treeObj != null) treeTarget = treeObj.transform;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTarget = playerObj.transform;
    }

    private void Update()
    {
        Transform currentTarget = DetermineTarget();
        if (currentTarget == null) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        // Flip sprite: gambar asli menghadap kanan, jadi flipX = true jika bergerak ke kiri
        if (currentTarget.position.x < transform.position.x)
            spriteRenderer.flipX = true;
        else if (currentTarget.position.x > transform.position.x)
            spriteRenderer.flipX = false;

        if (distance > attackRange)
        {
            // Bergerak mendekati target
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
            if (animator != null) animator.SetBool("IsMoving", true);
        }
        else
        {
            // Berhenti dan menyerang
            if (animator != null) animator.SetBool("IsMoving", false);
            AttackTarget(currentTarget);
        }
    }

    private Transform DetermineTarget()
    {
        // Babi hutan memprioritaskan Kala jika berada dalam jarak aggro, jika tidak maka ke Rumah Pohon
        if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= playerAggroRange)
        {
            return playerTarget;
        }
        return treeTarget;
    }

    private void AttackTarget(Transform target)
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        if (animator != null) animator.SetTrigger("Attack");

        HealthSystem targetHealth = target.GetComponent<HealthSystem>();
        if (targetHealth != null)
        {
            if (target.CompareTag("Player"))
                targetHealth.TakeDamage(damageToPlayer); // 20 damage ke Kala
            else if (target.CompareTag("TreeHouse"))
                targetHealth.TakeDamage(damageToTree);   // 30 damage ke Rumah Pohon
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        StartCoroutine(HitFlashRoutine());

        // Sedikit efek terpental mundur saat dicakar
        if (playerTarget != null)
        {
            Vector2 knockbackDir = (transform.position - playerTarget.position).normalized;
            transform.position += (Vector3)(knockbackDir * 0.35f);
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }
    private System.Collections.IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = new Color(1f, 0.35f, 0.35f); // Berubah kemerahan sesaat
        yield return new WaitForSeconds(0.12f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddCoins(coinReward); // +5 koin

        OnEnemyDied?.Invoke(this);
        Destroy(gameObject);
    }
}

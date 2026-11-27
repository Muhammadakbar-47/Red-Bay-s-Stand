using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Konfigurasi Serangan Sesuai GDD")]
    public int currentDamage = 30;      // Wave 1 (Cakar) = 30, Tombak Lv.1 = 50, dst.
    public float attackRange = 1.2f;    // Jangkauan tebasan cakar
    public float attackOffsetX = 0.8f;  // Jarak titik serang di depan badan Kala
    public LayerMask enemyLayer;        // Pilih layer musuh di Inspector

    private PlayerController playerController;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        playerController.Attacked += HandleAttack;
    }

    private void OnDisable()
    {
        playerController.Attacked -= HandleAttack;
    }

    private void HandleAttack(bool facingRight)
    {
        // Tentukan titik tengah area serang (kanan atau kiri Kala)
        float direction = facingRight ? 1f : -1f;
        Vector2 attackOrigin = (Vector2)transform.position + new Vector2(direction * attackOffsetX, 0f);

        // Deteksi semua musuh dalam lingkaran serangan
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackOrigin, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            BoarEnemy boar = enemy.GetComponent<BoarEnemy>();
            if (boar != null)
            {
                boar.TakeDamage(currentDamage);
            }
        }
    }

    // Visualisasi lingkaran area cakar di jendela Scene (warna merah)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float dir = (playerController != null && playerController.FacingRight) ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position + new Vector2(dir * attackOffsetX, 0f);
        Gizmos.DrawWireSphere(origin, attackRange);
    }
}
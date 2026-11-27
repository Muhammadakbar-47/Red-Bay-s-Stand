using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Gerak 8 arah. Animasi hanya KIRI dan KANAN:
//  - bergerak kiri/kanan  -> animasi jalan kiri/kanan
//  - bergerak atas/bawah  -> memakai arah kiri/kanan terakhir
//  - diam                 -> idle (digambar menghadap kiri, dicerminkan bila menghadap kanan)
//  - klik kiri / tombol R -> animasi serang (Kala berhenti sebentar saat menyerang)
// Bekerja di Input System lama maupun baru.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Animator animator;              // kosongkan = otomatis
    [SerializeField] private SpriteRenderer spriteRenderer;  // kosongkan = otomatis
    [SerializeField] private bool startFacingRight = false;  // arah awal saat game mulai

    [Header("Serangan (klik kiri atau tombol R)")]
    [SerializeField] private float attackDuration = 0.32f;   // samakan dengan lama animasi serang
    [SerializeField] private float attackCooldown = 0.1f;    // jeda sebelum boleh menyerang lagi
    [SerializeField] private bool faceMouseOnClick = false;  // false = serang ke arah hadap terakhir; true = ke sisi mouse saat klik

    [Header("Batas area jalan (tidak bisa keluar peta)")]
    [SerializeField] private bool limitArea = true;
    [SerializeField] private Vector2 areaMin = new Vector2(-12.2f, -6.6f);
    [SerializeField] private Vector2 areaMax = new Vector2(12.2f, 3.4f);

    // Dikirim saat serangan dimulai (true = menghadap kanan). Dipakai sistem damage nanti.
    public event Action<bool> Attacked;
    public bool IsAttacking => Time.time < attackEndTime;
    public bool FacingRight => facingRight;

    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool facingRight;
    private float attackEndTime;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;      // top-down: tidak ada gravitasi
        rb.freezeRotation = true;  // tidak berputar saat menabrak
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        facingRight = startFacingRight;
    }

    private void Update()
    {
        bool attacking = IsAttacking;

        if (Time.time >= nextAttackTime && ReadAttackInput(out bool byMouse))
        {
            StartAttack(byMouse);
            attacking = true;
        }

        // Selama menyerang, Kala berhenti. normalized: diagonal tidak lebih cepat.
        moveInput = attacking ? Vector2.zero : ReadMoveInput().normalized;
        bool moving = moveInput.sqrMagnitude > 0.01f;

        // Arah berubah hanya jika ada input horizontal; naik/turun mempertahankan arah terakhir
        if (moveInput.x != 0f) facingRight = moveInput.x > 0f;

        if (animator != null)
        {
            animator.SetBool(IsMovingHash, moving);
            animator.SetFloat(MoveXHash, facingRight ? 1f : -1f);   // -1 = kiri, 1 = kanan
        }

        // Jalan dan serang: gambar kiri dan kanan sudah terpisah (tanpa cermin).
        // Diam: hanya ada idle menghadap KIRI -> dicerminkan saat menghadap kanan.
        if (spriteRenderer != null) spriteRenderer.flipX = !moving && !attacking && facingRight;
    }

    private void StartAttack(bool faceMouse)
    {
        // Opsi Face Mouse On Click aktif: klik menghadap sisi mouse. Selain itu: pakai arah hadap terakhir.
        if (faceMouse && faceMouseOnClick && TryGetMouseWorldX(out float mouseX))
            facingRight = mouseX > transform.position.x;

        attackEndTime = Time.time + attackDuration;
        nextAttackTime = attackEndTime + attackCooldown;

        if (animator != null)
        {
            animator.SetFloat(MoveXHash, facingRight ? 1f : -1f);
            animator.SetTrigger(AttackHash);
        }

        Attacked?.Invoke(facingRight);
    }

    private void FixedUpdate()
    {
        Vector2 next = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;

        if (limitArea)
        {
            next.x = Mathf.Clamp(next.x, areaMin.x, areaMax.x);
            next.y = Mathf.Clamp(next.y, areaMin.y, areaMax.y);
        }

        rb.MovePosition(next);
    }

    // Garis kuning di Scene view = batas area (muncul saat objek kala dipilih)
    private void OnDrawGizmosSelected()
    {
        if (!limitArea) return;
        Gizmos.color = Color.yellow;
        Vector3 center = (areaMin + areaMax) * 0.5f;
        Vector3 size = areaMax - areaMin;
        Gizmos.DrawWireCube(center, size);
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return Vector2.zero;

        float x = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1f : 0f)
                - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1f : 0f);
        float y = (k.wKey.isPressed || k.upArrowKey.isPressed ? 1f : 0f)
                - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1f : 0f);
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    // Klik kiri atau tombol R
    private bool ReadAttackInput(out bool byMouse)
    {
#if ENABLE_INPUT_SYSTEM
        byMouse = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool key = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        byMouse = Input.GetMouseButtonDown(0);
        bool key = Input.GetKeyDown(KeyCode.R);
#endif
        return byMouse || key;
    }

    private bool TryGetMouseWorldX(out float worldX)
    {
        worldX = 0f;
        Camera cam = Camera.main;
        if (cam == null) return false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return false;
        Vector2 screen = Mouse.current.position.ReadValue();
#else
        Vector2 screen = Input.mousePosition;
#endif
        worldX = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z))).x;
        return true;
    }
}

using UnityEngine;

// Pasang di objek anak "FadeZone" milik pohon/rumah pohon.
// FadeZone harus punya Collider2D dengan Is Trigger dicentang.
// Saat Player masuk zona (berada di belakang kanopi), sprite target memudar.
public class FadeWhenPlayerBehind : MonoBehaviour
{
    [SerializeField] private SpriteRenderer target;          // sprite yang dipudarkan
    [SerializeField, Range(0f, 1f)] private float fadedAlpha = 0.4f;
    [SerializeField] private float fadeSpeed = 6f;
    [SerializeField] private string playerTag = "Player";

    private float targetAlpha = 1f;

    // Dipanggil otomatis saat komponen ditambahkan di editor
    private void Reset()
    {
        target = GetComponentInParent<SpriteRenderer>();
        if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
    }

    private void Awake()
    {
        if (target == null)
            Debug.LogWarning("FadeWhenPlayerBehind: slot Target kosong di " + name, this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) targetAlpha = fadedAlpha;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) targetAlpha = 1f;
    }

    private void Update()
    {
        if (target == null) return;

        Color c = target.color;
        c.a = Mathf.MoveTowards(c.a, targetAlpha, fadeSpeed * Time.deltaTime);
        target.color = c;
    }
}

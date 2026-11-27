using UnityEngine;

// Pasang di Player (isStatic = false) dan semua pohon/batu/rumah pohon (isStatic = true).
// Makin rendah posisi Y objek, makin di DEPAN tampilannya.
// Pivot sprite harus di pangkal/kaki objek.
[RequireComponent(typeof(SpriteRenderer))]
public class SortByY : MonoBehaviour
{
    [SerializeField] private bool isStatic = true;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        Apply();
    }

    private void LateUpdate()
    {
        if (!isStatic) Apply();
    }

    private void Apply()
    {
        sr.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
    }
}

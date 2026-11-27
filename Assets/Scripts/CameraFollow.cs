using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;           // drag Player ke sini
    [SerializeField] private float smoothTime = 0.15f;   // makin kecil = makin kaku
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.3f);

    [Header("Batas peta (opsional)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-5f, -4f);
    [SerializeField] private Vector2 maxBounds = new Vector2(5f, 4f);

    private Vector3 velocity;

    // LateUpdate: kamera bergerak SETELAH pemain selesai bergerak, biar tidak bergetar
    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);

        if (useBounds)
        {
            goal.x = Mathf.Clamp(goal.x, minBounds.x, maxBounds.x);
            goal.y = Mathf.Clamp(goal.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}

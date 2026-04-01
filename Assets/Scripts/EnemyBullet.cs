using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyBullet : MonoBehaviour
{
    public float speed = 15f;    // Bằng y chang tốc độ đạn của Player
    public int damage = 15;      // Sát thương viên đạn quái bắn ra
    public float lifetime = 3f;  // Thời gian sống bằng đạn Player
    public GameObject impactEffectPrefab; 

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 

        // Bay về phía trước (theo trục X)
        rb.linearVelocity = transform.right * speed;
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Chặn MỌI THỨ: Nếu không phải là Player thì viên đạn sẽ bay xuyên qua luôn (KHÔNG NỔ)
        if (!hitInfo.CompareTag("Player")) return;

        // Xử lý sát thương khi trúng chính xác Player
        PlayerController player = hitInfo.GetComponent<PlayerController>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }

        // Sinh ra hiệu ứng nổ / va chạm
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        }

        // Hủy viên đạn
        Destroy(gameObject);
    }
}

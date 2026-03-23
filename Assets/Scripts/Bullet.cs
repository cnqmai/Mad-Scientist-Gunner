using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 20;      // Sát thương của mỗi viên đạn
    public float lifetime = 3f;  // Đạn sống bao lâu trước khi biến mất
    public GameObject impactEffectPrefab; // Hiệu ứng va chạm (từ thư mục Collision_Fx)

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Bỏ trọng lực

        // Đạn luôn bay về phía trước (trục X cục bộ)
        rb.linearVelocity = transform.right * speed;
        
        // Gắn thời gian tự hủy để dọn dẹp bộ nhớ
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Tránh xét va chạm với người chơi (giả sử Player có tag "Player")
        if (hitInfo.CompareTag("Player")) return;

        // Xử lý sát thương nếu vật thể bị tông trúng là Kẻ địch
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
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

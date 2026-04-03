using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 20;      // Sát thương của mỗi viên đạn
    public Enemy.DamageType damageType = Enemy.DamageType.Normal; // Loại sát thương
    public float lifetime = 3f;  // Đạn sống bao lâu trước khi biến mất
    public GameObject impactEffectPrefab; // Hiệu ứng va chạm (từ thư mục Collision_Fx)
    
    [Header("Laser Settings")]
    public bool isStationaryLaser = false; // Bật lên nếu dùng viên đạn này làm tia laser đứng yên
    public float damageTickRate = 0.5f;    // Thời gian giãn cách giữa các lần giật dame (nếu để laser dính vào quái)
    private float nextDamageTime;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Bỏ trọng lực

        if (isStationaryLaser)
        {
            // Tự động set damage để 3 hit chết (100 máu / 3 = 34)
            damage = 34;
            // Laser mặc định là sát thương giật điện
            damageType = Enemy.DamageType.Electric;

            // Quan trọng: Phải chuyển sang Kinematic để nó dính chặt vào nòng súng và di chuyển theo Player
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        else
        {
            // Tự động set damage để 5 hit chết (100 máu / 5 = 20)
            damage = 20;
            // Đạn bay về phía trước (nếu là đạn thường)
            rb.linearVelocity = transform.right * speed;
            
            // Tự hủy đạn sau khoảng thời gian
            Destroy(gameObject, lifetime);
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Tránh xét va chạm với người chơi (giả sử Player có tag "Player")
        if (hitInfo.CompareTag("Player")) return;

        // Xử lý sát thương nếu vật thể bị tông trúng là Kẻ địch
        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, damageType);
        }

        // Sinh ra hiệu ứng nổ / va chạm
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, transform.rotation);
        }

        // Hủy viên đạn nếu không phải laser đứng yên
        if (!isStationaryLaser)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D hitInfo)
    {
        // Chức năng giật sát thương liên tục chỉ dành cho Laser
        if (!isStationaryLaser) return;

        if (hitInfo.CompareTag("Player")) return;

        if (Time.time >= nextDamageTime)
        {
            Enemy enemy = hitInfo.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageType);
                nextDamageTime = Time.time + damageTickRate;
            }
        }
    }
}

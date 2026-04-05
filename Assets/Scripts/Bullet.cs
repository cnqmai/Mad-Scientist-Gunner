using UnityEngine;
using System.Collections.Generic;

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
    
    // Danh sách lưu các mục tiêu đang nằm trong tia laser (để chống lỗi Physics Sleep gây nhấp nháy tuỳ ý)
    private List<Enemy> overlappingEnemies = new List<Enemy>();

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Bỏ trọng lực

        if (isStationaryLaser)
        {
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

    void Update()
    {
        // Liên tục cập nhật khống chế và sát thương cho các quái đang nằm trong tia Laser
        if (isStationaryLaser)
        {
            // Quét ngược danh sách để có thể xoá an toàn nếu quái đã chết
            for (int i = overlappingEnemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = overlappingEnemies[i];
                
                // Nếu quái bị huỷ hoặc chết (script bị vô hiệu hoá) thì xoá khỏi danh sách
                if (enemy == null || !enemy.enabled)
                {
                    overlappingEnemies.RemoveAt(i);
                    continue;
                }

                // Liên tục bơm Stun khống chế khi bị nướng (nhưng máu không tuột theo giây nữa)
                enemy.ApplyStun(0.15f);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Tránh xét va chạm với người chơi
        if (hitInfo.CompareTag("Player")) return;

        Enemy enemy = hitInfo.GetComponent<Enemy>();

        if (isStationaryLaser)
        {
            // Xử lý riêng cho Laser
            if (enemy != null && !overlappingEnemies.Contains(enemy))
            {
                overlappingEnemies.Add(enemy);
                
                enemy.ApplyStun(0.15f);
                enemy.TakeLaserHit(); // Gọi hàm xử lý sát thương theo số đếm hit
            }
        }
        else
        {
            // Xử lý cho Đạn thường
            if (enemy != null)
            {
                enemy.TakeDamage(damage, damageType);
            }

            // Sinh ra hiệu ứng nổ / va chạm
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, transform.position, transform.rotation);
            }

            // Hủy viên đạn thường ngay lập tức
            Destroy(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D hitInfo)
    {
        // Khi quái vật đi ra khỏi tia laser, xoá khỏi danh sách chạm để ngừng Update
        if (!isStationaryLaser) return;

        Enemy enemy = hitInfo.GetComponent<Enemy>();
        if (enemy != null && overlappingEnemies.Contains(enemy))
        {
            overlappingEnemies.Remove(enemy);
        }
    }
}

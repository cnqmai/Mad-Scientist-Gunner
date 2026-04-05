using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public GameObject deathEffect; // Hiệu ứng nổ tung khi chết (nếu có)
    private Animator animator;     // Khai báo Animator

    [Header("Tấn công Player")]
    public float attackRange = 1.5f; // Tầm đánh xa hay gần (Ví dụ: 1.5 mét)
    public int attackDamage = 10;    // Lượng máu trừ của Player khi bị chém
    public float attackCooldown = 3f; // Chờ 3 giây mới được chém tiếp (chống bug spam)
    public float moveSpeed = 1.25f;     // Tốc độ đi bộ đuổi theo Player
    
    [Header("Bắn đạn (Chỉ bật cho Quái đánh xa)")]
    public bool isRanged = false;      // Có phải quái bắn xa không?
    public GameObject bulletPrefab;    // Prefab viên đạn quái
    public Transform firePoint;        // Vị trí nòng súng của quái

    [Header("Rơi Potion hồi máu (Random)")]
    public GameObject[] pickupPrefabs;
    [Range(0f, 1f)]
    public float dropChance = 0.35f;   // Xác suất rơi potion (mặc định 35%)

    private Transform player; // Để ghi nhớ vị trí người chơi
    private float nextAttackTime = 0f;
    private bool facingRight = false; // Phụ thuộc vào ảnh gốc con quái của bạn quay hướng nào
    private float stunTimer = 0f; // Thời gian duy trì khống chế
    private int laserHitCount = 0; // Đếm số tia laser chạm trúng

    [Header("Cấu hình giật điện nhấp nháy")]
    public string electricStateName = "Enemy_GetElectric"; // Điền TÊN CỤC STATE GIẬT điện trong Animator vào đây
    private float electricPulseTime = 0f; // Đồng hồ nhấp nháy
    private bool lastPulseState = false; // Nhớ trạng thái chớp của frame trước

    // Bullet.cs sẽ gọi hàm này và bơm thêm thời gian stun khi laser chạm quái
    public void ApplyStun(float duration)
    {
        stunTimer = duration;
    }

    public void TakeLaserHit()
    {
        if (currentHealth <= 0) return;

        laserHitCount++;
        int sliceDamage = Mathf.CeilToInt(maxHealth / 3f);

        if (laserHitCount >= 3)
        {
            TakeDamage(currentHealth, DamageType.Electric); // Trúng tia thứ 3 là chết luôn
        }
        else
        {
            TakeDamage(sliceDamage, DamageType.Electric); // Mất 1/3 máu
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>(); // Tự động tìm Animator trên enemy
        
        // Tự động tìm Player trong màn hình qua Tag (Đảm bảo Player của bạn đã được set Tag là "Player")
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // Nếu quái chết hoặc không tìm thấy Player thì không làm gì cả
        if (currentHealth <= 0 || player == null) return;

        // Cập nhật bộ đếm thời gian Stun
        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
        
        if (stunTimer > 0)
        {
            electricPulseTime += Time.deltaTime;

            if (animator != null)
            {
                animator.speed = 0f; // ĐÓNG BĂNG TUYỆT ĐỐI thời gian Animator, bắt kẹt ở Frame lúc vừa trúng đạn

                // Tính toán xem nhịp này LÀ XƯƠNG (true) hay LÀ THƯỜNG (false)
                bool currentPulseState = (electricPulseTime % 0.2f < 0.1f);

                // CHỈ chạy lệnh đổi hình khi bị đảo nhịp, TRÁNH spam mỗi frame làm Unity Optimizer bị đơ
                if (currentPulseState != lastPulseState)
                {
                    lastPulseState = currentPulseState; // Cập nhật cờ nhớ

                    if (currentPulseState)
                    {
                        animator.SetBool("isElectric", true); 
                        // Hiện Frame 0.0f (Sprite người bình thường) của Clip Electric
                        animator.Play(electricStateName, 0, 0f);
                    }
                    else
                    {
                        animator.SetBool("isElectric", true);
                        // Hiện Frame 1.0f (Sprite xương) của Clip Electric
                        animator.Play(electricStateName, 0, 1f);
                    }

                    // LỆNH THẦN THÁNH: Ép Unity cập nhật Sprite render ra màn hình NGAY TỨC KHẮC mặc kệ tốc độ thời gian đang bị khóa 0f
                    animator.Update(0f);
                }
            }

            animator.SetBool("IsWalking", false); // Dừng animation đi bộ
            return; // Thoát ra khỏi hàm Update, CHẶN không cho dòng code di chuyển & đánh chạy!
        }
        else
        {
            // Trả lại tốc độ chạy bình thường khi thả nút nướng Laser
            electricPulseTime = 0f;
            if (animator != null)
            {
                animator.speed = 1f;
                // Clear bool để phòng trường hợp Transition cũ vẫn còn kẹt
                animator.SetBool("isElectric", false); 
            }
        }

        // Tính khoảng cách giữa Quái vật và Player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Quay mặt về phía Player
        if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }

        // Kiểm tra xem đã đến lúc được đánh tiếp chưa
        if (distanceToPlayer <= attackRange)
        {
            // Nếu ở trong tầm đánh thì đứng lại chuẩn bị chém
            animator.SetBool("IsWalking", false);

            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }
        else
        {
            // Nếu ở ngoài tầm đánh thì đi bộ đuổi theo
            animator.SetBool("IsWalking", true);
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1; // Lật ngược hình ảnh
        transform.localScale = scaler;
    }

    void Attack()
    {
        // Reset thời gian chờ chiêu
        nextAttackTime = Time.time + attackCooldown;

        // Kích hoạt animation chém (Hit) hoặc bắn
        if (animator != null)
        {
            animator.SetTrigger("Hit"); 
        }

        if (isRanged && bulletPrefab != null && firePoint != null)
        {
            // Trì hoãn việc bắn đạn 0.3 giây để khớp với động tác vung súng
            Invoke("ShootBullet", 0.3f);
        }
        else if (!isRanged)
        {
            // Đánh cận chiến bằng vòng tròn ảo (cũng trì hoãn 0.3s cho chân thực)
            Invoke("MeleeDamage", 0.3f);
        }
    }

    void ShootBullet()
    {
        if (currentHealth <= 0) return; // Nếu chết trước khi kịp bắn thì hủy

        // Bắn đạn: Nếu đang quay phải (facingRight) thì đạn bay phải (0 độ), ngược lại bay trái (180 độ)
        Vector3 spawnRotation = facingRight ? Vector3.zero : new Vector3(0, 180, 0);
        Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(spawnRotation));
    }

    void MeleeDamage()
    {
        if (currentHealth <= 0) return; // Nếu chết trước khi kịp chém thì hủy

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController playerScript = hit.GetComponent<PlayerController>();
                if (playerScript != null)
                {
                    playerScript.TakeDamage(attackDamage);
                    Debug.Log("Quái vật chém Player 1 phát!");
                }
            }
        }
    }

    // Tùy chọn: Vẽ vòng tròn đỏ trong Unity Editor để bạn dễ hình dung tầm đánh
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public enum DamageType { Normal, Electric } // Phân loại sát thương

    // Hàm nhận sát thương (được gọi từ viên đạn)
    // Mặc định là Normal, nếu đạn điện gọi TakeDamage(damage, DamageType.Electric)
    public void TakeDamage(int damage, DamageType damageType = DamageType.Normal)
    {
        if (currentHealth <= 0) return; // Nếu đã chết thì không tính thêm sát thương hay Kill lặp lại nữa

        currentHealth -= damage;
        Debug.Log(gameObject.name + " bị trúng đạn! Máu còn: " + currentHealth);

        // Kích hoạt animation tương ứng nếu máu còn lớn hơn 0
        if (animator != null && currentHealth > 0)
        {
            // KHÔNG dùng Trigger cho Electric ở đây nữa [XÓA DÒNG CŨ]
            if (damageType != DamageType.Electric)
            {
                animator.SetTrigger("GetHit"); 
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " đã bị tiêu diệt!");

        // Cộng điểm và Kill
        if (UIManager.instance != null)
        {
            UIManager.instance.AddScore(1); // Giết 1 quái được 1 điểm (tương đương 1 mạng)
            UIManager.instance.AddKill();   // Tăng số mạng hạ gục lên 1
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");
            
            // Tắt va chạm để đạn bay xuyên qua (không trúng xác chết), và quái vật ngã xuống
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            float deathAnimationTime = 1f; // Thời gian chạy animation chết

            // Hẹn giờ rớt potion sau khi animation chạy xong
            Invoke("DropPotion", deathAnimationTime);

            // Xóa khỏi màn hình sau 1.1 giây (Trễ hơn hàm DropPotion 1 chút xíu để đảm bảo Potion kịp rớt ra trước khi quái bị xoá hẳn)
            Destroy(gameObject, deathAnimationTime + 0.1f); 
            
            // Vô hiệu hóa script này để không gọi máu hay hiệu ứng hit nữa
            this.enabled = false;
        }
        else
        {
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }
            DropPotion(); // Rớt ngay lập tức vì không có animation
            Destroy(gameObject); // Hủy GameObject
        }
    }

    void DropPotion()
    {
        // Random rơi potion hồi máu
        if (pickupPrefabs != null && pickupPrefabs.Length > 0 && Random.value <= dropChance)
        {
            // Chọn ngẫu nhiên 1 trong 3 loại potion
            int randomIndex = Random.Range(0, pickupPrefabs.Length);
            GameObject chosenPotion = pickupPrefabs[randomIndex];

            if (chosenPotion != null)
            {
                Vector3 dropPos = transform.position + new Vector3(0f, 0.3f, 0f);
                Instantiate(chosenPotion, dropPos, Quaternion.identity);
                Debug.Log(gameObject.name + " rơi ra potion loại " + randomIndex + " sau khi nằm xuống!");
            }
        }
    }
}

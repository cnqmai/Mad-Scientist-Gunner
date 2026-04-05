using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int maxHealth = 100; // Khai báo máu tối đa
    
    [Header("Giới hạn di chuyển")]
    public float maxX = 47.4f;  // Vị trí xa nhất bên phải màn hình
    public float minX = -47.4f; // Vị trí xa nhất bên trái màn hình
    public float maxY = -0.5f; // Vị trí cao nhất Player có thể đi lên (có thể chỉnh trong Inspector)
    public float minY = -2.8f; // Vị trí thấp nhất Player có thể đi xuống

    [Header("Hiệu ứng trúng đòn")]
    public GameObject hitMuzzlePrefab; // Kéo Prefab Muzzle 2 vào đây
    public string idleAnimationName = "Idle"; // Điền tên State Animation đứng im (ví dụ: Idle)
    public float hitStunDuration = 0.1f; // Cấu hình thời gian nhân vật đỏ lè và khựng lại

    private int currentHealth;  // Khai báo máu hiện tại
    private float hitStunTimer = 0f; // Đồng hồ đếm thời gian khựng

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private Vector2 movement;
    private bool facingRight = true;

    private bool isRunning;

    void Start()
    {
        currentHealth = maxHealth; // Khởi tạo máu đầu game
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Tìm Animator
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Lưu lại màu gốc của Player (Lúc chưa đổi sang đỏ)
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        rb.gravityScale = 0f; // Bỏ trọng lực để di chuyển góc nhìn Top-Down
        rb.freezeRotation = true; // Không cho nhân vật tự xoay khi va chạm

        // Gửi thông báo maxHealth cho UI
        if (UIManager.instance != null)
        {
            UIManager.instance.SetMaxHealth(maxHealth);
        }
    }

    void Update()
    {
        // 1. Xử lý trạng thái trúng đòn (Hit Stun) và bị đỏ lè
        if (hitStunTimer > 0)
        {
            hitStunTimer -= Time.deltaTime;
            movement = Vector2.zero; // Ngăn không cho chạy

            if (hitStunTimer <= 0)
            {
                // Khi hết thời gian khựng, trả lại màu zin và tốc độ animation bình thường
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
                if (animator != null) animator.speed = 1f;
            }
            
            return; // Khúc này rất quan trọng: chặn hoàn toàn việc đọc phím chạy và bắn ở dưới!
        }

        // Nhận diện phím bấm thô trước khi xử lý
        float hInput = Input.GetAxisRaw("Horizontal");
        float vInput = Input.GetAxisRaw("Vertical");

        // Khóa di chuyển nếu Player đang trong thời gian bắn
        WeaponController weaponController = GetComponent<WeaponController>();
        bool isShooting = weaponController != null && weaponController.IsCurrentlyShooting();

        if (isShooting)
        {
            // Bị đứng im khi bắn
            movement = Vector2.zero;
        }
        else
        {
            // Lấy thông tin bàn phím (W, A, S, D) khi không bắn
            movement.x = hInput;
            movement.y = vInput;
        }

        // Cập nhật trạng thái chạy cho Animator
        isRunning = movement.magnitude > 0;
        if (animator != null)
        {
            animator.SetBool("IsRunning", isRunning);
        }

        // Lấy hướng từ input thực tế (hInput) để xoay mặt, cho phép xoay cả khi đang đứng bắn
        float flipDirection = isShooting ? hInput : movement.x;

        // Xoay mặt nhân vật sang trái/phải
        if (flipDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (flipDirection < 0 && facingRight)
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        // Nếu đã chết thì không cho di chuyển nữa
        if (currentHealth <= 0) return;

        // Tính toán vị trí mới
        Vector2 newPosition = rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime;

        // Giới hạn trục Y không cho vượt quá maxY và không thấp hơn minY
        newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        
        // Giới hạn trục X không cho vượt quá hai rìa màn hình
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Di chuyển bằng Rigidbody2D để đảm bảo vật lý chính xác
        rb.MovePosition(newPosition);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1; // Đảo ngược trục X để lật hình
        transform.localScale = scaler;
    }

    // Hàm nhận sát thương cho Player
    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return; // Đã chết rồi thì bỏ qua

        currentHealth -= damage;
        Debug.Log("Player bị trúng đạn! Máu còn: " + currentHealth);

        // Xử lý hiệu ứng hình ảnh (Muzzle) và nhấp nháy Đỏ 1 tư thế
        if (currentHealth > 0)
        {
            // 1. Ép Player nhảy về frame số 0 của animation Idle và ĐÓNG BĂNG luôn tốc độ lại
            if (animator != null)
            {
                animator.Play(idleAnimationName, 0, 0f); 
                animator.speed = 0f; // Khóa chết tốc độ ở 0
            }

            // 2. Lấy thùng sơn sơn lại nguyên con sang màu đỏ lè
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red; 
            }

            // Gán lại bộ đếm để Update() bắt đầu đếm ngược thời gian hồi màu
            hitStunTimer = hitStunDuration;

            // 3. Sinh ra khối Muzzle nổ ngay tại ngực Player
            if (hitMuzzlePrefab != null)
            {
                Instantiate(hitMuzzlePrefab, transform.position, Quaternion.identity);
            }
        }

        // Cập nhật lên UI
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Hàm hồi máu cho Player (gọi từ HealthPickup)
    public void Heal(int amount)
    {
        if (currentHealth <= 0) return; // Đã chết thì không thể hồi

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth); // Không vượt quá máu tối đa
        Debug.Log("Player được hồi " + amount + " máu! Máu hiện tại: " + currentHealth);

        // Cập nhật UI thanh máu
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Die()
    {
        Debug.Log("Player đã CHẾT! GAME OVER!");
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Tắt khả năng bắn súng
        WeaponController weaponController = GetComponent<WeaponController>();
        if (weaponController != null) weaponController.enabled = false;

        // Tắt va chạm 
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Tắt script di chuyển này
        this.enabled = false;

        // Hiện bảng Game Over
        if (UIManager.instance != null)
        {
            // Dùng hàm Invoke để trễ 1 chút chờ hiệu ứng ngã chết chạy xong (khoảng 1 giây)
            UIManager.instance.Invoke("ShowGameOver", 1f); 
        }
    }
}

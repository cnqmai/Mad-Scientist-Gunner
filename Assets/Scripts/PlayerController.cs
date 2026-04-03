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

    private int currentHealth;  // Khai báo máu hiện tại
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private bool facingRight = true;

    private bool isRunning;

    void Start()
    {
        currentHealth = maxHealth; // Khởi tạo máu đầu game
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Tìm Animator
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

        // Kích hoạt animation bị thương (chớp màu đỏ)
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger("GetHit");
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

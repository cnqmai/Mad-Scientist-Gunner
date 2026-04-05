using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform firePoint; // Vị trí nòng súng để bắn đạn
    
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public Sprite weaponIcon;       // Hình ảnh của cẩu súng để hiển thị lên UI
        public RuntimeAnimatorController weaponAnimator; // Animator (hình dạng Player) ứng với súng này
        public GameObject bulletPrefab; // Prefab viên đạn của vũ khí này
        public float fireRate = 0.5f;   // Tốc độ bắn (giây)
        public GameObject muzzleFlashPrefab; // Prefab hiệu ứng lửa đầu nòng
        public bool isAutomatic = true; // Bật để cho phép giữ phím bắn liên tục (như súng tiểu liên)
        
        [Header("Laser Setup")]
        public bool isContinuousLaser = false; // Bật nếu vũ khí này là Laser đứng yên
        public Vector3 firePointOffset; // Chỉnh lệch nòng súng (VD: trục X nhích 0.5, Y nhích 0.1)

        [Header("Audio Setup")]
        public AudioClip shootSound;
    }

    public Weapon[] weapons;
    private int currentWeaponIndex = 0;
    private float nextFireTime = 0f;
    private float shootBlockTimer = 0f; // Bộ đếm thời gian khóa di chuyển khi bắn
    private Animator animator;
    private AudioSource audioSource;
    private GameObject activeContinuousLaser; // Lưu trữ tia laser đang xuất hiện

    void Start()
    {
        animator = GetComponent<Animator>(); // Tìm Animator của Player

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Cập nhật UI và hình dáng Player cầm súng đầu tiên ngay khi vào game
        if (weapons.Length > 0)
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.UpdateWeaponIcon(weapons[0].weaponIcon);
            }
            if (animator != null && weapons[0].weaponAnimator != null)
            {
                animator.runtimeAnimatorController = weapons[0].weaponAnimator;
            }
        }
    }

    void Update()
    {
        // Chuyển vũ khí bằng phím Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon();
        }

        if (weapons.Length > 0)
        {
            Weapon currentWeapon = weapons[currentWeaponIndex];

            // Nếu là siêu vũ khí Laser bắn ra 1 tia đứng yên dài liên tục
            if (currentWeapon.isContinuousLaser)
            {
                if (Input.GetKeyDown(KeyCode.K))
                {
                    // QUAN TRỌNG: Kích hoạt Trigger Shoot cho Laser để nó chuyển sang trạng thái Player_Shoot
                    if (animator != null)
                    {
                        animator.SetTrigger("Shoot");
                    }

                    // Phát âm thanh Laser khi bắt đầu nhấn phím K [THÊM MỚI]
                    PlayWeaponSound(currentWeapon.shootSound);

                    // Vừa bấm tạo ra tia laser và gắn vào dưới FirePoint
                    if (activeContinuousLaser == null && currentWeapon.bulletPrefab != null)
                    {
                        // TransformPoint sẽ tự động bao gồm cả tỷ lệ co giãn lật mặt (Scale.x = -1) nên tọa độ Offset sẽ chuẩn mốc
                        Vector3 actualFirePoint = firePoint.TransformPoint(currentWeapon.firePointOffset);

                        // Vì tia Laser là CON (Child) của nòng súng, nó tự động thừa kế lật mặt Scale.
                        // Do đó, KHÔNG tự quay nó 180 độ nữa nếu không nó sẽ bị lật ngược 2 lần.
                        activeContinuousLaser = Instantiate(currentWeapon.bulletPrefab, actualFirePoint, firePoint.rotation, firePoint);
                    }
                }
                else if (Input.GetKeyUp(KeyCode.K))
                {
                    // Thả phím ra thì xóa tia laser
                    if (activeContinuousLaser != null) Destroy(activeContinuousLaser);

                    // Nếu muốn âm thanh Laser dừng ngay lập tức khi thả phím, hãy dùng:
                    audioSource.Stop();
                }
            }
            else
            {
                // Súng bình thường (cho giữ phím hoặc bấm 1 phát)
                bool tryToShoot = currentWeapon.isAutomatic ? Input.GetKey(KeyCode.K) : Input.GetKeyDown(KeyCode.K);

                if (tryToShoot)
                {
                    Shoot();
                }
            }

            // Truyền trạng thái giữ phím vào Animator (Dùng boolean IsShooting cho súng laser cần tiếp tục animation khi giữ)
            if (animator != null)
            {
                // Bật cờ "IsShooting" trong Animator khi người chơi đang giữ nút K (Laser auto lặp animation)
                bool isHoldingFire = (currentWeapon.isAutomatic || currentWeapon.isContinuousLaser) && Input.GetKey(KeyCode.K);
                animator.SetBool("IsShooting", isHoldingFire);
            }
        }
    }

    void SwitchWeapon()
    {
        if (weapons.Length == 0) return;
        
        // Nếu đang bắn tia laser mà lỡ đổi súng thì xóa tia laser kia đi
        if (activeContinuousLaser != null) Destroy(activeContinuousLaser);

        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        Debug.Log("Đã chuyển sang vũ khí: " + weapons[currentWeaponIndex].weaponName);
        
        // Cập nhật biểu tượng súng trên UI
        if (UIManager.instance != null && weapons[currentWeaponIndex].weaponIcon != null)
        {
            UIManager.instance.UpdateWeaponIcon(weapons[currentWeaponIndex].weaponIcon);
        }

        // Thay đổi toàn bộ bộ Animation của Player (để cầm súng tương ứng)
        if (animator != null && weapons[currentWeaponIndex].weaponAnimator != null)
        {
            animator.runtimeAnimatorController = weapons[currentWeaponIndex].weaponAnimator;
        }
    }

    void Shoot()
    {
        if (weapons.Length == 0) return;

        if (Time.time >= nextFireTime)
        {
            Weapon currentWeapon = weapons[currentWeaponIndex];
            
            // Kích hoạt animation bắn 
            if (animator != null)
            {
                animator.SetTrigger("Shoot");
            }

            // Phát âm thanh bắn súng thường/tự động [THÊM MỚI]
            PlayWeaponSound(currentWeapon.shootSound);

            if (currentWeapon.bulletPrefab != null && firePoint != null)
            {
                // Chú ý: Đạn bay ra ngoài không trung (không làm con của Player) nên phải quay mặt thủ công 180 độ.
                Vector3 spawnRotation = transform.localScale.x > 0 ? Vector3.zero : new Vector3(0, 180, 0);
                
                // Cập nhật vị trí bù trừ (Offset) cho từng loại súng (TransformPoint hỗ trợ lật Offset khi Scale đảo)
                Vector3 actualFirePoint = firePoint.TransformPoint(currentWeapon.firePointOffset);

                // Instatiate đạn thường tại tọa độ lệch và KHÔNG GẮN VÀO PLAYER
                Instantiate(currentWeapon.bulletPrefab, actualFirePoint, Quaternion.Euler(spawnRotation));

                // Sinh ra hiệu ứng tia lửa nòng súng nếu có
                if (currentWeapon.muzzleFlashPrefab != null)
                {
                    Instantiate(currentWeapon.muzzleFlashPrefab, actualFirePoint, Quaternion.Euler(spawnRotation), firePoint);
                }
            }
            
            nextFireTime = Time.time + currentWeapon.fireRate;
            shootBlockTimer = Time.time + currentWeapon.fireRate; // Khóa di chuyển theo thời gian FireRate
        }
    }

    // Hàm bổ trợ để phát âm thanh [THÊM MỚI]
    void PlayWeaponSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Hàm gọi từ bên ngoài để kiểm tra xem Player có đang trong tư thế bắn hay không
    public bool IsCurrentlyShooting()
    {
        if (weapons.Length == 0) return false;
        Weapon currentWeapon = weapons[currentWeaponIndex];

        // Nếu là súng Laser đứng yên, Player bị khóa di chuyển miễn là còn giữ nút K
        if (currentWeapon.isContinuousLaser)
        {
            return Input.GetKey(KeyCode.K);
        }
        else
        {
            // Các súng khác: Khóa di chuyển theo ngưỡng thời gian xả đạn (fireRate)
            return Time.time < shootBlockTimer;
        }
    }
}

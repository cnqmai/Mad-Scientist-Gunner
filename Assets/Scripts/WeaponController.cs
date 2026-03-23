using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform firePoint; // Vị trí nòng súng để bắn đạn
    
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public Sprite weaponIcon;       // Hình ảnh của cẩu súng để hiển thị lên UI
        public GameObject bulletPrefab; // Prefab viên đạn của vũ khí này
        public float fireRate = 0.5f;   // Tốc độ bắn (giây)
        public GameObject muzzleFlashPrefab; // Prefab hiệu ứng lửa đầu nòng
    }

    public Weapon[] weapons;
    private int currentWeaponIndex = 0;
    private float nextFireTime = 0f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>(); // Tìm Animator của Player
        
        // Cập nhật UI vũ khí đầu tiên ngay khi vào game
        if (weapons.Length > 0 && UIManager.instance != null)
        {
            UIManager.instance.UpdateWeaponIcon(weapons[0].weaponIcon);
        }
    }

    void Update()
    {
        // Chuyển vũ khí bằng phím Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchWeapon();
        }

        // Bắn súng bằng phím K (cho phép giữ phím nếu muốn, dùng GetKey thay vì GetKeyDown)
        // Yêu cầu của bạn là ấn phím K để bắn:
        if (Input.GetKeyDown(KeyCode.K))
        {
            Shoot();
        }
    }

    void SwitchWeapon()
    {
        if (weapons.Length == 0) return;
        
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        Debug.Log("Đã chuyển sang vũ khí: " + weapons[currentWeaponIndex].weaponName);
        
        // Cập nhật biểu tượng súng trên UI
        if (UIManager.instance != null && weapons[currentWeaponIndex].weaponIcon != null)
        {
            UIManager.instance.UpdateWeaponIcon(weapons[currentWeaponIndex].weaponIcon);
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

            if (currentWeapon.bulletPrefab != null && firePoint != null)
            {
                // Chú ý: Đạn bắn ra sẽ quay mặt theo hướng của Player (thông qua firePoint.rotation)
                Vector3 spawnRotation = transform.localScale.x > 0 ? Vector3.zero : new Vector3(0, 180, 0);
                Instantiate(currentWeapon.bulletPrefab, firePoint.position, Quaternion.Euler(spawnRotation));

                // Sinh ra hiệu ứng tia lửa nòng súng nếu có
                if (currentWeapon.muzzleFlashPrefab != null)
                {
                    Instantiate(currentWeapon.muzzleFlashPrefab, firePoint.position, Quaternion.Euler(spawnRotation), firePoint);
                }
            }
            
            nextFireTime = Time.time + currentWeapon.fireRate;
        }
    }
}

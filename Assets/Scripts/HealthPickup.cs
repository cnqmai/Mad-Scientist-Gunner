using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public enum PotionType { Green, Blue, Red, Pink }

    [Header("Loại Potion")]
    public PotionType potionType = PotionType.Green;

    [Header("Lượng hồi máu theo loại")]
    public int greenHeal = 30;  // Xanh lá: hồi nhiều
    public int pinkHeal  = 20;  // Hồng: hồi khá
    public int blueHeal  = 25;  // Xanh dương: hồi vừa
    public int redHeal   = 15;  // Đỏ: hồi ít

    [Header("Tự biến mất sau X giây")]
    public float lifetime = 8f;

    // --- Private ---
    private Vector3 startPos;
    private int healAmount;

    void Start()
    {
        startPos = transform.position;
        Destroy(gameObject, lifetime);

        // Xác định lượng hồi máu dựa theo loại
        switch (potionType)
        {
            case PotionType.Green: healAmount = greenHeal; break;
            case PotionType.Pink:  healAmount = pinkHeal;  break;
            case PotionType.Blue:  healAmount = blueHeal;  break;
            case PotionType.Red:   healAmount = redHeal;   break;
        }

        // Đảm bảo Collider là Trigger để Player có thể lụm được
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Heal(healAmount);
            Debug.Log("Player nh\u1eb7t Potion " + potionType + " h\u1ed3i " + healAmount + " m\u00e1u!");
        }

        Destroy(gameObject);
    }
}

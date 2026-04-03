using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MuzzleFlash : MonoBehaviour
{
    public Sprite[] flashSprites; // Kéo thả các hình ảnh tia lửa (skeleton-animation 0-5) vào đây
    public float frameRate = 0.02f; // Tốc độ trễ giữa các khung hình (giây)

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (flashSprites.Length > 0)
        {
            spriteRenderer.sprite = flashSprites[0];
        }
    }

    void Update()
    {
        if (flashSprites == null || flashSprites.Length == 0) return;

        timer += Time.deltaTime;

        // Chuyển sang frame tiếp theo khi đủ thời gian
        if (timer >= frameRate)
        {
            timer -= frameRate;
            currentFrame++;

            // Nếu đã chạy hết mảng hình thì tự động hủy GameObject
            if (currentFrame >= flashSprites.Length)
            {
                Destroy(gameObject);
            }
            else
            {
                spriteRenderer.sprite = flashSprites[currentFrame];
            }
        }
    }
}

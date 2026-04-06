using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // Kéo Player vào ô này
    public Vector3 offset = new Vector3(0, 0, -10f); // Khoảng cách giữa Camera và Player (Z phải luôn âm)
    public float smoothSpeed = 0.125f; // Tốc độ lướt mượt mà của camera (0 = đứng im, 1 = bám chặt)

    public bool lockYAxis = false; // Tick vào nếu không muốn Camera nhảy lên cao khi Player nhảy/bay

    public bool enableClamp = false; // Bật giới hạn góc nhìn camera
    public Vector2 minBounds; // Tọa độ nhỏ nhất (Góc dưới bên trái giới hạn)
    public Vector2 maxBounds; // Tọa độ lớn nhất (Góc trên bên phải giới hạn)

    void LateUpdate()
    {
        // Kiểm tra xem đã gán mục tiêu chưa
        if (target == null) return;

        // Vị trí lý tưởng mà Camera cần đi tới
        Vector3 desiredPosition = target.position + offset;

        // Tùy chọn: Khóa trục Y (Chỉ cho camera đi ngang qua trái/phải, không chạy lên dọc)
        // Rất hữu dụng trong các game màn hình ngang hành lang giống hình của bạn
        if (lockYAxis)
        {
            desiredPosition.y = transform.position.y; // Giữ nguyên độ cao Y của camera hiện tại
        }

        // Dùng SmoothDamp hoặc Lerp để tạo cảm giác lướt camera nhịp nhàng (không bị cứng ngắc)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Căn chỉnh giới hạn di chuyển camera nếu bật
        if (enableClamp)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
        }

        // Cập nhật vị trí mới cho Camera
        transform.position = smoothedPosition;
    }
}

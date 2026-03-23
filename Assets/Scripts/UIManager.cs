using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm thư viện TextMeshPro

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("UI Player Health")]
    public Image fillImage; // Kéo Health_Fill vào ô này, không cần Slider nữa

    [Header("Tùy chọn: Màu đổi khi gần chết (Tick nếu muốn)")]
    public bool changeColorOnLowHealth = false;
    public Color highHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SetMaxHealth(int maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f; // Mức 1 = 100% đầy máu
            if (changeColorOnLowHealth) fillImage.color = highHealthColor;
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (fillImage != null)
        {
            // Tính số phần trăm (Từ 0.0 đến 1.0)
            float healthPercent = (float)currentHealth / maxHealth;
            fillImage.fillAmount = healthPercent;

            if (changeColorOnLowHealth)
            {
                fillImage.color = Color.Lerp(lowHealthColor, highHealthColor, healthPercent);
            }
        }
    }

    [Header("UI Weapon & Kills")]
    public TextMeshProUGUI killsText;   // Chữ hiển thị KILLS
    public Image weaponIcon;            // Hình ảnh súng đang cầm

    [Header("UI Game Over")]
    public GameObject gameOverPanel; // Kéo thả Panel Game Over vào đây
    public TextMeshProUGUI scoreText;           // Chữ hiện điểm số (hoặc TextMeshProUGUI)
    public TextMeshProUGUI highScoreText;       // Chữ hiện điểm cao nhất

    private int currentScore = 0;
    private int currentKills = 0;

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIcon != null && icon != null)
        {
            weaponIcon.sprite = icon;
        }
    }

    // Hàm gọi khi Player chết
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // Bật bảng Game Over lên
            
            // Lấy Highscore từ bộ nhớ máy
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            
            // Nếu điểm hiện tại cao hơn Highscore thì lưu lại
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
            }

            // Ghi điểm lên bảng
            if (scoreText != null) scoreText.text = "SCORE: " + currentScore.ToString();
            if (highScoreText != null) highScoreText.text = "HIGHSCORE: " + highScore.ToString();
        }
    }

    // Hàm gọi để cộng điểm khi giết quái
    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    // Hàm gọi để cộng thêm 1 Kill
    public void AddKill()
    {
        currentKills++;
        if (killsText != null)
        {
            killsText.text = "KILLS: " + currentKills.ToString();
        }
    }

    // Hàm gọi khi bấm nút "PLAY AGAIN"
    public void RestartGame()
    {
        // Tải lại Scene hiện tại
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}

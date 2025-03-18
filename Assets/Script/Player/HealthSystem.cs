using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public float health = 200f; // Máu của người chơi
    public Slider healthSlider; // Tham chiếu đến Slider UI

    private float maxHealth; // Lưu trữ giá trị máu tối đa
    public GameManager gameManager;
    void Start()
    {
        // Lấy giá trị máu tối đa từ GameManager (nếu bạn đã thiết lập)
        if (gameManager != null)
        {
            maxHealth = gameManager.playerMaxHealth;
        }
        else
        {
            maxHealth = health; // Sử dụng giá trị khởi tạo nếu không tìm thấy GameManager
        }

        // Đảm bảo Slider được thiết lập giá trị tối đa khi bắt đầu
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health; // Khởi tạo giá trị hiển thị
        }
        else
        {
            Debug.LogError("Chưa gán Slider vào HealthSystem!");
        }
    }

    // Hàm này có thể được gọi từ các script khác (ví dụ Enemy) khi người chơi nhận sát thương
    public void TakeDamage(float damage)
    {
        health -= damage;
        // Đảm bảo giá trị máu không âm
        if (health < 0)
        {
            health = 0;
        }
        UpdateHealthUI();
    }

    // Hàm này có thể được gọi để tăng máu (ví dụ khi nhặt vật phẩm)
    public void Heal(float amount)
    {
        health += amount;
        // Đảm bảo giá trị máu không vượt quá tối đa
        if (health > maxHealth)
        {
            health = maxHealth;
        }
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = health;
        }
    }

    // Bạn cũng có thể cần một hàm để đặt lại máu (ví dụ khi bắt đầu màn chơi mới)
    public void ResetHealth()
    {
        health = maxHealth;
        UpdateHealthUI();
    }
}

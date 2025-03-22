using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    public float health = 200f; 
    public Slider healthSlider; 

    private float maxHealth; 
    public GameManager gameManager;
    void Start()
    {
        if (gameManager != null)
        {
            maxHealth = gameManager.playerMaxHealth;
        }
        else
        {
            maxHealth = health; 
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
        else
        {
            Debug.LogError("Chưa gán Slider vào HealthSystem!");
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
        }
        UpdateHealthUI();
    }

    public void Heal(float amount)
    {
        health += amount;
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

    public void ResetHealth()
    {
        health = maxHealth;
        UpdateHealthUI();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab; 
    public Transform[] spawnPoints; 
    public Transform player; 
    public MoneySystem moneySystem; 
    public TextMeshProUGUI timerText; 
    public TextMeshProUGUI killCountText; 
    public GameObject winPanel; 
    public GameObject losePanel; 
    public float gameDuration = 70f; 
    private float timer;
    private int enemiesKilled = 0;
    private bool gameEnded = false;
    public int enemiesToKill = 5;
    public int playerMaxHealth = 200;
    private float playerCurrentHealth;

    public HealthSystem playerHealthSystem;
    public Button replayLoserBtn;
    public Button playWinnerBtn;
    public Button homeLoserBtn;
    public Button homeWinnerBtn;

    void Start()
    {
        timer = gameDuration;
        enemiesKilled = 0;
        gameEnded = false;
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        UpdateTimerUI();
        UpdateKillCountUI();
        playerCurrentHealth = playerMaxHealth; 

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        gameEnded = false;

        SpawnNewEnemy();
        replayLoserBtn.onClick.AddListener(RestartGame);
        playWinnerBtn.onClick.AddListener(RestartGame);
        homeLoserBtn.onClick.AddListener(Home);
        homeWinnerBtn.onClick.AddListener(Home);
    }

    void Update()
    {
        if (!gameEnded)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (timer <= 0)
            {
                if (enemiesKilled >= enemiesToKill)
                {
                    gameEnded = true;
                    WinGame();
                }
                else
                {
                    gameEnded = true;
                    LoseGame(); 
                }
            }

            if (playerHealthSystem != null && playerHealthSystem.health <= 0)
            {
                gameEnded = true;
                LoseGame();
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timer).ToString();
        }
    }

    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Đã tiêu diệt: " + enemiesKilled + " / " + enemiesToKill;
        }
    }

    public void IncrementKillCount()
    {
        if (!gameEnded)
        {
            enemiesKilled++;
            UpdateKillCountUI();
        }
    }

    void WinGame()
    {
        Debug.Log("Bạn đã chiến thắng!");
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    void LoseGame()
    {
        Debug.Log("Bạn đã thua!");
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
    }

    public void SpawnNewEnemy()
    {
        if (!gameEnded)
        {
            if (enemyPrefab != null && spawnPoints.Length > 0 && player != null && moneySystem != null)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                Transform spawnPoint = spawnPoints[randomIndex];

                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                Enemy enemyScript = newEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.gameManager = this;
                    enemyScript.player = player;
                    enemyScript.moneySystem = moneySystem;
                    enemyScript.SetDamageRange(10, 20); 
                }
            }
            else
            {
                Debug.LogError("Thiếu tham chiếu trong GameManager!");
            }
        }
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartCoroutine(DelayedResume());
    }

    IEnumerator DelayedResume()
    {
        yield return new WaitForEndOfFrame(); 
        ResumeGame();
    }

    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        gameEnded = false;
    }
    private void Home()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
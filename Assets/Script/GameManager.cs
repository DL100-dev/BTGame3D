using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab của kẻ địch
    public Transform[] spawnPoints; // Mảng các điểm xuất hiện
    public Transform player; // Tham chiếu đến người chơi
    public MoneySystem moneySystem; // Tham chiếu đến hệ thống tiền tệ
    public TextMeshProUGUI timerText; // Text hiển thị thời gian
    public TextMeshProUGUI killCountText; // Text hiển thị số lần tiêu diệt kẻ địch
    public GameObject winPanel; // Panel hiển thị khi thắng
    public GameObject losePanel; // Panel hiển thị khi thua
    public float gameDuration = 70f; // Thời gian chơi (1 phút)
    private float timer;
    private int enemiesKilled = 0;
    private bool gameEnded = false;
    public int enemiesToKill = 5;
    public int playerMaxHealth = 200;
    private float playerCurrentHealth; // Giả sử điều này được quản lý ở nơi khác, nhưng chúng ta cần nó cho điều kiện thua cuộc.

    // Rất có thể bạn sẽ có một script PlayerHealth để quản lý điều này.
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
        playerCurrentHealth = playerMaxHealth; // Khởi tạo máu của người chơi

        // Thiết lập trạng thái chuột cho gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        gameEnded = false;

        // Tạo kẻ địch đầu tiên khi trò chơi bắt đầu
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
                // Khi hết thời gian, kiểm tra xem đã tiêu diệt đủ kẻ địch chưa
                if (enemiesKilled >= enemiesToKill)
                {
                    gameEnded = true;
                    WinGame();
                }
                else
                {
                    gameEnded = true;
                    LoseGame(); // Nếu hết thời gian mà không đủ kill, thì thua
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
        // Thực hiện bất kỳ hành động nào khi thắng trò chơi tại đây
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
        // Thực hiện bất kỳ hành động nào khi thua trò chơi tại đây
    }

    public void SpawnNewEnemy()
    {
        if (!gameEnded)
        {
            if (enemyPrefab != null && spawnPoints.Length > 0 && player != null && moneySystem != null)
            {
                // Chọn một điểm xuất hiện ngẫu nhiên
                int randomIndex = Random.Range(0, spawnPoints.Length);
                Transform spawnPoint = spawnPoints[randomIndex];

                // Tạo kẻ địch mới tại điểm xuất hiện
                GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

                // Lấy tham chiếu đến script Enemy và gán GameManager
                Enemy enemyScript = newEnemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.gameManager = this;
                    enemyScript.player = player;
                    enemyScript.moneySystem = moneySystem;
                    enemyScript.SetDamageRange(10, 20); // Đặt phạm vi sát thương
                }
            }
            else
            {
                Debug.LogError("Thiếu tham chiếu trong GameManager!");
            }
        }
    }
    // Hàm này sẽ được gọi khi nút Replay được nhấn
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartCoroutine(DelayedResume());
    }

    IEnumerator DelayedResume()
    {
        yield return new WaitForEndOfFrame(); // Hoặc một delay ngắn khác (ví dụ: WaitForSeconds(0.1f))
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
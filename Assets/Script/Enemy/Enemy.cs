using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using TMPro;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;
    public float detectionAngle = 45f;
    public float chaseDistance = 15f;
    public float patrolWaitTime = 2f; // Thời gian đứng yên khi tuần tra
    public float patrolRadius = 10f; // Bán kính tuần tra

    private NavMeshAgent agent;
    private Animator animator;
    private float nextAttackTime = 0f;
    private Vector3 lastKnownPlayerPosition;
    private bool playerDetected = false;
    private bool isPatrolling = true; // Thêm biến để theo dõi trạng thái tuần tra

    public float health = 100f; // Thêm biến máu
    public MoneySystem moneySystem; // Thêm tham chiếu đến hệ thống tiền tệ
    public GameManager gameManager; // Thêm tham chiếu đến GameManager
    private int minDamage = 10;
    private int maxDamage = 20;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        StartCoroutine(PatrolCoroutine()); // Bắt đầu tuần tra khi khởi tạo
    }

    void Update()
    {
        if (player == null) return;

        DetectPlayer();

        if (playerDetected)
        {
            ChasePlayer();
        }
        else if (isPatrolling)
        {
            // Tuần tra được xử lý trong coroutine
        }
    }

    public void SetDamageRange(int min, int max)
    {
        minDamage = min;
        maxDamage = max;
    }

    void DetectPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = player.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);

            if (angle <= detectionAngle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange))
                {
                    if (hit.transform == player)
                    {
                        playerDetected = true;
                        lastKnownPlayerPosition = player.position;
                        isPatrolling = false; // Dừng tuần tra khi phát hiện người chơi
                        return;
                    }
                }
            }
        }
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Attack();
        }
        else if (distanceToPlayer <= chaseDistance)
        {
            agent.SetDestination(player.position);
            if (animator != null)
            {
                animator.SetBool("IsAttacking", false);
                animator.SetBool("IsWalking", true);
            }
        }
        else
        {
            playerDetected = false;
            isPatrolling = true; // Bắt đầu lại tuần tra khi mất dấu người chơi
            StartCoroutine(PatrolCoroutine());
        }
    }

    IEnumerator PatrolCoroutine()
    {
        while (isPatrolling)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1);
            Vector3 finalPosition = hit.position;
            agent.SetDestination(finalPosition);

            if (animator != null)
            {
                animator.SetBool("IsWalking", true);
            }

            // Thêm kiểm tra ở đây
            yield return new WaitUntil(() => agent != null && agent.isActiveAndEnabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance);

            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
            }

            yield return new WaitForSeconds(patrolWaitTime);
        }
    }

    void Attack()
    {
        if (Time.time >= nextAttackTime)
        {
            Debug.Log("Kẻ địch tấn công!");
            if (animator != null)
            {
                animator.SetBool("IsAttacking", true);
                animator.SetBool("IsWalking", false);
            }

            // Gây sát thương cho người chơi (giả sử bạn có script PlayerHealth)
            if (player != null)
            {
                HealthSystem playerHealth = player.GetComponent<HealthSystem>();
                if (playerHealth != null)
                {
                    int damageToDeal = Random.Range(minDamage, maxDamage + 1);
                    Debug.Log("Kẻ địch gây " + damageToDeal + " sát thương.");
                    playerHealth.TakeDamage(damageToDeal); // Gọi hàm TakeDamage
                }
                else
                {
                    Debug.LogError("Người chơi không có script HealthSystem!");
                }
            }

            nextAttackTime = Time.time + attackCooldown;
            // Đặt lại trigger animation tấn công sau khi nó chạy (bạn có thể cần điều chỉnh thời gian chờ tùy thuộc vào thiết lập animation của bạn)
            StartCoroutine(ResetAttackAnimation());
        }
    }

    IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Điều chỉnh độ trễ này nếu cần
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    public void TakeDamage(Vector3 hitPosition, float damage)
    {
        health -= damage;
        lastKnownPlayerPosition = hitPosition;
        playerDetected = true;
        isPatrolling = false;
        Debug.Log("Kẻ địch bị trúng đạn! Máu còn lại: " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Kẻ địch đã chết!");
        // Thêm logic chết ở đây (ví dụ: animation, hiệu ứng, phá hủy GameObject)
        Destroy(gameObject);

        // Thêm tiền cho người chơi khi kẻ địch chết
        if (moneySystem != null)
        {
            moneySystem.playerMoney += 50;
            moneySystem.UpdateMoneyUI();
        }
        if (gameManager != null)
        {
            gameManager.IncrementKillCount();
            gameManager.SpawnNewEnemy();
        }
    }
}
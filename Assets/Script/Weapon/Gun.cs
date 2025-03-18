using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public abstract class Gun : MonoBehaviour
{
    public GunData gunData;
    public Transform gunMuzzle;

    [HideInInspector] public PlayerController playerController;
    [HideInInspector] public Transform cameraTransform;

    [HideInInspector] public float currentAmmo = 0f;
    [HideInInspector] public float totalAmmo = 0f; // Thêm biến tổng số đạn
    private float nextTimeToFire = 0f;

    [HideInInspector] public bool isReloading = false;
    [SerializeField] private Animator reloadAnim;
    private int reloadHash;


    // Recoil variables
    private Vector3 currentRecoilRotation;
    protected Vector3 targetRecoilRotation;

    public ParticleSystem muzzleFlash; // Thêm biến này

    public Image cross;
    public TextMeshProUGUI ammoText; // Tham chiếu đến Text hiển thị số lượng đạn

    protected bool isAiming = false;
    public GameObject bulletHolePrefab; // Prefab vết bắn

    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    public virtual void Start()
    {
        currentAmmo = gunData.magazineSize; // Khởi tạo số đạn trong băng
        totalAmmo = gunData.totalAmmo;     // Khởi tạo tổng số đạn dự trữ
        playerController = transform.root.GetComponent<PlayerController>();
        cameraTransform = playerController.mainCamera.transform;
        reloadHash = Animator.StringToHash("isReloading");
        // Ensure crosshair is initially active
        if (cross != null)
        {
            cross.gameObject.SetActive(true);
        }
        UpdateAmmoUI(); // Cập nhật UI hiển thị đạn khi bắt đầu
    }
    public virtual void Update()
    {
        // Recoil recovery
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, Time.deltaTime * gunData.recoilRecoverySpeed);
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, Time.deltaTime * gunData.recoilRecoverySpeed);
        cameraTransform.localRotation = Quaternion.Euler(currentRecoilRotation) * cameraTransform.localRotation;

        // Tắt muzzle flash khi không bắn hoặc hết đạn
        if (currentAmmo <= 0f || !IsShootingInput())
        {
            if (muzzleFlash != null && muzzleFlash.isPlaying)
            {
                muzzleFlash.Stop();
            }
        }

        DetermineAim();

        if (IsShootingInput())
        {
            TryShoot();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            TryReload();
            if (muzzleFlash != null)
            {
                muzzleFlash.Stop();
            }
        }

        // Ngừng ngắm nếu đang thay đạn và hiển thị crosshair
        if (IsReloading() && isAiming)
        {
            isAiming = false;
            if (cross != null)
            {
                cross.gameObject.SetActive(true); // Hiển thị crosshair khi không ngắm
            }
        }
    }
    public virtual void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
        else if (!isReloading && currentAmmo <= 0 && totalAmmo > 0) // Reload khi hết đạn
        {
            StartCoroutine(Reload());
        }
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log(gunData.gunName + " is Reloading...");
        reloadAnim.SetBool(reloadHash, true);
        yield return new WaitForSeconds(gunData.reloadTime);

        float bulletsToReload = gunData.magazineSize - currentAmmo;
        float bulletsCanReload = Mathf.Min(bulletsToReload, totalAmmo);
        currentAmmo += bulletsCanReload;
        totalAmmo -= bulletsCanReload;

        isReloading = false;
        Debug.Log(gunData.gunName + " is Reloaded");
        reloadAnim.SetBool(reloadHash, false);
        UpdateAmmoUI();
    }

    public void TryShoot()
    {
        if (isReloading)
        {
            Debug.Log(gunData.gunName + " is Reloading...");
            return;
        }
        if (currentAmmo <= 0f)
        {
            Debug.Log(gunData.gunName + " has no bullets left, Please reload");
            TryReload(); // Tự động thử reload khi hết đạn
            return;
        }
        if (Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + (1 / gunData.fireRate);
            HandleShoot();
        }
    }
    private void HandleShoot()
    {
        currentAmmo--;
        Debug.Log(gunData.gunName + " Shoot, bullets in magazine: " + currentAmmo + ", total bullets: " + totalAmmo);
        // Apply recoil here before shooting
        ApplyRecoil();
        // Bật muzzle flash khi bắn
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        audioManager.PlaySFX(audioManager.shootSFX);
        Shoot();
        UpdateAmmoUI(); // Cập nhật UI sau mỗi lần bắn
    }

    public virtual void Shoot()
    {
        RaycastHit hit;
        Vector3 target = Vector3.zero;
        float currentSpread = isAiming ? gunData.aimSpread : gunData.hipFireSpread;
        Vector3 fireDirection = cameraTransform.forward + Random.insideUnitSphere * currentSpread;
        fireDirection.Normalize();

        Debug.DrawRay(cameraTransform.position, fireDirection * gunData.shootingRange, Color.red, 2f);

        // Thực hiện một raycast duy nhất, kiểm tra cả layer tường và layer mục tiêu
        if (Physics.Raycast(cameraTransform.position, fireDirection, out hit, gunData.shootingRange, gunData.targetLayerMask | gunData.wallLayerMask))
        {
            target = hit.point;
            // Kiểm tra layer của vật thể bị bắn trúng
            if ((gunData.wallLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("Trung tuong");
                CreateBulletHole(hit); // Tạo vết bắn
            }
            else if ((gunData.targetLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("Trung ke dich");
                // Handle hitting the target here (e.g., apply damage)
                OnTargetHit(hit);
            }
        }
        else
        {
            target = cameraTransform.position + fireDirection * gunData.shootingRange;
        }
        StartCoroutine(BulletFire(target, hit));
    }

    protected virtual void OnTargetHit(RaycastHit hit)
    {
        // Kiểm tra xem vật thể bị bắn trúng có phải là Enemy không
        Enemy enemy = hit.collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Gọi phương thức TakeDamage() của Enemy và truyền sát thương
            enemy.TakeDamage(hit.point, gunData.damage);
        }
    }

    private void CreateBulletHole(RaycastHit hit)
    {
        if (bulletHolePrefab != null)
        {
            Debug.Log("Bullet hole created at: " + hit.point);
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            bulletHole.transform.parent = hit.collider.transform;
            Destroy(bulletHole, 3f);
            Debug.Log("Bullet hole created!");
        }
    }

    private IEnumerator BulletFire(Vector3 target, RaycastHit hit)
    {
        if (gunData.bulletTrailPrefab != null)
        {
            GameObject bulletTrail = Instantiate(gunData.bulletTrailPrefab, gunMuzzle.position, Quaternion.identity);
            while (bulletTrail != null && Vector3.Distance(bulletTrail.transform.position, target) > 0.1f)
            {
                bulletTrail.transform.position = Vector3.MoveTowards(bulletTrail.transform.position, target, Time.deltaTime * gunData.bulletSpeed);
                yield return null;
            }
            Destroy(bulletTrail);
        }
    }

    private void DetermineAim()
    {
        if (!IsReloading()) // Chỉ cho phép ngắm nếu không đang thay đạn
        {
            if (Input.GetMouseButtonDown(1))
            {
                isAiming = !isAiming;
                if (cross != null)
                {
                    cross.gameObject.SetActive(!isAiming); // Ẩn crosshair khi ngắm, hiện khi không ngắm
                }
            }
        }
        else if (isAiming)
        {
            // Đảm bảo tắt ngắm khi bắt đầu thay đạn (nếu chưa tắt)
            isAiming = false;
            if (cross != null)
            {
                cross.gameObject.SetActive(true); // Đảm bảo crosshair hiện khi hết ngắm do reload
            }
        }

        Vector3 targetPosition = isAiming ? gunData.aimingLocalPosition : gunData.normalLocalPosition;
        Vector3 desiredPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * gunData.aimSmooting);
        transform.localPosition = desiredPosition;
    }
    public virtual void ResetAim()
    {
        isAiming = false;
        if (cross != null)
        {
            cross.gameObject.SetActive(true);
        }
    }
    public virtual void ApplyRecoil()
    {
        targetRecoilRotation += new Vector3(-gunData.recoilForceUpward, Random.Range(-5f, 5f), 0f);
        transform.localPosition -= Vector3.forward * gunData.recoilForceBackward;
    }
    // Hàm ảo để kiểm tra input bắn (sẽ được override trong lớp con)
    protected virtual bool IsShootingInput()
    {
        return false; // Mặc định là không có input bắn
    }
    public bool IsReloading()
    {
        return isReloading;
    }

    // Hàm để cập nhật UI hiển thị số lượng đạn
    public void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + totalAmmo;
        }
    }
}
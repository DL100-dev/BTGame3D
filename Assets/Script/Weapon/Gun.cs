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
    [HideInInspector] public float totalAmmo = 0f; 
    private float nextTimeToFire = 0f;

    [HideInInspector] public bool isReloading = false;
    [SerializeField] private Animator reloadAnim;
    private int reloadHash;


    private Vector3 currentRecoilRotation;
    protected Vector3 targetRecoilRotation;

    public ParticleSystem muzzleFlash; 

    public Image cross;
    public TextMeshProUGUI ammoText; 

    protected bool isAiming = false;
    public GameObject bulletHolePrefab; 

    AudioManager audioManager;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    public virtual void Start()
    {
        currentAmmo = gunData.magazineSize; 
        totalAmmo = gunData.totalAmmo;     
        playerController = transform.root.GetComponent<PlayerController>();
        cameraTransform = playerController.mainCamera.transform;
        reloadHash = Animator.StringToHash("isReloading");
        if (cross != null)
        {
            cross.gameObject.SetActive(true);
        }
        UpdateAmmoUI(); 
    }
    public virtual void Update()
    {
        targetRecoilRotation = Vector3.Lerp(targetRecoilRotation, Vector3.zero, Time.deltaTime * gunData.recoilRecoverySpeed);
        currentRecoilRotation = Vector3.Slerp(currentRecoilRotation, targetRecoilRotation, Time.deltaTime * gunData.recoilRecoverySpeed);
        cameraTransform.localRotation = Quaternion.Euler(currentRecoilRotation) * cameraTransform.localRotation;

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

        if (IsReloading() && isAiming)
        {
            isAiming = false;
            if (cross != null)
            {
                cross.gameObject.SetActive(true); 
            }
        }
    }
    public virtual void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize && totalAmmo > 0)
        {
            StartCoroutine(Reload());
        }
        else if (!isReloading && currentAmmo <= 0 && totalAmmo > 0) 
        {
            StartCoroutine(Reload());
        }
    }

    public IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log(gunData.gunName + " đang nạp đạn...");
        reloadAnim.SetBool(reloadHash, true);
        yield return new WaitForSeconds(gunData.reloadTime);

        float bulletsToReload = gunData.magazineSize - currentAmmo;
        float bulletsCanReload = Mathf.Min(bulletsToReload, totalAmmo);
        currentAmmo += bulletsCanReload;
        totalAmmo -= bulletsCanReload;

        isReloading = false;
        Debug.Log(gunData.gunName + " đã nạp");
        reloadAnim.SetBool(reloadHash, false);
        UpdateAmmoUI();
    }

    public void TryShoot()
    {
        if (isReloading)
        {
            Debug.Log(gunData.gunName + " đang nạp đạn...");
            return;
        }
        if (currentAmmo <= 0f)
        {
            Debug.Log(gunData.gunName + " hết đạn, hãy nạp đạn");
            TryReload(); 
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
        ApplyRecoil();
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
        audioManager.PlaySFX(audioManager.shootSFX);
        Shoot();
        UpdateAmmoUI(); 
    }

    public virtual void Shoot()
    {
        RaycastHit hit;
        Vector3 target = Vector3.zero;
        float currentSpread = isAiming ? gunData.aimSpread : gunData.hipFireSpread;
        Vector3 fireDirection = cameraTransform.forward + Random.insideUnitSphere * currentSpread;
        fireDirection.Normalize();

        Debug.DrawRay(cameraTransform.position, fireDirection * gunData.shootingRange, Color.red, 2f);

        if (Physics.Raycast(cameraTransform.position, fireDirection, out hit, gunData.shootingRange, gunData.targetLayerMask | gunData.wallLayerMask))
        {
            target = hit.point;
            if ((gunData.wallLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("Trung tuong");
                CreateBulletHole(hit); 
            }
            else if ((gunData.targetLayerMask.value & (1 << hit.collider.gameObject.layer)) != 0)
            {
                Debug.Log("Trung ke dich");
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
        Enemy enemy = hit.collider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(hit.point, gunData.damage);
        }
    }

    private void CreateBulletHole(RaycastHit hit)
    {
        if (bulletHolePrefab != null)
        {
            GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            bulletHole.transform.parent = hit.collider.transform;
            Destroy(bulletHole, 3f);
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
        if (!IsReloading()) 
        {
            if (Input.GetMouseButtonDown(1))
            {
                isAiming = !isAiming;
                if (cross != null)
                {
                    cross.gameObject.SetActive(!isAiming); 
                }
            }
        }
        else if (isAiming)
        {
            isAiming = false;
            if (cross != null)
            {
                cross.gameObject.SetActive(true); 
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
    protected virtual bool IsShootingInput()
    {
        return false; 
    }
    public bool IsReloading()
    {
        return isReloading;
    }

    public void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + totalAmmo;
        }
    }
}
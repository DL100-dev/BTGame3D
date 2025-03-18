using UnityEngine;

[CreateAssetMenu(fileName = "GunData", menuName = "Scriptable Objects/GunData")]
public class GunData : ScriptableObject
{
    public string gunName;
    public LayerMask targetLayerMask;
    public LayerMask wallLayerMask;

    [Header("Fire Config")]
    public float shootingRange;
    public float fireRate;

    [Header("Ammo Config")]
    public float magazineSize = 30f; // Kích thước băng đạn
    public float totalAmmo = 90f; // Tổng số đạn dự trữ

    [Header("Reload Config")]
    public float reloadTime = 2f; // Thời gian nạp đạn

    [Header("Aim")]
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;

    [Header("VFX")]
    public GameObject bulletTrailPrefab;
    public float bulletSpeed;

    public float aimSmooting = 10f;

    [Header("Recoil")]
    public float recoilForceUpward = 2f; // Lực giật lên trên
    public float recoilForceBackward = 0.5f; // Lực giật về sau
    public float recoilRecoverySpeed = 10f; // Tốc độ phục hồi recoil

    [Header("Accuracy")]
    [Range(0f, 1f)]
    public float hipFireSpread = 0.1f; // Độ lan rộng khi bắn thường (hip-fire)
    [Range(0f, 1f)]
    public float aimSpread = 0.01f; // Độ lan rộng khi ngắm bắn (ADS)

    [Header("Damage")]
    public float damage = 10f; // Thêm biến sát thương
}
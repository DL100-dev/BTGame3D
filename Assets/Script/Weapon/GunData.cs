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
    public float magazineSize = 30f; 
    public float totalAmmo = 90f; 

    [Header("Reload Config")]
    public float reloadTime = 2f; 

    [Header("Aim")]
    public Vector3 normalLocalPosition;
    public Vector3 aimingLocalPosition;

    [Header("VFX")]
    public GameObject bulletTrailPrefab;
    public float bulletSpeed;

    public float aimSmooting = 10f;

    [Header("Recoil")]
    public float recoilForceUpward = 2f; 
    public float recoilForceBackward = 0.5f;
    public float recoilRecoverySpeed = 10f; 

    [Header("Accuracy")]
    [Range(0f, 1f)]
    public float hipFireSpread = 0.1f;
    [Range(0f, 1f)]
    public float aimSpread = 0.01f; 

    [Header("Damage")]
    public float damage = 10f; 
}
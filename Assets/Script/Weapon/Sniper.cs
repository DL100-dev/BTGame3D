using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class Sniper : Gun
{
    public float zoomedInFOV = 10f;
    public float zoomTransitionSpeed = 15f;
    private float normalFOV;
    private CinemachineCamera virtualCamera;
    private bool isAimingActive = false;
    public GameObject spoer;
    public Image SniperAim;

    protected override bool IsShootingInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    public override void Start()
    {
        base.Start();
        virtualCamera = playerController.GetComponentInChildren<CinemachineCamera>();
        if (virtualCamera == null)
        {
            Debug.LogError("Không tìm thấy Cinemachine Virtual Camera trên người chơi!");
            enabled = false;
            return;
        }
        normalFOV = virtualCamera.Lens.FieldOfView;
    }

    public override void Update()
    {
        base.Update();
        HandleZoom();
    }

    void HandleZoom()
    {
        if (!IsReloading())
        {
            if (Input.GetMouseButtonDown(1))
            {
                isAimingActive = !isAimingActive;
                if (isAimingActive)
                {
                    isAiming = true;
                    if (cross != null)
                    {
                        cross.gameObject.SetActive(false);
                        spoer.SetActive(false);
                        SniperAim.gameObject.SetActive(true);
                    }
                }
                else
                {
                    isAiming = false;
                    if (cross != null)
                    {
                        cross.gameObject.SetActive(true);
                        spoer.SetActive(true);
                        SniperAim.gameObject.SetActive(false);
                    }
                }
            }

            if (isAimingActive)
            {
                virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, zoomedInFOV, Time.deltaTime * zoomTransitionSpeed);
            }
            else
            {
                virtualCamera.Lens.FieldOfView = Mathf.Lerp(virtualCamera.Lens.FieldOfView, normalFOV, Time.deltaTime * zoomTransitionSpeed);
            }
        }
        else if (isAiming)
        {
            isAimingActive = false;
            isAiming = false;
            virtualCamera.Lens.FieldOfView = normalFOV;
            if (cross != null)
            {
                cross.gameObject.SetActive(true);
            }
        }
    }

    public override void ResetAim()
    {
        base.ResetAim();
        isAimingActive = false;
        if (virtualCamera != null)
        {
            virtualCamera.Lens.FieldOfView = normalFOV;
        }
        if (spoer != null) spoer.SetActive(true);
        if (SniperAim != null) SniperAim.gameObject.SetActive(false);
    }

    public override void ApplyRecoil()
    {
        targetRecoilRotation += new Vector3(-gunData.recoilForceUpward * 3f, Random.Range(-10f, 10f), 0f);
        transform.localPosition -= Vector3.forward * gunData.recoilForceBackward * 2f;
    }

    public override void TryReload()
    {
        if (!isReloading && currentAmmo < gunData.magazineSize && totalAmmo > 0)
        {
            isAimingActive = false;
            isAiming = false;
            if (virtualCamera != null)
            {
                virtualCamera.Lens.FieldOfView = normalFOV;
            }
            if (cross != null)
            {
                cross.gameObject.SetActive(true);
                if (spoer != null) spoer.SetActive(true);
                if (SniperAim != null) SniperAim.gameObject.SetActive(false);
            }
            StartCoroutine(Reload());
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunShopManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Button pistolButton;
    public Button rifleButton;
    public Button sniperButton;
    public Button reloadButton; // Thêm tham chiếu đến nút hồi đạn
    public TextMeshProUGUI ammoText;

    [Header("Gun Prices")]
    public int pistolPrice = 100;
    public int riflePrice = 300;
    public int sniperPrice = 500;
    public int reloadPrice = 50; // Giá hồi đạn

    [Header("Gun GameObjects")]
    public GameObject pistolGameObject;
    public GameObject rifleGameObject;
    public GameObject sniperGameObject;

    private GameObject currentActiveGun;
    private bool isShopOpen = false;

    // Thêm tham chiếu đến MoneySystem
    public MoneySystem moneySystem;

    void Start()
    {
        shopPanel.SetActive(false);

        if (pistolGameObject != null) pistolGameObject.SetActive(false);
        if (rifleGameObject != null) rifleGameObject.SetActive(false);
        if (sniperGameObject != null) sniperGameObject.SetActive(false);

        pistolButton.onClick.AddListener(HandlePistolPurchase);
        rifleButton.onClick.AddListener(HandleRiflePurchase);
        sniperButton.onClick.AddListener(HandleSniperPurchase);
        reloadButton.onClick.AddListener(HandleReloadAmmo); // Thêm sự kiện click cho nút hồi đạn

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (ammoText != null)
        {
            ammoText.gameObject.SetActive(false);
        }

        // Cập nhật hiển thị tiền ban đầu
        moneySystem.UpdateMoneyUI();
        UpdateShopButtons();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }

        if (isShopOpen) return;

        if (Input.GetMouseButtonDown(0)) // 0 là chuột trái
        {
            FireWeapon();
        }
    }

    void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopPanel.SetActive(isShopOpen);

        if (isShopOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f;
            moneySystem.UpdateMoneyUI();
            UpdateShopButtons();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    void HandlePistolPurchase()
    {
        if (moneySystem.TryBuyWeapon(pistolPrice))
        {
            moneySystem.UpdateMoneyUI();
            Debug.Log("Pistol purchased!");
            SelectGun(pistolGameObject);
            UpdateShopButtons();
        }
        else
        {
            Debug.Log("Not enough money to purchase Pistol.");
        }
    }

    void HandleRiflePurchase()
    {
        if (moneySystem.TryBuyWeapon(riflePrice))
        {
            moneySystem.UpdateMoneyUI();
            Debug.Log("Rifle purchased!");
            SelectGun(rifleGameObject);
            UpdateShopButtons();
        }
        else
        {
            Debug.Log("Not enough money to purchase Rifle.");
        }
    }

    void HandleSniperPurchase()
    {
        if (moneySystem.TryBuyWeapon(sniperPrice))
        {
            moneySystem.UpdateMoneyUI();
            Debug.Log("Sniper purchased!");
            SelectGun(sniperGameObject);
            UpdateShopButtons();
        }
        else
        {
            Debug.Log("Not enough money to purchase Sniper.");
        }
    }

    // Thêm logic hồi đạn
    void HandleReloadAmmo()
    {
        if (currentActiveGun != null && moneySystem.playerMoney >= reloadPrice)
        {
            Gun gunScript = currentActiveGun.GetComponent<Gun>();
            if (gunScript != null)
            {
                // Hồi đầy đạn cho súng hiện tại
                gunScript.currentAmmo = gunScript.gunData.magazineSize;
                gunScript.totalAmmo = gunScript.gunData.totalAmmo;
                gunScript.UpdateAmmoUI();
                // Trừ tiền khi hồi đạn
                moneySystem.playerMoney -= reloadPrice;
                moneySystem.UpdateMoneyUI();
                Debug.Log("Ammo reloaded!");
                // Gọi UpdateShopButtons() sau khi trừ tiền
                UpdateShopButtons();
            }
        }
        else
        {
            Debug.Log("Not enough money to reload ammo or no weapon selected.");
        }
    }

    void SelectGun(GameObject gunGameObject)
    {
        if (currentActiveGun != null)
        {
            Gun currentGunScript = currentActiveGun.GetComponent<Gun>();
            if (currentGunScript != null)
            {
                currentGunScript.ResetAim();
            }
            currentActiveGun.SetActive(false);
        }

        if (gunGameObject != null)
        {
            gunGameObject.SetActive(true);
            currentActiveGun = gunGameObject;
            ammoText.gameObject.SetActive(true);
            Gun gunScript = currentActiveGun.GetComponent<Gun>();
            if (gunScript != null)
            {
                gunScript.currentAmmo = gunScript.gunData.magazineSize;
                gunScript.totalAmmo = gunScript.gunData.totalAmmo;
                gunScript.UpdateAmmoUI();
                gunScript.isReloading = false;
            }
        }
    }

    void FireWeapon()
    {
        if (currentActiveGun != null)
        {
            Gun gunScript = currentActiveGun.GetComponent<Gun>();
            if (gunScript != null)
            {
                gunScript.TryShoot();
            }
        }
        else
        {
            Debug.Log("No weapon selected!");
        }
    }

    void UpdateShopButtons()
    {
        if (pistolButton != null)
        {
            pistolButton.interactable = moneySystem.playerMoney >= pistolPrice;
        }
        if (rifleButton != null)
        {
            rifleButton.interactable = moneySystem.playerMoney >= riflePrice;
        }
        if (sniperButton != null)
        {
            sniperButton.interactable = moneySystem.playerMoney >= sniperPrice;
        }

        // Kiểm tra xem nút hồi đạn có thể sử dụng được không
        if (reloadButton != null)
        {
            reloadButton.interactable = currentActiveGun != null && moneySystem.playerMoney >= reloadPrice;
        }
    }
}

using TMPro;
using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public int playerMoney = 500; // Số tiền ban đầu của người chơi
    public TextMeshProUGUI moneyText;

    // Hàm để cập nhật tiền khi mua súng
    public bool TryBuyWeapon(int price)
    {
        if (playerMoney >= price)
        {
            playerMoney -= price;  // Trừ tiền khi mua
            return true;  // Mua thành công
        }
        else
        {
            return false;  // Không đủ tiền
        }
    }

    // Hàm để cập nhật UI hiển thị tiền
    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: $" + playerMoney;
        }
    }
}

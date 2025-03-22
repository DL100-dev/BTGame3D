using TMPro;
using UnityEngine;

public class MoneySystem : MonoBehaviour
{
    public int playerMoney = 500; 
    public TextMeshProUGUI moneyText;

    public bool TryBuyWeapon(int price)
    {
        if (playerMoney >= price)
        {
            playerMoney -= price;  
            return true;  
        }
        else
        {
            return false; 
        }
    }

    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = "Money: $" + playerMoney;
        }
    }
}
